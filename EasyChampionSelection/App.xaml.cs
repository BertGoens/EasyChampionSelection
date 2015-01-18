using EasyChampionSelection.ECS.AppRuntimeResources;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        Semaphore sema;
        bool shouldRelease = false;

        private AppRuntimeResourcesManager arr;

        private void Application_Startup(object sender, StartupEventArgs e) {
            SingleInstanceCheck();
            arr = new AppRuntimeResourcesManager();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            if(shouldRelease) { //Are we the application holding on to the resources?
                sema.Release();
                arr.Dispose();
            }

            Application.Current.Shutdown();
        }

        private void SingleInstanceCheck() {
            //Check for duplicates running, close if there is already an Easy Champion Selection running
            string semaLock = getSemaLock();
            bool result = Semaphore.TryOpenExisting(semaLock, out sema);
            if(result) { // we have another instance running
                App.Current.Shutdown();
            } else { //New instance
                try {
                    sema = new Semaphore(1, 1, semaLock);
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

        private string getSemaLock() {
            string semaLock = WindowsIdentity.GetCurrent().Name + ":Easy Champion Selection";
            semaLock = semaLock.Replace("\\", ":");
            return semaLock;
        }
    }
}
