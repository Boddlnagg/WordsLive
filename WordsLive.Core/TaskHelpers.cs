/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WordsLive.Core
{
	/// <summary>
	/// Mostly taken from http://bradwilson.typepad.com/blog/2012/04/tpl-and-servers-pt4.html
	/// Probably licensed under the Apache License, Version 2.0
	/// </summary>
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

		public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				task.Wait(cancellationToken);
				return task.Result;
			}
			catch (AggregateException ex)
			{
				// note: this destroys the stack trace
				throw ex.InnerException;
			}
		}

		public static void WaitAndUnwrapException(this Task task, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				task.Wait(cancellationToken);
			}
			catch (AggregateException ex)
			{
				// note: this destroys the stack trace
				throw ex.InnerException;
			}
		}
	}
}
