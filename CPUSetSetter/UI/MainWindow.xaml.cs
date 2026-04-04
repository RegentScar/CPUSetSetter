using CPUSetSetter.Platforms.Windows;
using CPUSetSetter.UI.Tabs.Processes;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CPUSetSetter.UI
{
    public partial class MainWindow : Window
    {
        private bool _listIsPaused = false;

        // Store the handler in a field for proper unsubscription
        private readonly EventHandler _gameModeChangedHandler;

        public MainWindow()
        {
            InitializeComponent();

            // Listen for the Ctrl key, so the processes list's live sorting can be paused
            PreviewKeyDown += (_, e) => KeyPressed(e);
            PreviewKeyUp += (_, e) => KeyReleased(e);

            Deactivated += (_, _) => ResumeListUpdates();

            GameModeCheckBox.IsChecked = GameMode.IsEnabled;
            GameModeCheckBox.Checked += (s, e) => GameMode.IsEnabled = true;
            GameModeCheckBox.Unchecked += (s, e) => GameMode.IsEnabled = false;

            _gameModeChangedHandler = (s, e) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (GameModeCheckBox.IsChecked != GameMode.IsEnabled)
                    {
                        GameModeCheckBox.IsChecked = GameMode.IsEnabled;
                    }
                });
            };
            GameMode.IsEnabledChanged += _gameModeChangedHandler;
            Application.Current.Exit += (s, ev) => GameMode.IsEnabledChanged -= _gameModeChangedHandler;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void KeyPressed(KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && !_listIsPaused)
            {
                _listIsPaused = true;
                ProcessesTabViewModel.Instance?.PauseListUpdates();
            }
        }

        private void KeyReleased(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ResumeListUpdates();
            }
        }

        private void ResumeListUpdates()
        {
            if (_listIsPaused)
            {
                _listIsPaused = false;
                ProcessesTabViewModel.Instance?.ResumeListUpdates();
            }
        }
    }
}
