using EasyChampionSelection.ECS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private AppRuntimeResources arr;

        private void Application_Startup(object sender, StartupEventArgs e) {
            bool isNotRunFromProgramFiles = false;
            double dotNetVersion = 0;
            if(!isRunFromProgramFiles()) {
                isNotRunFromProgramFiles = true;
            }

            if(!VersionFromRegistryIs_4_5()) {
                dotNetVersion = 5;
            }

            arr = new AppRuntimeResources();
            if(arr.MySettings.ShowMainFormOnLaunch) {
                arr.Window_Main.Show();
            }

            if(isNotRunFromProgramFiles) {
                arr.DisplayPopup("Please run ECS from /Program Files (x86)/! \nElse we can't request the lolClient UI.", true, null);
            }
            if(dotNetVersion < 4.5) {
                arr.DisplayPopup("Please download .NET 4.5.", true, null);
            }
            
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            arr.SaveSerializedData();
            arr.Dispose();
            Application.Current.Shutdown();
        }

        private bool isRunFromProgramFiles() {
            string path = AppDomain.CurrentDomain.BaseDirectory.ToString();
            string programfileX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if(path.IndexOf(programfileX86, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
            return false;
        }

        private static double versionToDouble(string version) {
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

        private static bool VersionFromRegistryIs_4_5() {
            double cVersion = 0;
            double thisVersion;

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
            if(cVersion < 4.5) {
                return false;
            } else {
                return true;
            }
        }
    }
}
