using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private TaskbarIcon _tb;

        private void Application_Startup(object sender, StartupEventArgs e) {
            SetupNotifyIcon();
            wndMain wndM = new wndMain(_tb);
            wndM.Load(null,null);
        }

        private void SetupNotifyIcon() {
            _tb = new TaskbarIcon();
            _tb.Icon =  EasyChampionSelection.Properties.Resources.LolIcon;
            _tb.ToolTipText = "Easy Champion Selection";
            _tb.Visibility = Visibility.Visible;
            _tb.MenuActivation = PopupActivationMode.RightClick;
        }
    }
}
