using CPUSetSetter.UI.Tabs.Processes;
using Microsoft.Win32;
using System;

namespace CPUSetSetter.Platforms.Windows
{
    public static class GameMode
    {
        private const string GameBarKey = @"Software\Microsoft\GameBar";
        private const string ValueName = "AutoGameModeEnabled";

        public static event EventHandler? IsEnabledChanged;
        private static readonly object _stateLock = new();
        private static bool? _lastState;

        public static void CheckState()
        {
            bool raiseEvent = false;

            lock (_stateLock)
            {
                bool currentState = IsEnabled;
                if (!_lastState.HasValue)
                {
                    _lastState = currentState;
                    return;
                }
                if (_lastState != currentState)
                {
                    _lastState = currentState;
                    raiseEvent = true;
                }
            }

            if (raiseEvent)
            {
                IsEnabledChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool IsEnabled
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(GameBarKey);
                return key?.GetValue(ValueName, 0) is int intValue && intValue != 0;
            }
            set
            {
                try
                {
                    using var key = Registry.CurrentUser.CreateSubKey(GameBarKey);
                    if (key == null)
                    {
                        WindowLogger.Write("Unable to toggle Windows Game Mode: Registry key could not be created or opened.");
                        return;
                    }

                    key.SetValue(ValueName, value ? 1 : 0, RegistryValueKind.DWord);
                    CheckState();
                }
                catch (Exception ex)
                {
                    WindowLogger.Write($"Unable to toggle Windows Game Mode: {ex.Message}");
                }
            }
        }
    }
}
