using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader;
using Xunit;

namespace Tests.Core.Unit
{
	public class DataLoaderTests
	{
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
	}
}
