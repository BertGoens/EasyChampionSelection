using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A basic (de)serialization class
    /// </summary>
    public static class StaticSerializer {
        public const string PATH_FolderForSaveData = "Save/";
        public const string PATH_GroupManager = "Groups.ser";
        public const string PATH_AllChampions = "AllChampions.ser";
        public const string PATH_Settings = "Settings.ser";

        public static void SerializeObject(object objectToSerialize, string fileName) {
            try {
                Directory.CreateDirectory(fileName.Substring(0, fileName.LastIndexOf("/")));
                Stream stream = File.Open(fileName, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
                stream.Close();
            } catch(DirectoryNotFoundException) {
            }

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
                } catch(SerializationException) {
                    //Attempting to deserialize an empty stream, just delete it as it is void
                    stream.Close();
                    File.Delete(fileName);
                }
            }

            return null;
        }
    }
}
