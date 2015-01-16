using EasyChampionSelection.ECS.AppRuntimeResources;
using EasyChampionSelection.Helper_Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace EasyChampionSelection.ECS.AppRuntimeResources.LolClient {
    /// <summary>
    /// The visual code of the lolClient.
    /// </summary>
    public class LolClientVisualHelper {

        public delegate void LolClientVisualHelperHanlder(LolClientVisualHelper sender, EventArgs e);

        /// <summary>
        /// Occurs when a new league client is found
        /// There is only 1 lolclient.exe possible so the old one gets deleted.
        /// </summary>
        [field: NonSerialized]
        public event LolClientVisualHelperHanlder NewLeagueClient;

        /// <summary>
        /// Occurs when an old client is closed
        /// </summary>
        [field: NonSerialized]
        public event LolClientVisualHelperHanlder ClientClosed;

        #region Properties

        private EcsSettings _MySettings;
        private Action<string> _DisplayPopup;

        private wndClientOverload _wndCO;
        private StaticPinvokeLolClient _MyPinvokeLolClient; // The not visual code is all in here
        private bool _ManuallyEnableTimerVisual;

        private TimeSpan _tmspTimerClienActiveInterval;
        private TimeSpan _tmspTimerAfkInterval;
        private DispatcherTimer _tmrCheckForChampSelect;
        #endregion

        public wndClientOverload Window_ClientOverlay {
            get { return _wndCO; }
            set { _wndCO = value; }
        }

        public StaticPinvokeLolClient MyPinvokeLolClient {
            get { return _MyPinvokeLolClient; }
            private set { _MyPinvokeLolClient = value; }
        }

        public bool ManuallyEnableTimerVisual {
            get { return _ManuallyEnableTimerVisual; }
            set { _ManuallyEnableTimerVisual = value; }
        }

        #region Constructors

        private LolClientVisualHelper() {
            _tmspTimerClienActiveInterval = new TimeSpan(0, 0, 1);
            _tmspTimerAfkInterval = new TimeSpan(0, 0, 10);
            _tmrCheckForChampSelect = new DispatcherTimer(DispatcherPriority.Background);
            _tmrCheckForChampSelect.Interval = _tmspTimerClienActiveInterval;
            _tmrCheckForChampSelect.Tick += tmrCheckForChampSelect_Tick;
        }

        public LolClientVisualHelper(EcsSettings MySettings, Action<string> DisplayPopup)
            : this() {
            _MySettings = MySettings;
            _DisplayPopup = DisplayPopup;
            StartTimer();
        }

        #endregion

        public void StartTimer() {
            _tmrCheckForChampSelect.Start();
        }

        private void UserClosedClient() {
            _tmrCheckForChampSelect.Interval = _tmspTimerAfkInterval;
            DeleteOldStaticLolClientGraphics(); //Delete all references to client dependent classes with events
            _tmrCheckForChampSelect.Start();
            if(ClientClosed != null) {
                ClientClosed(this, EventArgs.Empty);
            }
        }

        public static bool isIngame() {
            Process[] gameClient = Process.GetProcessesByName("League of Legends");
            if(gameClient.Length > 0) {
                return true;
            }
            return false;
        }

        public static bool isLolClientStarted() {
            Process[] p = Process.GetProcessesByName("lolclient"); //Look for the lolClient process
            if(p.Length > 0) {
                return true;
            }
            return false;
        }

        private void tmrCheckForChampSelect_Tick(object sender, EventArgs e) {
            _tmrCheckForChampSelect.Stop();

            try {
                //Check if player is ingame
                if(isIngame()) {
                    _tmrCheckForChampSelect.Interval = _tmspTimerAfkInterval;
                } else {
                    _tmrCheckForChampSelect.Interval = _tmspTimerClienActiveInterval;

                    Process[] p = Process.GetProcessesByName("lolclient"); //Look for the lolClient process
                    if(_MyPinvokeLolClient == null) {
                        if(isLolClientStarted()) {
                            NewStaticLolClientGraphics(p[0]);
                        } else {
                            UserClosedClient();
                            return;
                        }
                    }

                    if(p.Length == 0) {
                        UserClosedClient();
                        return;
                    }

                    bool clientExists = false;
                    for(int i = 0; i < p.Length; i++) {
                        if(p[i].Id == _MyPinvokeLolClient.getProcessLolClient().Id) {
                            clientExists = true;
                            break;
                        }
                    }

                    if(!clientExists) {
                        NewStaticLolClientGraphics(p[0]);
                    }

                    if(_MyPinvokeLolClient.isLolClientFocussed() || _MyPinvokeLolClient.isEasyChampionSelectionFoccussed()) {
                        if(_MyPinvokeLolClient.isInChampSelect()) {
                            Window_ClientOverlay.Visibility = System.Windows.Visibility.Visible;
                        } else {
                            Window_ClientOverlay.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }

                }
            } catch(Exception ex) {
                _ManuallyEnableTimerVisual = true;
                wndErrorHelper wndEH = new wndErrorHelper(ex, _DisplayPopup);
                wndEH.ShowDialog();
                _tmrCheckForChampSelect.Stop();
                return;
            }
            _tmrCheckForChampSelect.Start();
        }

        private void DeleteOldStaticLolClientGraphics() {
            if(Window_ClientOverlay != null) {
                Window_ClientOverlay.Close();
                Window_ClientOverlay = null;
            }

            _MyPinvokeLolClient = null;
        }

        private void NewStaticLolClientGraphics(Process LeagueOfLegendsClientProcess) {
            if(LeagueOfLegendsClientProcess != null) {
                DeleteOldStaticLolClientGraphics();

                _MyPinvokeLolClient = null;
                _MyPinvokeLolClient = StaticPinvokeLolClient.GetInstance(LeagueOfLegendsClientProcess, _MySettings);

                if(NewLeagueClient != null) {
                    NewLeagueClient(this, EventArgs.Empty);
                }
            }
        }
    }
}