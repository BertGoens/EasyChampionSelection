using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.AppRuntimeResources;
using System.Windows;
using System.Windows.Threading;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private AppRuntimeResourcesManager arr;
        private static readonly SingleInstance SingleInstance = new SingleInstance();

        private void Application_Startup(object sender, StartupEventArgs e) {
            if(SingleInstance.IsFirstInstance) {
                SingleInstance.ArgumentsReceived += SingleInstanceParameter;
                SingleInstance.ListenForArgumentsFromSuccessiveInstances();
                // Do your other app logic
                arr = new AppRuntimeResourcesManager();
            } else {
                //Tell it to show it's main window
                SingleInstance.PassArgumentsToFirstInstance("Show");
                // if there is an argument available, fire it
                if(e.Args.Length > 0) {
                    SingleInstance.PassArgumentsToFirstInstance(e.Args[0]);
                }

                Application.Current.Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            if(SingleInstance.IsFirstInstance) { //First (unique) instance
                SingleInstance.Dispose();
                arr.Dispose();
            }
        }

        /// <summary>
        ///     Handles the DispatcherUnhandledException event of the App control. 
        ///     Makes sure that any unhandled exceptions produce an error report that includes a stack trace.
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            arr.DisplayPopup("An unhandled exception occurred: " + e.Exception.Message);
            StaticErrorLogger.WriteErrorReport(e.Exception, "Unhandled!");
            e.Handled = true;   // Prevent default unhandled exception processing
        }

        //Used to be static
        private void SingleInstanceParameter(object sender, GenericEventArgs<string> e) {
                arr.Window_Main.Show();
        }
    }
}
