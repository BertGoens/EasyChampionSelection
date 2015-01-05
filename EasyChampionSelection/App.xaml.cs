using EasyChampionSelection.ECS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

            /*double dotNetVersion = DotnetRegistryVersion();*/

            arr = new AppRuntimeResources();
            if(arr.MySettings.ShowMainFormOnLaunch) {
                arr.Window_Main.Show();
            }

            if(isRunFromProgramFiles()) {
                arr.DisplayPopup("Please run " + appName + " from /Program Files (x86)/! \nElse we can't request the lolClient UI.", true, null);
            }

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

        private bool isRunFromProgramFiles() {
            string path = AppDomain.CurrentDomain.BaseDirectory.ToString();
            string programfileX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if(path.IndexOf(programfileX86, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
            return false;
        }

        private double versionToDouble(string version) {
            if(version.Length < 1) {
                return 0;
            }

            int dotPos = -1;
            if(version.Contains(".")) {
                dotPos = version.IndexOf(".");
            }

            string numbersOnly = "";
            for(int i = 0; i < version.Length; i++) {
                string thisChar = version.Substring(i, 1);
                int testIfInt = 0;
                if(int.TryParse(thisChar, out testIfInt)) {
                    numbersOnly += thisChar;
                }
            }

            if(dotPos > -1) {
                numbersOnly = numbersOnly.Insert(dotPos, ".");
            }
            return double.Parse(numbersOnly);
        }

        private double DotnetRegistryVersion() {
            double cVersion = 0;
            double thisVersion = 0;

            // Opens the registry key for the .NET Framework entry. 
            using(RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\")) {

                foreach(string versionKeyName in ndpKey.GetSubKeyNames()) {
                    if(versionKeyName.StartsWith("v")) {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        thisVersion = versionToDouble(name);
                        if(cVersion < thisVersion) {
                            cVersion = thisVersion;
                        }
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();

                        if(name != "") {
                            continue;
                        }

                        foreach(string subKeyName in versionKey.GetSubKeyNames()) {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            thisVersion = versionToDouble(name);
                            if(cVersion < thisVersion) {
                                cVersion = thisVersion;
                            }
                        }

                    }
                }
            }
            return cVersion;
        }
    }
}
