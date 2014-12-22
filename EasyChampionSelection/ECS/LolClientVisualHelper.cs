using EasyChampionSelection.Helper_Windows;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace EasyChampionSelection.ECS {
    public class LolClientVisualHelper {

        #region Properties

        private Settings _MySettings;
        private Action _UpdateClientOverlay;
        private StaticTaskbarManager _tbm;
        public wndClientOverload _wndCO;

        public StaticPinvokeLolClient _MyPinvokeLolClient;
        private bool _ManuallyEnableTimerVisual;

        private TimeSpan _tmspTimerClienActiveInterval;
        private TimeSpan _tmspTimerAfkInterval;
        private DispatcherTimer _tmrCheckForChampSelect;
        #endregion

        #region Constructors

        private LolClientVisualHelper() {
            _tmspTimerClienActiveInterval = new TimeSpan(0, 0, 1);
            _tmspTimerAfkInterval = new TimeSpan(0, 0, 10);
            _tmrCheckForChampSelect = new DispatcherTimer(DispatcherPriority.Background);
            _tmrCheckForChampSelect.Interval = _tmspTimerClienActiveInterval;
            _tmrCheckForChampSelect.Tick += tmrCheckForChampSelect_Tick;
        }

        public LolClientVisualHelper(wndClientOverload wndCO, Settings MySettings, bool ManuallyEnableTimerVisual, Action UpdateClientOverlay, StaticTaskbarManager tbm) : this() {
            _tbm = tbm;
            _wndCO = wndCO;
            _MySettings = MySettings;
            _UpdateClientOverlay = UpdateClientOverlay;
            _ManuallyEnableTimerVisual = ManuallyEnableTimerVisual;
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
                            _wndCO.Visibility = System.Windows.Visibility.Visible;
                        } else {
                            _wndCO.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }

                }
            } catch(Exception ex) {
                _ManuallyEnableTimerVisual = true;
                wndErrorHelper wndEH = new wndErrorHelper(ex, _tbm);
                wndEH.ShowDialog();
                _tmrCheckForChampSelect.Stop();
                return;
            }
            _tmrCheckForChampSelect.Start();
        }

        private void DeleteOldStaticLolClientGraphics() {
            if(_wndCO != null) {
                _wndCO.Close();
                _wndCO = null;
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