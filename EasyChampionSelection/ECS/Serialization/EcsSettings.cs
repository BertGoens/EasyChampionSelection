using System;
using System.Drawing;

namespace EasyChampionSelection.ECS.Serialization {
    /// <summary>
    /// Settings class
    /// </summary>
    [Serializable]
    public class EcsSettings {
        private Rectangle _rChampionSearchbarRelativePos = new Rectangle();
        private Rectangle _rTeamChatRelativePos = new Rectangle();
        private Rectangle _rClientOverlayRelativePos = new Rectangle();

        private bool _startLeagueWithEcs;
        private bool _showMainFormOnLaunch;
        private string _userApiKey;
        private string _LeaguePath;
        private EcsVersion _ecsVersion;

        #region events
        public delegate void ChangedEventHandler(EcsSettings sender, EventArgs e);

        /// <summary>
        /// Occurs when the ChampionSearchbar is moved or reseized.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler ChampionSearchbarChanged;

        /// <summary>
        /// Occurs when the TeamChatbar is moved or reseized.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler TeamChatChanged;

        /// <summary>
        /// Occurs when the Client Overlay is moved or reseized.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler ClientOverlayChanged;

        /// <summary>
        /// Occurs when the api key is changed.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler ApiKeyChanged;

        /// <summary>
        /// Occurs when the settings are updated.
        /// </summary>
        [field: NonSerialized]
        public event ChangedEventHandler SettingsChanged;

        #endregion events
        #region Getters & Setters
        /// <summary>
        /// Get or set the relative postition of the Champion Searchbar.
        /// </summary>
        public Rectangle ChampionSearchbarRelativePos {
            get { return _rChampionSearchbarRelativePos; }
            set {
                if(value != null && value != _rChampionSearchbarRelativePos) {
                    _rChampionSearchbarRelativePos = value;
                    if(ChampionSearchbarChanged != null) {
                        ChampionSearchbarChanged(this, EventArgs.Empty);
                    }
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set the relative postition of the Team Chat
        /// </summary>
        public Rectangle TeamChatRelativePos {
            get { return _rTeamChatRelativePos; }
            set {
                if(value != null && value != _rTeamChatRelativePos) {
                    _rTeamChatRelativePos = value;
                    if(TeamChatChanged != null) {
                        TeamChatChanged(this, EventArgs.Empty);
                    }
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set the relative postition of the Client Overlay
        /// </summary>
        public Rectangle ClientOverlayRelativePos {
            get { return _rClientOverlayRelativePos; }
            set {
                if(value != null && value != _rClientOverlayRelativePos) {
                    _rClientOverlayRelativePos = value;
                    if(ClientOverlayChanged != null) {
                        ClientOverlayChanged(this, EventArgs.Empty);
                    }
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set if the main form (opening one) should be hidden into the tray by default
        /// </summary>
        public bool ShowMainFormOnLaunch {
            get { return _showMainFormOnLaunch; }
            set {
                if(value != _showMainFormOnLaunch) {
                    _showMainFormOnLaunch = value;
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set if League Of Legends.exe should start whenever the user starts Easy Champion Selection
        /// </summary>
        public bool StartLeagueWithEcs {
            get { return _startLeagueWithEcs; }
            set {
                if(value != _startLeagueWithEcs) {
                    _startLeagueWithEcs = value;
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set the Path to the league of legends launcher executable.
        /// Likely C:/Riot Games/League of Legends/lol.launcher.exe
        /// </summary>
        public string LeaguePath {
            get { return _LeaguePath; }
            set {
                if(value != null && value != _LeaguePath) {
                    _LeaguePath = value;
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set the user given API key
        /// </summary>
        public string UserApiKey {
            get { return _userApiKey; }
            set {
                if(value != null && value != _userApiKey && (value.Length == 36 || value.Length == 0)) {
                    _userApiKey = value;
                    if(ApiKeyChanged != null) {
                        ApiKeyChanged(this, EventArgs.Empty);
                    }
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Returns this <see cref="EcsVersion"/>version of Easy Champion Selection
        /// </summary>
        public EcsVersion EcsVersion {
            get { return _ecsVersion; }
            set {
                if(value != _ecsVersion) {
                    _ecsVersion = value;
                    if(SettingsChanged != null) {
                        SettingsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        #endregion Getters & Setters

        public EcsSettings() {
            _startLeagueWithEcs = false;
            _showMainFormOnLaunch = true;
            _userApiKey = "";
            _LeaguePath = "";
            _ecsVersion = new EcsVersion("1.0.0.0");
        }
    }
}