using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace EasyChampionSelection.ECS {
    [Serializable]
    public class Settings {

        private Rectangle _rChampionSearchbarRelativePos = new Rectangle();
        private Rectangle _rTeamChatRelativePos = new Rectangle();
        private Rectangle _rClientOverlayRelativePos = new Rectangle();

        [OptionalField]
        private bool _startLeagueWithEcs = false;
        private bool _showMainFormOnLaunch = true;
        private string _userApiKey = "";
        [OptionalField]
        private string _LeaguePath = "";

        #region events
        public delegate void ChangedEventHandler(Settings sender, EventArgs e);

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
        [field:NonSerialized]
        public event ChangedEventHandler ApiKeyChanged;

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
                }
            }
        }

        #endregion Getters & Setters

        public Settings() {}
    }
}
