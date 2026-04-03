using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;


namespace CPUSetSetter.Themes
{
    public static class AppTheme
    {
        public static void ApplyTheme(Theme theme)
        {
            string themePath;
            bool isDark = false;
            switch (theme)
            {
                case Theme.Light:
                    themePath = "Themes/LightThemeColors.xaml";
                    break;
                case Theme.Dark:
                    themePath = "Themes/DarkThemeColors.xaml";
                    isDark = true;
                    break;
                case Theme.System:
                    ApplySystemTheme();
                    return;
                default:
                    throw new ArgumentException("Invalid theme");
            }

            var mergedDicts = App.Current.Resources.MergedDictionaries;
            ResourceDictionary colorDict = new() { Source = new(themePath, UriKind.Relative) };
            if (mergedDicts.Count == 0)
            {
                // Theme is being set for the first time, set the theme colors and the Styles that use them
                mergedDicts.Add(colorDict);
                mergedDicts.Add(new() { Source = new("Themes/Styles.xaml", UriKind.Relative) });
            }
            else if (mergedDicts.Count == 2)
            {
                // Theme is being hot-switched. Only change the colors
                mergedDicts[0] = colorDict;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected MergedDictionaries count: {mergedDicts.Count}");
            }

            UpdateTitleBarStatus(isDark);
        }

        private static void ApplySystemTheme()
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            object? value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue && intValue == 0)
            {
                ApplyTheme(Theme.Dark);
                return;
            }
            ApplyTheme(Theme.Light);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static bool _isDarkTheme;
        private static bool _isWindowLoadedRegistered;

        private static void UpdateTitleBarStatus(bool isDarkTheme)
        {
            if (Application.Current == null) return;
            Application.Current.Dispatcher.VerifyAccess();

            _isDarkTheme = isDarkTheme;

            if (!_isWindowLoadedRegistered)
            {
                EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(Window_Loaded));
                _isWindowLoadedRegistered = true;
            }

            foreach (Window window in Application.Current.Windows)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero)
                {
                    window.SourceInitialized += OnSourceInitialized;
                }
                else
                {
                    ApplyDarkMode(hwnd, _isDarkTheme);
                }
            }
        }

        private static void OnSourceInitialized(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.SourceInitialized -= OnSourceInitialized;
                var hwnd = new WindowInteropHelper(window).Handle;
                ApplyDarkMode(hwnd, _isDarkTheme);
            }
        }

        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    ApplyDarkMode(hwnd, _isDarkTheme);
                }
            }
        }

        private static void ApplyDarkMode(IntPtr hwnd, bool isDarkTheme)
        {
            int useImmersiveDarkMode = isDarkTheme ? 1 : 0;
            if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
            }
        }
    }

    public enum Theme
    {
        Light,
        Dark,
        System
    }
}
