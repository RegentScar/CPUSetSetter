using CPUSetSetter.Config.Models;
using CPUSetSetter.UI.Tabs.Processes;
using System.Windows;

namespace CPUSetSetter.Platforms.Windows
{
    /// <summary>
    /// Windows Game Mode is abysmal to gaming performance when combined with CPU Set Setter.
    /// Warn the user, and encourage them to turn it off, or experiment with it on.
    /// </summary>
    public static class WindowsGameModeWarning
    {
        private static bool _eventHooked = false;
        private const string LogWarningMessage = "WARNING: While Windows Game Mode is enabled, no masks are applied. (This behavior can be disabled in the Settings tab)";

        public static void ShowIfEnabled()
        {
            if (!_eventHooked)
            {
                _eventHooked = true;
                GameMode.IsEnabledChanged += (s, e) =>
                {
                    if (GameMode.IsEnabled && !AppConfig.Instance.DisableGameModeMaskClearing)
                    {
                        WindowLogger.Write(LogWarningMessage);
                    }
                };
            }

            if (!GameMode.IsEnabled)
                return;

            if (AppConfig.Instance.ShowGameModePopup)
            {
                Application.Current?.Dispatcher?.InvokeAsync(() =>
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Windows Game Mode is currently enabled.\n" +
                        "On AMD CPUs, Game Mode is known to conflict with CPU Set Setter, leading to lower FPS and game crashes.\n" +
                        "I am not sure if Intel CPUs are affected too. So if you have one, please share your findings with me on GitHub!\n\n" +
                        "Would you like CPU Set Setter to disable Game Mode for you?\n\n" +
                        "(This popup can be disabled in the Settings tab)",
                        "CPU Set Setter: Windows Game Mode warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        GameMode.IsEnabled = false;
                    }
                    else
                    {
                        if (!AppConfig.Instance.DisableGameModeMaskClearing)
                            WindowLogger.Write(LogWarningMessage);
                    }
                });
            }
            else
            {
                if (!AppConfig.Instance.DisableGameModeMaskClearing)
                    WindowLogger.Write(LogWarningMessage);
            }
        }
    }
}
