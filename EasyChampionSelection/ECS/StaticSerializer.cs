using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A basic (de)serialization class
    /// </summary>
    public static class StaticSerializer {
        // @\Save\
        public static string FullPath_ClientImage = applicationPath() + Folder_SaveData + Object_ClientImage;
        public static string FullPath_Settings = applicationPath() + Folder_SaveData + Object_Settings;
        public static string FullPath_AllChampions = applicationPath() + Folder_SaveData + Object_AllChampions;
        public static string FullPath_GroupManager = applicationPath() + Folder_SaveData + Object_GroupManager;
        // @\Error\
        public static string FullPath_ErrorFile = applicationPath() + Folder_ErrorData + DateTime.Today.ToString("d").Replace("/", "_") + ".txt";

        //Local folders
        private const string Folder_SaveData = @"\Save\";
        private const string Folder_ErrorData = @"\Error\";

        //Local files
        private const string Object_GroupManager = "Groups.ser";
        private const string Object_AllChampions = "AllChampions.ser";
        private const string Object_Settings = "Settings.ser";
        private const string Object_ClientImage = "ClientImage.jpg";

        public static string applicationPath() {
            string r = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            r = Path.GetDirectoryName(r);
            r = r.Replace("file:/", "");
            r = r.Replace(@"file:\", "");
            return r;
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
