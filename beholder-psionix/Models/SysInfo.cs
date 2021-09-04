namespace beholder_psionix.Models
{
  using System;
  using System.Drawing;
  using System.Text.Json.Serialization;
  using System.Windows.Forms;
  using Rectangle = beholder_nest.Models.Rectangle;
  using Size = beholder_nest.Models.Size;

  public record SysInfo
  {
    public SysInfo()
    {
      var cursorSize = SystemInformation.CursorSize;
      CursorSize = new Size()
      {
        Width = cursorSize.Width,
        Height = cursorSize.Height,
      };

      var doubleClickSize = SystemInformation.DoubleClickSize;
      DoubleClickSize = new Size()
      {
        Width = doubleClickSize.Width,
        Height = doubleClickSize.Height,
      };

      DoubleClickTime = SystemInformation.DoubleClickTime;

      KeyboardDelay = SystemInformation.KeyboardDelay;
      KeyboardSpeed = SystemInformation.KeyboardSpeed;
      MonitorCount = SystemInformation.MonitorCount;
      MouseSpeed = SystemInformation.MouseSpeed;

      var wa = SystemInformation.WorkingArea;
      WorkingArea = new Rectangle()
      {
        X = wa.X,
        Y = wa.Y,
        Width = wa.Width,
        Height = wa.Height,
      };

      MouseInfo = NativeMethods.GetMouseInfo();
      MouseUpdateRate = 150; // Just windows thangs...

      using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
      {
        DpiX = graphics.DpiX;
        DpiX = graphics.DpiY;
      }

    }

    [JsonPropertyName("cursorSize")]
    public Size CursorSize
    {
      get;
      init;
    }

    [JsonPropertyName("dpiX")]
    public float DpiX
    {
      get;
      set;
    }

    [JsonPropertyName("dpiY")]
    public float DpiY
    {
      get;
      set;
    }

    [JsonPropertyName("doubleClickSize")]
    public Size DoubleClickSize
    {
      get;
      init;
    }

    [JsonPropertyName("doubleClickTime")]
    public int DoubleClickTime
    {
      get;
      init;
    }

    [JsonPropertyName("keyboardDelay")]
    public int KeyboardDelay
    {
      get;
      init;
    }

    [JsonPropertyName("keyboardSpeed")]
    public int KeyboardSpeed
    {
      get;
      init;
    }

    [JsonPropertyName("monitorCount")]
    public int MonitorCount
    {
      get;
      init;
    }

    [JsonPropertyName("mouseSpeed")]
    public int MouseSpeed
    {
      get;
      init;
    }

    [JsonPropertyName("mouseInfo")]
    public MouseInfo MouseInfo
    {
      get;
      init;
    }

    [JsonPropertyName("mouseUpdateRate")]
    public int MouseUpdateRate
    {
      get;
      init;
    }

    [JsonPropertyName("workingArea")]
    public Rectangle WorkingArea
    {
      get;
      init;
    }
  }
}
