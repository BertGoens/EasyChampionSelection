using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A basic (de)serialization class
    /// It will automatically create directories of requested files if needed.
    /// </summary>
    public static class StaticSerializer {
        private const string appName = "Easy Champion Selection";

        // @\Save\
        private static string _fullPath_ClientImage = userAppDataPath() + Folder_SaveData + Object_ClientImage;
        private static string _fullPath_Settings = userAppDataPath() + Folder_SaveData + Object_Settings;
        private static string _fullPath_AllChampions = userAppDataPath() + Folder_SaveData + Object_AllChampions;
        private static string _fullPath_GroupManager = userAppDataPath() + Folder_SaveData + Object_GroupManager;

        // @\Error\
        private static string _fullPath_ErrorFile = userAppDataPath() + Folder_ErrorData + DateTime.Today.ToString("d").Replace("/", "_") + ".txt";

        //Local folders
        private const string Folder_SaveData = @"\Save";
        private const string Folder_ErrorData = @"\Error";

        //Local files
        private const string Object_GroupManager = @"\Groups.ser";
        private const string Object_AllChampions = @"\AllChampions.ser";
        private const string Object_Settings = @"\Settings.ser";
        private const string Object_ClientImage = @"\ClientImage.jpg";

        #region Getters
        public static string FullPath_ClientImage {
            get 
            {
                CheckDirToPath(_fullPath_ClientImage);
                return _fullPath_ClientImage;
            }
        }

        public static string FullPath_Settings {
            get 
            {
                CheckDirToPath(_fullPath_Settings);
                return _fullPath_Settings;
            }
        }

        public static string FullPath_AllChampions {
            get 
            {
                CheckDirToPath(_fullPath_AllChampions);
                return _fullPath_AllChampions; 
            }
        }

        public static string FullPath_GroupManager {
            get 
            {
                CheckDirToPath(_fullPath_GroupManager);
                return _fullPath_GroupManager; 
            }
        }

        public static string FullPath_ErrorFile {
            get 
            {
                CheckDirToPath(_fullPath_ErrorFile);
                return _fullPath_ErrorFile; 
            }
        }
        #endregion

        /// <summary>
        /// If it's directory doesn't exist, it will be created by requesting admin rights to write it.
        /// </summary>
        private static void CheckDirToPath(string fullPath) {
            string dirToPath = fullPath.Substring(0, fullPath.LastIndexOf(@"\"));
            if(!Directory.Exists(dirToPath)) {
                Directory.CreateDirectory(dirToPath);
            }
        }

        /// <summary>
        /// Returns the absolute path of where the exe is located
        /// </summary>
        public static string applicationPath() {
            string r = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            r = Path.GetDirectoryName(r);
            r = r.Replace("file:/", "");
            r = r.Replace(@"file:\", "");
            return r;
        }

        /// <summary>
        /// Returns the absolute path of the Application Data Folder where all the data is stored
        /// </summary>
        /// <returns></returns>
        public static string userAppDataPath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + appName;
        }

        public static bool SerializeObject(object objectToSerialize, string fileName) {
            try {
                Directory.CreateDirectory(fileName.Substring(0, fileName.LastIndexOf(@"\"))); //Extract directory

                Stream stream = File.Open(fileName, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
                stream.Close();
            } catch(Exception ex) {
                ex.ToString();
                return false;
            }
            return true;
        }

        public static object DeSerializeObject(string fileName) {
            if(File.Exists(fileName)) {
                object objectToDeSerialize;
                Stream stream;
                try {
                    stream = File.Open(fileName, FileMode.Open);
                } catch(IOException) {
                    //File is in use, your kind of fucked if I don't write additional code to catch it
                    return null;
                }

                try {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    objectToDeSerialize = bFormatter.Deserialize(stream);
                    stream.Close();
                    return objectToDeSerialize;
                } catch(SerializationException) {  //Attempting to deserialize an empty stream, just delete it as it is void
                    stream.Close();
                    File.Delete(fileName);
                }
            }

            return null;
        }
    }
}