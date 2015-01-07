using EasyChampionSelection.ECS;
using System.Threading;
using System.Windows;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        Semaphore sema;
        bool shouldRelease = false;
        const string appName = "Easy Champion Selection"; //Used as semaphore lock

        private AppRuntimeResources arr;

        private void Application_Startup(object sender, StartupEventArgs e) {
            SingleInstanceCheck();
            arr = new AppRuntimeResources();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            if(shouldRelease) { //Are we the application holding on to the resources?
                sema.Release();
                arr.SaveSerializedData();
                arr.Dispose();
            }

            Application.Current.Shutdown();
        }

        private void SingleInstanceCheck() {
            //Check for duplicates running, close if there is already an Easy Champion Selection running
            bool result = Semaphore.TryOpenExisting(appName, out sema);
            if(result) { // we have another instance running
                App.Current.Shutdown();
            } else { //New instance
                try {
                    sema = new Semaphore(1, 1, appName);
                } catch {
                    App.Current.Shutdown(); //
                }
            }

            if(!sema.WaitOne(0)) {
                App.Current.Shutdown();
            } else {
                shouldRelease = true;
            }
        }
    }
}
