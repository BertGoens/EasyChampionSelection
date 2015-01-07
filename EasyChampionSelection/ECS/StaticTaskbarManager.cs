using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace EasyChampionSelection.ECS {
    public sealed class StaticTaskbarManager {
        private static string _appName;
        private static StaticTaskbarManager _instance;
        private static TaskbarIcon _myTaskbarIcon;

        public TaskbarIcon MyTaskbarIcon {
            get { return StaticTaskbarManager._myTaskbarIcon; }
            set { StaticTaskbarManager._myTaskbarIcon = value; }
        }

        public StaticTaskbarManager Instance {
            get { return StaticTaskbarManager._instance; }
            private set { StaticTaskbarManager._instance = value; }
        }

        private StaticTaskbarManager() {
            if(_myTaskbarIcon != null) {
                _myTaskbarIcon.Dispose();
            }

            SetupNotifyIcon();
        }

        public static StaticTaskbarManager getInstance(string appName) {
            _appName = appName;
            _instance = new StaticTaskbarManager();
            return _instance;
        }

        private void SetupNotifyIcon() {
            _myTaskbarIcon = new TaskbarIcon();
            _myTaskbarIcon.Icon = EasyChampionSelection.Properties.Resources.LolIcon;
            _myTaskbarIcon.ToolTipText = _appName;
            _myTaskbarIcon.MenuActivation = PopupActivationMode.RightClick;
        }

        public void DisplayPopup(string message) {
            if(message == null) {
                return;
            }

            try {
                FancyBalloon balloon = new FancyBalloon(_appName, message);
                _myTaskbarIcon.ShowCustomBalloon(balloon, PopupAnimation.Fade, 3500);
            } catch(Exception) { }
        }

        public void Dispose() {
           _myTaskbarIcon.Dispose();
        }
    }
}
