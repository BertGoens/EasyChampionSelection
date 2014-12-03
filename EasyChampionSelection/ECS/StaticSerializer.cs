using System.Collections.Generic;
using System.IO;
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
            try {
                object objectToDeSerialize;
                Stream stream = File.Open(location, FileMode.Open);
                BinaryFormatter bFormatter = new BinaryFormatter();
                objectToDeSerialize = bFormatter.Deserialize(stream);
                stream.Close();
                return objectToDeSerialize;
            } catch(IOException) {}
            return null;
        }
    }
}
