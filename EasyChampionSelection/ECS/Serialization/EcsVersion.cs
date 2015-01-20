using System;
using System.Net;

namespace EasyChampionSelection.ECS.Serialization {
    [Serializable]
    public class EcsVersion {
        private string _version;
        private string _versionOnline;
        private int[] _versionDetailed;
        private DateTime _lastChecked = DateTime.Today.AddDays(-1);

        /// <summary>
        /// The amount of split unique numeric values in the <see cref="EcsVersion"/>EcsVersion.
        /// </summary>
        public const int versionParts = 4;

        /// <summary>
        /// The string representation of the <see cref="EcsVersion"/>EcsVersion.
        /// Eg: 1.0.0.0
        /// </summary>
        public string Version {
            get { return _version; }
            private set { _version = value; }
        }

        /// <summary>
        /// The string representation of the latest online <see cref="EcsVersion"/>EcsVersion.
        /// </summary>
        public string VersionOnline {
            get { return _versionOnline; }
            private set { _versionOnline = value; }
        }

        /// <summary>
        /// The numeric representation of the <see cref="EcsVersion"/>EcsVersion
        /// Eg: {1,0,0,0}
        /// </summary>
        public int[] VersionDetailed {
            get { return _versionDetailed; }
            private set { _versionDetailed = value; }
        }

        /// <summary>
        /// Read the last time this <see cref="EcsVersion"/>Version has been compared to another <see cref="EcsVersion"/>EcsVersion.
        /// </summary>
        public DateTime LastChecked {
            get { return _lastChecked; }
            private set { _lastChecked = value; }
        }

        /// <summary>
        /// Construct a <see cref="EcsVersion"/>EcsVersion object.
        /// </summary>
        /// <param name="version"></param>
        public EcsVersion(string version) {
            Version = version;
            string[] digits = _version.Split('.');
            VersionDetailed = new int[versionParts];
            for(int i = 0; i < versionParts; i++) {
                VersionDetailed[i] = int.Parse(digits[i]);
            }
        }

        /// <summary>
        /// Returns true if the given <see cref="EcsVersion"/>EcsVersion is newer than the current (this) one.
        /// </summary>
        /// <param name="ver">The 'new' <see cref="EcsVersion"/>EcsVersion to compare to (Format: 1.0.0.0)</param>
        /// <returns>True: parameter <see cref="EcsVersion"/>EcsVersion is newer / False: not newer.</returns>
        private bool isNewer(string ver) {
            if(ver == null) {
                return false;
            }
            LastChecked = DateTime.Today;

            string[] digits = ver.Split('.');
            int[] onlineVer = new int[versionParts];
            for(int i = 0; i < versionParts; i++) {
                onlineVer[i] = int.Parse(digits[i]);
            }

            for(int i = 0; i < versionParts; i++) {
                if(VersionDetailed[i] < onlineVer[i]) {
                    return true;
                }
            }
            return false;
        }

        public override string ToString() {
            return Version;
        }

        /// <summary>
        /// Checks the lastest online version (refreshes once a day)
        /// On a second check it just returns the last mined answer
        /// </summary>
        public bool isOnlineVersionNewer() {
            if(LastChecked < DateTime.Today) {
                string onlineVer;
                try {
                    using(WebClient client = new WebClient()) {
                        onlineVer = client.DownloadString(@"https://github.com/BertGoens/EasyChampionSelection/blob/master/Version.txt");
                    }

                } catch(Exception) {
                    onlineVer = null;
                }
                if(onlineVer != null) {
                    _versionOnline = onlineVer;
                }
            }

            return isNewer(_versionOnline);
        }

    }
}
