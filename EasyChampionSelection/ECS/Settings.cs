using System;
using System.Drawing;

namespace EasyChampionSelection.ECS {
    [Serializable]
    public class Settings {
        
        private Rectangle _rChampionSearchbarRelativePos;
        private Rectangle _rTeamChatRelativePos; 
        private Rectangle _rClientOverlayRelativePos;

        private bool _showMainFormOnLaunch;
        private bool _startOnBoot;
        private string _userApiKey;

       #region Getters & Setters
        /// <summary>
        /// Get or set the relative postition of the Champion Searchbar.
        /// </summary>
        public Rectangle ChampionSearchbarRelativePos {
            get { return _rChampionSearchbarRelativePos; }
            set { _rChampionSearchbarRelativePos = value; }
        }

        /// <summary>
        /// Get or set the relative postition of the Team Chat
        /// </summary>
        public Rectangle TeamChatRelativePos {
            get { return _rTeamChatRelativePos; }
            set { _rTeamChatRelativePos = value; }
        }

        /// <summary>
        /// Get or set the relative postition of the Client Overlay
        /// </summary>
        public Rectangle ClientOverlayRelativePos {
            get { return _rClientOverlayRelativePos; }
            set { _rClientOverlayRelativePos = value; }
        }

        /// <summary>
        /// Get or set if the main form (opening one) should be hidden into the tray by default
        /// </summary>
        public bool ShowMainFormOnLaunch {
            get { return _showMainFormOnLaunch; }
            set { _showMainFormOnLaunch = value; }
        }

        /// <summary>
        /// Get or set if the programn should start when the computer starts
        /// </summary>
        public bool StartOnBoot {
            get { return _startOnBoot; }
            set { _startOnBoot = value; }
        }

        /// <summary>
        /// Get or set the user given API key
        /// </summary>
        public string UserApiKey {
            get { return _userApiKey; }
            set { _userApiKey = value; }
        }
        #endregion Getters & Setters

        public Settings() {
            this._showMainFormOnLaunch = true;
            this._startOnBoot = false;
            this._userApiKey = "";
            this._rChampionSearchbarRelativePos = new Rectangle();
            this._rClientOverlayRelativePos = new Rectangle();
            this._rTeamChatRelativePos = new Rectangle();
        }
    }
}
