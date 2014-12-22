using EasyChampionSelection.ECS;
using System.Windows;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private AppRuntimeResources arr;

        private void Application_Startup(object sender, StartupEventArgs e) {
            arr = new AppRuntimeResources();
            if(arr.MySettings.ShowMainFormOnLaunch) {
                arr.Window_Main.Show();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            arr.SaveSerializedData();
            arr.Dispose();
            Application.Current.Shutdown();
        }

    }
}
