namespace beholder_nest.Models
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Text.Json.Serialization;

  public record BeholderServiceInfo
  {
    public BeholderServiceInfo()
    {
      HostName = Dns.GetHostName();
      IpAddresses = string.Join(", ", Dns.GetHostAddresses(Dns.GetHostName()).Select(ip => ip.ToString()));
      OS = Environment.OSVersion.ToString();
      ServiceName = "daemon";
      Version = "v1";
    }

    [JsonPropertyName("hostName")]
    public string HostName
    {
      get;
    }

    [JsonPropertyName("ipAddress")]
    public string IpAddresses
    {
      get;
    }


    [JsonPropertyName("os")]
    public string OS
    {
      get;
    }

    [JsonPropertyName("serviceName")]
    public string ServiceName
    {
      get;
      set;
    }

    [JsonPropertyName("version")]
    public string Version
    {
      get;
      set;
    }
  }
}