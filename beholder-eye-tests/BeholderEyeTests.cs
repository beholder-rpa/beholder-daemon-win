namespace beholder_eye_tests
{
  using beholder_eye;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Security.Cryptography;
  using System.Threading;
  using System.Threading.Tasks;
  using Xunit;

  public class BeholderEyeTests
  {
    private readonly Mock<ILogger<BeholderEye>> _mockLogger;

    public BeholderEyeTests()
    {
      _mockLogger = new Mock<ILogger<BeholderEye>>();
      _mockLogger
          .Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
          .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, eventId, obj, ex, fn) =>
          {
            Console.WriteLine($"{level} - {obj} {ex}");
          });
    }

    [Fact]
    public async Task CanCaptureDesktopFramesUntilCancelled()
    {
      var callbackCount = 0;
      var sha256 = SHA256.Create();

      using var cts = new CancellationTokenSource();

      var beholderEye = new BeholderEye(_mockLogger.Object);
      await beholderEye.ObserveWithUnwaveringSight(new ObservationRequest()
      {

      },
      null,
      cts.Token
      );

      Assert.True(callbackCount > 0);
    }
  }
}