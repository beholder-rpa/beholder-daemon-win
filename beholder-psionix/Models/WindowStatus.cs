namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  // See https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowplacement
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum WindowStatus
  {
    /// <summary>
    /// Hides the window and activates another window.
    /// </summary>
    Hidden = 0,
    /// <summary>
    /// Maximizes the specified window.
    /// </summary>
    Maximized = 3,
    /// <summary>
    /// Minimizes the specified window and activates the next top-level window in the z-order.
    /// </summary>
    Minimize = 6,
    /// <summary>
    /// Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
    /// </summary>
    Restore = 9,

    /// <summary>
    /// Activates the window and displays it in its current size and position.
    /// </summary>
    Show = 5,

    /// <summary>
    /// Activates the window and displays it as a minimized window.
    /// </summary>
    ShowMinimized = 2,

    /// <summary>
    /// Displays the window in its current size and position. This value is similar to SW_SHOW, except the window is not activated.
    /// </summary>
    ShowNoActivate = 4,

    /// <summary>
    /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
    /// </summary>
    ShowMinimizedNotActive = 7,

    /// <summary>
    /// Displays a window in its most recent size and position.This value is similar to SW_SHOWNORMAL, except the window is not activated.
    /// </summary>
    ShowNotActive = 8,

    /// <summary>
    /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
    /// </summary>
    ShowNormal = 1,
  }
}