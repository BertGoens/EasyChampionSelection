using EasyChampionSelection.Helper_Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace EasyChampionSelection.ECS {
    /// <summary>
    /// The visual code of the lolClient.
    /// </summary>
    public class LolClientVisualHelper {

        #region Properties

        private Settings _MySettings;
        private Action _UpdateClientOverlay;
        private Action<string, bool, Window> _DisplayPopup;

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

        public LolClientVisualHelper(Settings MySettings, Action UpdateClientOverlay, Action<string, bool, Window> DisplayPopup) : this() {
            _MySettings = MySettings;
            _UpdateClientOverlay = UpdateClientOverlay;
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
        }

        private void tmrCheckForChampSelect_Tick(object sender, EventArgs e) {
            _tmrCheckForChampSelect.Stop();

            try {
                //Check if player is ingame
                Process[] gameClient = Process.GetProcessesByName("League of Legends");
                if(gameClient.Length > 0) {
                    _tmrCheckForChampSelect.Interval = _tmspTimerAfkInterval;
                } else {
                    _tmrCheckForChampSelect.Interval = _tmspTimerClienActiveInterval;

                    Process[] p = Process.GetProcessesByName("lolclient"); //Look for the lolClient process
                    if( _MyPinvokeLolClient == null) {
                        if(p.Length > 0) {
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
                        if(p[i].Id ==  _MyPinvokeLolClient.getProcessLolClient().Id) {
                            clientExists = true;
                            break;
                        }
                    }

                    if(!clientExists) {
                        NewStaticLolClientGraphics(p[0]);
                    }

                    if( _MyPinvokeLolClient.isLolClientFocussed() ||  _MyPinvokeLolClient.isEasyChampionSelectionFoccussed()) {
                        if( _MyPinvokeLolClient.isInChampSelect()) {
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

                 _MyPinvokeLolClient = StaticPinvokeLolClient.GetInstance(LeagueOfLegendsClientProcess, _MySettings);

                 _UpdateClientOverlay();
            }
        }
    }
}