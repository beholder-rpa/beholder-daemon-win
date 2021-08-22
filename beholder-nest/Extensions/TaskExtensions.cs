﻿namespace beholder_nest.Extensions
{
  using System.Threading.Tasks;

  public static class TaskExtensions
  {
    /// <summary>
    /// Allows for forgetting the invocation of a task.
    /// </summary>
    /// <param name="task"></param>
    public static void Forget(this Task task)
    {
      // note: this code is inspired by a tweet from Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
      // Only care about tasks that may fault (not completed) or are faulted,
      // so fast-path for SuccessfullyCompleted and Cancelled tasks.
      if (!task.IsCompleted || task.IsFaulted)
      {
        // use "_" (Discard operation) to remove the warning IDE0058: Because this call is not awaited, execution of the current method continues before the call is completed
        // https://docs.microsoft.com/en-us/dotnet/csharp/discards#a-standalone-discard
        _ = ForgetAwaited(task);
      }

      // Allocate the async/await state machine only when needed for performance reason.
      // More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/
      async static Task ForgetAwaited(Task task)
      {
        try
        {
          // No need to resume on the original SynchronizationContext, so use ConfigureAwait(false)
          await task.ConfigureAwait(false);
        }
        catch
        {
          // Nothing to do here
        }
      }
    }
  }
}