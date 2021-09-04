namespace beholder_psionix
{
  using beholder_psionix.Models;
  using System;
  using System.Runtime.InteropServices;

  public static unsafe partial class NativeMethods
  {
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SystemParametersInfo(SystemParametersInfoActions uiAction, uint uiParam, void* pvParam, SystemParametersInfoFlags flags);

    public static MouseInfo GetMouseInfo()
    {
      var data = new int[3];
      unsafe
      {
        fixed (int* ptrDataBuffer = data)
        {
          var result = SystemParametersInfo(SystemParametersInfoActions.SPI_GETMOUSE, 0, ptrDataBuffer, 0);
          if (!result)
          {
            throw new InvalidOperationException("Last call returned error.");
          }
        }
      }
      return new MouseInfo()
      {
        FirstThreshold = data[0],
        SecondThreshold = data[1],
        Acceleration = data[2],
      };
    }

    public static bool SetMouseInfo(MouseInfo mouseInfo)
    {
      if (mouseInfo == null)
      {
        throw new ArgumentNullException(nameof(mouseInfo));
      }

      var data = new int[3];
      data[0] = mouseInfo.FirstThreshold;
      data[1] = mouseInfo.SecondThreshold;
      data[2] = mouseInfo.Acceleration;

      unsafe
      {
        fixed (int* ptrDataBuffer = data)
        {
          var result = SystemParametersInfo(SystemParametersInfoActions.SPI_SETMOUSE, 0, ptrDataBuffer, 0);
          return result;
        }
      }
    }

    public static bool SetMouseSpeed(int newSpeed)
    {
      if (newSpeed < 1 || newSpeed > 20)
        throw new ArgumentOutOfRangeException(nameof(newSpeed));

      unsafe
      {
        var result = SystemParametersInfo(SystemParametersInfoActions.SPI_SETMOUSE, 0, &newSpeed, 0);
        return result;
      }
    }

    public enum SystemParametersInfoActions : uint
    {
      SPI_GETMOUSE = 0x0003,
      SPI_SETMOUSE = 0x0004,

      SPI_SETMOUSESPEED = 0x0071
    }

    [Flags]
    public enum SystemParametersInfoFlags
    {
      None = 0x00,

      /// <summary>
      /// Writes the new system-wide parameter setting to the user profile.
      /// </summary>
      SPIF_UPDATEINIFILE = 0x01,

      /// <summary>
      /// Broadcasts the WM_SETTINGCHANGE message after updating the user profile.
      /// </summary>
      SPIF_SENDCHANGE = 0x02,

      /// <summary>
      /// Same as SPIF_SENDCHANGE.
      /// </summary>
      SPIF_SENDWININICHANGE = 0x02,
    }
  }
}
