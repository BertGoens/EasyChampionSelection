using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace EasyChampionSelection.ECS {
    public sealed class StaticTaskbarManager {
        private static StaticTaskbarManager _instance;
        private static TaskbarIcon _tb;

        public TaskbarIcon Tb {
            get { return StaticTaskbarManager._tb; }
            set { StaticTaskbarManager._tb = value; }
        }

        private const string appTitle = "Easy Champion Selection";

        private StaticTaskbarManager() {
            if(_tb != null) {
                _tb.Dispose();
            }

            SetupNotifyIcon();
        }

        public static StaticTaskbarManager getInstance() {
            _instance = new StaticTaskbarManager();
            return _instance;
        }

        private void SetupNotifyIcon() {
            _tb = new TaskbarIcon();
            _tb.Icon = EasyChampionSelection.Properties.Resources.LolIcon;
            _tb.ToolTipText = appTitle;
            _tb.Visibility = Visibility.Visible;
            _tb.MenuActivation = PopupActivationMode.RightClick;
        }

        public void DisplayPopup(string message, bool isErrorMessage, Window sender = null) {
            if(sender != null) {
                if(!isErrorMessage && !sender.IsLoaded) {
                    return;
                }
            }

            try {
                FancyBalloon balloon = new FancyBalloon(appTitle, message);
                _tb.ShowCustomBalloon(balloon, PopupAnimation.Fade, 3500);
            } catch(Exception) { }
        }

        public void Dispose() {
            _tb.Dispose();
        }
    }
}
