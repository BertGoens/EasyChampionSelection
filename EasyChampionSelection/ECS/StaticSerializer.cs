using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A basic (de)serialization class
    /// </summary>
    public static class StaticSerializer {

        public const string PATH_GroupManager = "Groups.ser";
        public const string PATH_AllChampions = "AllChampions.ser";

        public static void SerializeObject(object objectToSerialize, string location) {
            Stream stream = File.Open(location, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, objectToSerialize);
            stream.Close();
        }

        public static object DeSerializeObject(string location) {
            if(File.Exists(location)) {
                object objectToDeSerialize;
                Stream stream;
                try {
                    stream = File.Open(location, FileMode.Open);
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
                    File.Delete(location);
                }
            }

            return null;
        }
    }
}
