using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This class represents the state of a <see cref="IDataLoader"/>.
	/// This class is immutable.
	/// </summary>
	[Preserve(AllMembers = true)]
	public class DataLoaderState : IDataLoaderState
	{
		/// <summary>
		/// Gets the default <see cref="DataLoaderState"/>.
		/// </summary>
		public static DataLoaderState Default { get; } = new DataLoaderState();

		/// <summary>
		/// Initializes a new instance of the <see cref="DataLoaderState"/> class.
		/// </summary>
		private DataLoaderState()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataLoaderState"/> class.
		/// </summary>
		/// <param name="source">The <see cref="IDataLoaderState"/>.</param>
		public DataLoaderState(IDataLoaderState source)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			Request = source.Request;
			IsLoading = source.IsLoading;
			Data = source.Data;
			Error = source.Error;
			IsInitial = source.IsInitial;
			IsEmpty = source.IsEmpty;
		}

		/// <inheritdoc cref="IDataLoaderState.Request" />
		public IDataLoaderRequest Request { get; private set; }

		/// <inheritdoc cref="IDataLoaderState.Data" />
		public object Data { get; private set; }

		/// <inheritdoc cref="IDataLoaderState.Error" />
		public Exception Error { get; private set; }

		/// <inheritdoc cref="IDataLoaderState.IsLoading" />
		public bool IsLoading { get; private set; }

		/// <inheritdoc cref="IDataLoaderState.IsInitial" />
		public bool IsInitial { get; private set; } = true;

		/// <inheritdoc cref="IDataLoaderState.IsEmpty" />
		public bool IsEmpty { get; private set; } = true;

		public override string ToString()
		{
			return string.Join(", ", GetQualifications());

			IEnumerable<string> GetQualifications()
			{
				if (IsInitial)
				{
					yield return "Initial";
				}

				if (IsEmpty)
				{
					yield return "Empty";
				}
				else if (Data != null)
				{
					yield return "Data";
				}

				if (IsLoading)
				{
					yield return "IsLoading";
				}

				if (Error != null)
				{
					yield return "Error";
				}

				if (Request != null)
				{
					yield return $"({Request})";
				}
			}
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (!(obj is IDataLoaderState state))
			{
				return false;
			}

			if (state.Request != Request)
			{
				return false;
			}
			if (state.Data != Data)
			{
				return false;
			}
			if (state.Error != Error)
			{
				return false;
			}
			if (state.IsLoading != IsLoading)
			{
				return false;
			}
			if (state.IsInitial != IsInitial)
			{
				return false;
			}
			if (state.IsEmpty != IsEmpty)
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Request?.GetHashCode() ?? 0 ^ Data?.GetHashCode() ?? 0 ^ IsLoading.GetHashCode();
		}

		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderState"/>.
		/// </summary>
		/// <param name="request">The <see cref="IDataLoaderRequest"/>.</param>
		/// <returns>The <see cref="DataLoaderState"/>.</returns>
		public DataLoaderState WithRequest(IDataLoaderRequest request)
		{
			return new DataLoaderState(this)
			{
				Request = request
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderState"/>.
		/// </summary>
		/// <param name="isLoading"><see cref="IDataLoaderState.IsLoading"/></param>
		/// <returns><see cref="DataLoaderState"/></returns>
		public DataLoaderState WithIsLoading(bool isLoading)
		{
			return new DataLoaderState(this)
			{
				IsLoading = isLoading
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderState"/>.
		/// </summary>
		/// <param name="data">The data to populate the <see cref="DataLoaderState"/>.</param>
		/// <returns><see cref="DataLoaderState"/></returns>
		public DataLoaderState WithData(object data)
		{
			var newState = new DataLoaderState(this)
			{
				Data = data,
				IsInitial = false
			};

			return newState;
		}

		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderState"/>.
		/// </summary>
		/// <param name="isEmpty">The flag indicating whether the <see cref="DataLoaderState"/> is empty.</param>
		/// <returns><see cref="DataLoaderState"/></returns>
		public DataLoaderState WithIsEmpty(bool isEmpty)
		{
			var newState = new DataLoaderState(this)
			{
				IsEmpty = isEmpty
			};

			return newState;
		}

		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderState"/>.
		/// </summary>
		/// <param name="error">The error that <see cref="DataLoaderState"/> will show.</param>
		/// <returns><see cref="DataLoaderState"/></returns>
		public DataLoaderState WithError(Exception error)
		{
			return new DataLoaderState(this)
			{
				Error = error
			};
		}
	}

	/// <summary>
	/// Typed version of <see cref="DataLoaderState"/>.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public class DataLoaderState<TData> : DataLoaderState, IDataLoaderState<TData>
	{
		public DataLoaderState(IDataLoaderState state)
			: base(state)
		{
		}

		TData IDataLoaderState<TData>.Data => (TData)base.Data;
	}

	/// <summary>
	/// Extensions on <see cref="IDataLoaderState"/>.
	/// </summary>
	public static class DataLoaderStateExtensions
	{
		/// <summary>
		/// Returns a typed version of <see cref="IDataLoaderState"/>.
		/// </summary>
		/// <typeparam name="TData">Type of data</typeparam>
		/// <param name="state"><see cref="IDataLoaderState"/></param>
		/// <returns><see cref="IDataLoaderState{TData}"/></returns>
		public static IDataLoaderState<TData> AsOf<TData>(this IDataLoaderState state)
		{
			if (state is IDataLoaderState<TData> typed)
			{
				return typed;
			}
			else
			{
				return new DataLoaderState<TData>(state);
			}
		}
	}
}
