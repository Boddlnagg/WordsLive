using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WordsLive.Core
{
	public static class TaskHelpers
	{
		public static Task Canceled()
		{
			return CancelCache<AsyncVoid>.Canceled;
		}

		public static Task<TResult> Canceled<TResult>()
		{
			return CancelCache<TResult>.Canceled;
		}

		static class CancelCache<TResult>
		{
			public static readonly Task<TResult> Canceled
				= GetCancelledTask();

			static Task<TResult> GetCancelledTask()
			{
				var tcs = new TaskCompletionSource<TResult>();
				tcs.SetCanceled();
				return tcs.Task;
			}
		}

		struct AsyncVoid { }

		static readonly Task<object> _completedTaskReturningNull
			= FromResult<object>(null);

		static readonly Task _defaultCompleted
			= FromResult<AsyncVoid>(default(AsyncVoid));

		public static Task Completed()
		{
			return _defaultCompleted;
		}

		public static Task<TResult> FromResult<TResult>(
			TResult result)
		{
			var tcs = new TaskCompletionSource<TResult>();
			tcs.SetResult(result);
			return tcs.Task;
		}

		public static Task<object> NullResult()
		{
			return _completedTaskReturningNull;
		}

		public static Task FromError(Exception exception)
		{
			return FromError<AsyncVoid>(exception);
		}

		public static Task<TResult> FromError<TResult>(Exception exception)
		{
			var tcs = new TaskCompletionSource<TResult>();
			tcs.SetException(exception);
			return tcs.Task;
		}

		public static Task FromErrors(IEnumerable<Exception> exceptions)
		{
			return FromErrors<AsyncVoid>(exceptions);
		}

		public static Task<TResult> FromErrors<TResult>(IEnumerable<Exception> exceptions)
		{
			var tcs = new TaskCompletionSource<TResult>();
			tcs.SetException(exceptions);
			return tcs.Task;
		}
		public static Task RunSynchronously(
			Action action,
			CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested)
				return Canceled();

			try
			{
				action();
				return Completed();
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task<TResult> RunSynchronously<TResult>(
			Func<TResult> func,
			CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested)
				return Canceled<TResult>();

			try
			{
				return FromResult(func());
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task<TResult> RunSynchronously<TResult>(
			Func<Task<TResult>> func,
			CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested)
				return Canceled<TResult>();

			try
			{
				return func();
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}
	}
}
