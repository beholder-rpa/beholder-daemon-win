﻿namespace beholder_occipital
{
  using beholder_nest;
  using beholder_nest.Extensions;
  using beholder_nest.Mqtt;
  using beholder_occipital.Models;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class BeholderOccipitalObserver : IObserver<BeholderOccipitalEvent>
  {
    private readonly ILogger<BeholderOccipitalObserver> _logger;
    private readonly IBeholderMqttClient _beholderClient;

    private IList<ObjectPoly> _lastResponse;

    public BeholderOccipitalObserver(ILogger<BeholderOccipitalObserver> logger, IBeholderMqttClient beholderClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
    }

    public void OnCompleted()
    {
      // Do Nothing
    }

    public void OnError(Exception error)
    {
      // Do Nothing
    }

    public void OnNext(BeholderOccipitalEvent occipitalEvent)
    {
      switch (occipitalEvent)
      {
        case ObjectDetectionEvent objectDetectionEvent:
          HandleObjectDetection(objectDetectionEvent.Locations).Forget();
          break;
        default:
          _logger.LogWarning($"Unhandled or unknown BeholderOccipitalEvent: {occipitalEvent}");
          break;
      }
    }

    private async Task HandleObjectDetection(IList<ObjectPoly> objectLocations)
    {
      await _beholderClient.MqttClient.PublishEventAsync(
        BeholderConsts.PubSubName,
        $"beholder/occipital/{{HOSTNAME}}/detected_objects",
        objectLocations
      );

      _logger.LogInformation($"Occipital Located {objectLocations.Count} polys.");
    }
  }
}