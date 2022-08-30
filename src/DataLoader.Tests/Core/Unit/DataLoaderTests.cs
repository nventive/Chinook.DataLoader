using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Core.Unit
{
	public class DataLoaderTests
	{
		public DataLoaderTests(ITestOutputHelper outputHelper)
		{
			DataLoaderConfiguration.LoggerFactory = LoggerFactory.Create(builder => builder
				.AddSerilog(new LoggerConfiguration()
					.WriteTo.TestOutput(outputHelper)
					.MinimumLevel.Verbose()
					.CreateLogger()
				)
			);
		}

		[Fact]
		public void Ctor_inputs_are_validated()
		{
			var name = "name";
			DataLoaderDelegate loadMethod = async (ct, request) => "result";
			var strategy = new DelegatedDataLoaderStrategy(loadMethod);
			var concurrentMode = DataLoaderConcurrentMode.CancelPrevious;
			var delegatingStrategies = new List<DelegatingDataLoaderStrategy>();
			Func<IDataLoaderState, bool> emptySelector = (_) => false;

			new DataLoader(name, strategy, concurrentMode, emptySelector);

			Assert.Throws<ArgumentNullException>(() => new DataLoader(null, strategy, concurrentMode, emptySelector));
			Assert.Throws<ArgumentNullException>(() => new DataLoader(name, null, concurrentMode, emptySelector));
			Assert.Throws<ArgumentNullException>(() => new DataLoader(name, strategy, concurrentMode, null));
		}

		[Fact]
		public void Null_trigger_cannot_be_added()
		{
			var dataloader = GetDefaultDataLoader();

			Assert.Throws<ArgumentNullException>(() => dataloader.AddTrigger(null));
		}

		[Fact]
		public void Null_trigger_cannot_be_Removed()
		{
			var dataloader = GetDefaultDataLoader();

			Assert.Throws<ArgumentNullException>(() => dataloader.RemoveTrigger(null));
		}

		[Fact]
		public void Removed_trigger_doesnt_cause_loads()
		{
			var loadCount = 0;
			var dataloader = GetDefaultDataLoader();
			var trigger = new ManualDataLoaderTrigger();

			dataloader.StateChanged += Dataloader_StateChanged;

			dataloader.AddTrigger(trigger);

			trigger.Trigger();

			loadCount.Should().Be(1);

			dataloader.RemoveTrigger(trigger);

			trigger.Trigger();

			// Should not be 2 because the trigger was removed.
			loadCount.Should().Be(1);

			void Dataloader_StateChanged(IDataLoader dataLoader, IDataLoaderState newState)
			{
				if (!newState.IsLoading)
				{
					++loadCount;
				}
			}
		}

		[Fact]
		public async Task Canceled_token_prevents_load()
		{
			var dataloader = GetDefaultDataLoader(Load);
			var ct = new CancellationToken(canceled: true);

			var result = await dataloader.Load(ct);

			result.Should().BeNull();

			Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				throw new AssertionFailedException("Load should not be called");
			}
		}

		[Fact]
		public async Task Exception_in_load_results_in_error_state()
		{
			var dataloader = GetDefaultDataLoader(Load);

			var result = await dataloader.Load(CancellationToken.None);

			result.Should().BeNull();

			dataloader.State.Data.Should().BeNull();
			dataloader.State.Error.Should().BeOfType<InvalidOperationException>();
			dataloader.State.IsLoading.Should().BeFalse();

			Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				throw new InvalidOperationException();
			}
		}

		[Fact]
		public async Task Error_from_previous_concurrent_load_isnt_reported_when_CancelPrevious()
		{
			var tcs1 = new TaskCompletionSource<bool>();
			var tcs2 = new TaskCompletionSource<bool>();

			var dataloader = GetDefaultDataLoader(Load);
			dataloader.StateChanged += Dataloader_StateChanged;

			var load1 = dataloader.Load(CancellationToken.None);
			var load2 = dataloader.Load(CancellationToken.None);

			tcs2.SetResult(true);
			tcs1.SetResult(true);

			await load1;
			await load2;

			dataloader.State.Data.Should().Be("result");
			dataloader.State.Error.Should().BeNull();

			async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				if (request.SequenceId == 0)
				{
					await tcs1.Task;
					throw new InvalidOperationException();
				}
				else
				{
					await tcs2.Task;
					return "result";
				}
			}

			void Dataloader_StateChanged(IDataLoader dataLoader, IDataLoaderState newState)
			{
				newState.Error.Should().BeNull();
			}
		}

		[Fact]
		public async Task Cancellation_works_for_CancelPrevious()
		{
			var ct = CancellationToken.None;
			var tcs1 = new TaskCompletionSource<bool>();
			var tcs2 = new TaskCompletionSource<bool>();
			var load1WasCancelled = false;
			var states = new List<IDataLoaderState>();
			var dataLoader = new DataLoader("sut", new DelegatedDataLoaderStrategy(Load), DataLoaderConcurrentMode.CancelPrevious, s => false);

			dataLoader.StateChanged += OnStateChanged;

			var load1 = dataLoader.Load(ct, new DataLoaderContext(new Dictionary<string, object> { { "sequence", 1 } }));
			var load2 = dataLoader.Load(ct, new DataLoaderContext(new Dictionary<string, object> { { "sequence", 2 } }));

			tcs2.SetResult(true);
			tcs1.SetResult(false);

			await load2;
			await load1;

			// Make sure the first load was cancelled by the second.
			Assert.True(load1WasCancelled);

			// Make sure the DataLoader didn't produce any error.
			Assert.True(states.All(s => s.Error == null));

			async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				if ((int)request.Context["sequence"] == 1)
				{
					ct.Register(() => load1WasCancelled = true);
					await tcs1.Task;
				}
				else
				{
					await tcs2.Task;
				}

				return request.Context["sequence"];
			}

			void OnStateChanged(IDataLoader dataLoader, IDataLoaderState newState)
			{
				states.Add(newState);
			}
		}

		[Fact]
		public async Task Cancellation_works_for_DiscardNew()
		{
			var ct = CancellationToken.None;
			var tcs1 = new TaskCompletionSource<bool>();
			var tcs2 = new TaskCompletionSource<bool>();
			var load2WasInvoked = false;
			var states = new List<IDataLoaderState>();
			var dataLoader = new DataLoader("sut", new DelegatedDataLoaderStrategy(Load), DataLoaderConcurrentMode.DiscardNew, s => false);

			dataLoader.StateChanged += OnStateChanged;

			var load1 = dataLoader.Load(ct, new DataLoaderContext(new Dictionary<string, object> { { "sequence", 1 } }));
			var load2 = dataLoader.Load(ct, new DataLoaderContext(new Dictionary<string, object> { { "sequence", 2 } }));

			tcs2.SetResult(false);
			tcs1.SetResult(true);

			await load2;
			await load1;

			// Make sure the second load was not invoked because the first load was still executing at that moment.
			Assert.False(load2WasInvoked);

			// Make sure the DataLoader didn't produce any error.
			Assert.True(states.All(s => s.Error == null));

			async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				if ((int)request.Context["sequence"] == 1)
				{
					await tcs1.Task;
				}
				else
				{
					load2WasInvoked = true;
					await tcs2.Task;
				}

				return request.Context["sequence"];
			}

			void OnStateChanged(IDataLoader dataLoader, IDataLoaderState newState)
			{
				states.Add(newState);
			}
		}

		[Fact]
		public async Task SequenceIds_are_generated_correctly()
		{
			// Test that SequenceIds are generate like the following: 0, 1, 2, 3, etc.

			var ct = CancellationToken.None;
			var ids = new List<int>();
			var dataLoader = new DataLoader("sut", new DelegatedDataLoaderStrategy(Load), DataLoaderConcurrentMode.CancelPrevious, s => false);

			await dataLoader.Load(ct);

			Assert.Single(ids);
			Assert.Equal(0, ids[0]);

			await dataLoader.Load(ct);

			Assert.Equal(2, ids.Count);
			Assert.Equal(0, ids[0]);
			Assert.Equal(1, ids[1]);

			var trigger = new ManualDataLoaderTrigger();
			dataLoader.AddTrigger(trigger);
			trigger.Trigger();

			Assert.Equal(3, ids.Count);
			Assert.Equal(0, ids[0]);
			Assert.Equal(1, ids[1]);
			Assert.Equal(2, ids[2]);

			Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				ids.Add(request.SequenceId);

				return Task.FromResult<object>(null);
			}
		}

		[Fact]
		/// <summary>
		/// Tests that values added in <see cref="IDataLoaderRequest.Context"/> from the Load method are still available in next Load methods.
		/// </summary>
		public async Task Context_values_from_load_are_kept_across_loads()
		{
			var ct = CancellationToken.None;
			var dataLoader = new DataLoader("sut", new DelegatedDataLoaderStrategy(Load), DataLoaderConcurrentMode.CancelPrevious, s => false);

			await dataLoader.Load(ct);

			Assert.True(dataLoader.State.Request.Context.ContainsKey("0"));

			await dataLoader.Load(ct);

			Assert.True(dataLoader.State.Request.Context.ContainsKey("0"));
			Assert.True(dataLoader.State.Request.Context.ContainsKey("1"));

			Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				for (int i = 0; i < request.SequenceId; i++)
				{
					Assert.True(request.Context.ContainsKey(i.ToString()));
				}

				var key = request.SequenceId.ToString();
				request.Context.Add(key, key);

				return Task.FromResult<object>(null);
			}
		}

		[Fact]
		/// <summary>
		/// Tests that values added in <see cref="IDataLoaderRequest.Context"/> from the <see cref="IDataLoaderTrigger.LoadRequested"/> are not available in next Load methods.
		/// </summary>
		public async Task Context_values_from_trigger_are_not_kept_across_loads()
		{
			var ct = CancellationToken.None;
			var dataLoader = new DataLoader("sut", new DelegatedDataLoaderStrategy(Load), DataLoaderConcurrentMode.CancelPrevious, s => false);

			await dataLoader.Load(ct, new DataLoaderContext(new Dictionary<string, object> { { "first", null } }));

			Assert.True(dataLoader.State.Request.Context.ContainsKey("first"));

			await dataLoader.Load(ct, new DataLoaderContext(new Dictionary<string, object> { { "second", null } }));

			Assert.False(dataLoader.State.Request.Context.ContainsKey("first"));
			Assert.True(dataLoader.State.Request.Context.ContainsKey("second"));

			Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return Task.FromResult<object>(null);
			}
		}

		private IDataLoader GetDefaultDataLoader(DataLoaderDelegate loadMethod = null)
		{
			loadMethod ??= async (ct, request) => "result";

			return new DataLoader("sut", new DelegatedDataLoaderStrategy(loadMethod), DataLoaderConcurrentMode.CancelPrevious, s => false);
		}
	}
}
