using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace EasyChampionSelection.ECS {

    /// <summary>
    /// A basic (de)serialization class
    /// It will automatically create directories of requested files if needed.
    /// </summary>
    public static class StaticSerializer {
        // @\Save\
        private static string _fullPath_ClientImage = applicationPath() + Folder_SaveData + Object_ClientImage;
        private static string _fullPath_Settings = applicationPath() + Folder_SaveData + Object_Settings;
        private static string _fullPath_AllChampions = applicationPath() + Folder_SaveData + Object_AllChampions;
        private static string _fullPath_GroupManager = applicationPath() + Folder_SaveData + Object_GroupManager;

        // @\Error\
        private static string _fullPath_ErrorFile = applicationPath() + Folder_ErrorData + DateTime.Today.ToString("d").Replace("/", "_") + ".txt";

        //Local folders
        private const string Folder_SaveData = @"\Save\";
        private const string Folder_ErrorData = @"\Error\";

        //Local files
        private const string Object_GroupManager = "Groups.ser";
        private const string Object_AllChampions = "AllChampions.ser";
        private const string Object_Settings = "Settings.ser";
        private const string Object_ClientImage = "ClientImage.jpg";

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
                
                if (HasFolderWritePermission(dirToPath)) { //Check if we need admin rights to write the directory
                    Directory.CreateDirectory(dirToPath);
                } else {
                    CreateEndDirectoryAdminRights(dirToPath); 
                }
            }
        }

        /// <summary>
        /// Will check if we have access to write to a directory
        /// </summary>
        private static bool HasFolderWritePermission(string destDir) {
            if(string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
                return false;
            try {
                DirectorySecurity security = Directory.GetAccessControl(destDir);
                SecurityIdentifier users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                foreach(AuthorizationRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier))) {
                    if(rule.IdentityReference == users) {
                        FileSystemAccessRule rights = ((FileSystemAccessRule)rule);
                        if(rights.AccessControlType == AccessControlType.Allow) {
                            if(rights.FileSystemRights == (rights.FileSystemRights | FileSystemRights.Modify))
                                return true;
                        }
                    }
                }
                return false;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Will require admin rights to create a directory
        /// </summary>
        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private static void CreateEndDirectoryAdminRights(string dir) {
            Directory.CreateDirectory(dir);
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