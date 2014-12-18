using System;
using System.Drawing;

namespace EasyChampionSelection.ECS {
    [Serializable]
    public class Settings {

        private Rectangle _rChampionSearchbarRelativePos;
        private Rectangle _rTeamChatRelativePos;
        private Rectangle _rClientOverlayRelativePos;

        private bool _showMainFormOnLaunch;
        private string _userApiKey;

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
        /// Get or set the user given API key
        /// </summary>
        public string UserApiKey {
            get { return _userApiKey; }
            set {
                if(value != null && value != _userApiKey && value.Length == 36) {
                    _userApiKey = value;
                    if(ApiKeyChanged != null) {
                        ApiKeyChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        #endregion Getters & Setters

        public Settings() {
            this._showMainFormOnLaunch = true;
            this._userApiKey = "";
            this._rChampionSearchbarRelativePos = new Rectangle();
            this._rClientOverlayRelativePos = new Rectangle();
            this._rTeamChatRelativePos = new Rectangle();
        }
    }
}
