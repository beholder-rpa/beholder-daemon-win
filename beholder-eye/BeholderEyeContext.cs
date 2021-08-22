namespace beholder_eye
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  public sealed class BeholderEyeContext : IDisposable
  {
    private bool _isDisposed;

    public BeholderEyeContext()
    {
      ObserverCts = new CancellationTokenSource();
    }

    public Task Observer
    {
      get;
      set;
    }

    public CancellationTokenSource ObserverCts
    {
      get;
      set;
    }

    private void Dispose(bool disposing)
    {
      if (!_isDisposed)
      {
        if (disposing)
        {
          if (ObserverCts != null)
          {
            ObserverCts.Cancel();
            ObserverCts.Dispose();
            ObserverCts = null;
          }
        }

        _isDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}