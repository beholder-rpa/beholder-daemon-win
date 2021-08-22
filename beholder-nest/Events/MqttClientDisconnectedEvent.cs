namespace beholder_nest
{
  /// <summary>
  /// Event that is produced when the MqttClient is disconnected
  /// </summary>
  public class MqttClientDisconnectedEvent : MqttClientEvent
  {
    public string Reason
    {
      get;
      set;
    }
  }
}