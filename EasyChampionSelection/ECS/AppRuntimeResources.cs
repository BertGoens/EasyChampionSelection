using EasyChampionSelection.Helper_Windows;
using RiotSharp;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection.ECS {
    public class AppRuntimeResources {
        private StaticTaskbarManager _tbm;
        private StaticGroupManager _gm;
        private Settings _s;
        private ChampionList _allChampions;

        private LolClientVisualHelper _lcvh;
        private bool _manuallyEnableTimerVisual = false;

        private wndClientOverload _wndCO;
        private wndMain _wndM;
        private wndContactCreator _wndCC;

        #region Getters & Setters

        public StaticTaskbarManager MyTaskbarManager {
            get { return _tbm; }
            private set { _tbm = value; }
        }

        public StaticGroupManager MyGroupManager {
            get { return _gm; }
            set { _gm = value; }
        }

        public Settings MySettings {
            get { return _s; }
            private set { _s = value; }
        }

        public ChampionList AllChampions {
            get { return _allChampions; }
            private set { _allChampions = value; }
        }

        public LolClientVisualHelper MyLolClientVisualHelper {
            get { return _lcvh; }
            private set { _lcvh = value; }
        }

        public wndClientOverload Window_ClientOverlay {
            get { return _wndCO; }
            private set { _wndCO = value; }
        }

        public wndMain Window_Main {
            get { return _wndM; }
            private set { _wndM = value; }
        }

        public bool ManuallyEnableTimerVisual {
            get { return _manuallyEnableTimerVisual; }
            set { _manuallyEnableTimerVisual = value; }
        }

        #endregion

        public AppRuntimeResources() {
            _tbm = StaticTaskbarManager.getInstance();

            _tbm.Tb.PreviewTrayContextMenuOpen += Tb_PreviewTrayContextMenuOpen; //Taskbar ContextMenu binding

            LoadSettings(); //Load this first
            LoadSerializedGroupManager();
            LoadAllChampions(); //Load all champions (Riot api or local)

            _lcvh = new LolClientVisualHelper(Window_ClientOverlay, MySettings, ManuallyEnableTimerVisual, UpdateClientOverlay, MyTaskbarManager);

            _wndM = new wndMain(this);
        }

        public void SaveSerializedData() {
            StaticSerializer.SerializeObject(_gm, StaticSerializer.FullPath_GroupManager);
            StaticSerializer.SerializeObject(_allChampions, StaticSerializer.FullPath_AllChampions);
            StaticSerializer.SerializeObject(_s, StaticSerializer.FullPath_Settings);
        }

        public void Dispose() {
            _tbm.Dispose();
        }

        #region LoadSerializedData

        private void LoadSettings() {
            if(File.Exists(StaticSerializer.FullPath_Settings)) {
                _s = (Settings)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_Settings);
                if(_s == null) {
                    _s = new Settings();
                }
            } else {
                _s = new Settings();
            }
        }

        private void LoadAllChampions() {
            _allChampions = new ChampionList("AllChampions");
            LoadAllChampionsRiotApi(); //Use api to get all champions
            if(_allChampions.getCount() < 1) {
                LoadAllChampionsLocal();
            }
        }

        public async void LoadAllChampionsRiotApi() {
            _tbm.DisplayPopup("Loading champions with API", false, null);
            _allChampions.RemoveAllChampions();
            if(_s.UserApiKey.Length == 36) {
                try {
                    StaticRiotApi staticApi = StaticRiotApi.GetInstance(_s.UserApiKey);
                    RiotSharp.StaticDataEndpoint.ChampionListStatic champions = await staticApi.GetChampionsAsync(RiotSharp.Region.euw, RiotSharp.StaticDataEndpoint.ChampionData.info, RiotSharp.Language.en_US);

                    for(int i = 0; i < champions.Champions.Count; i++) {
                        string ChampionName = champions.Champions.Values.ElementAt(i).Name;
                        _allChampions.AddChampion(ChampionName);
                    }

                } catch(RiotSharpException ex) {
                    _tbm.DisplayPopup("Trouble loading trough the api: \n" + ex.ToString(), true, null);
                } catch(NullReferenceException) {
                }
            } else {
                _tbm.DisplayPopup("No correct API key found, get one at https://developer.riotgames.com/", true, null);
            }

        }

        public void LoadAllChampionsLocal() {
            if(File.Exists(StaticSerializer.FullPath_AllChampions)) {
                _allChampions = (ChampionList)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_AllChampions);
            } else {
                _allChampions = new ChampionList("AllChamps");
            }
        }

        private void NewGroupManager() {
            _gm = StaticGroupManager.GetInstance();
        }

        private void LoadSerializedGroupManager() {
            if(File.Exists(StaticSerializer.FullPath_GroupManager)) {
                _gm = (StaticGroupManager)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_GroupManager);
                if(_gm == null) {
                    NewGroupManager();
                }
            } else {
                NewGroupManager();
            }
        }

        #endregion

        #region TaskbarIcon

        private MenuItem CreateMenuItem(String Header, RoutedEventHandler ClickEventHandler) {
            MenuItem mniMI = new MenuItem();
            mniMI.Header = Header;
            mniMI.Click += ClickEventHandler;
            return mniMI;
        }

        private void Tb_PreviewTrayContextMenuOpen(object sender, System.Windows.RoutedEventArgs e) {
            _tbm.Tb.ContextMenu = null;
            System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();

            if(_wndM == null) {
                _wndM = new wndMain(this);
                MenuItem mniHideMainWindow = CreateMenuItem("Show Main window", mniHideMainWindow_Click);
                cm.Items.Add(mniHideMainWindow);
            } else {
                if(_wndM.IsLoaded == false) { //Closed
                    _wndM = new wndMain(this);
                    MenuItem mniShowMainWindow = CreateMenuItem("Show Main window", mniShowMainWindow_Click);
                    cm.Items.Add(mniShowMainWindow);
                } else { //Not closed
                    if(_wndM.IsVisible) { //Visible
                        MenuItem mniHideMainWindow = CreateMenuItem("Hide Main window", mniHideMainWindow_Click);
                        cm.Items.Add(mniHideMainWindow);
                    } else { //Hidden in tray icon
                        MenuItem mniShowMainWindow = CreateMenuItem("Show Main window", mniShowMainWindow_Click);
                        cm.Items.Add(mniShowMainWindow);
                    }
                }
            }

            cm.Items.Add(new Separator());
            if(_manuallyEnableTimerVisual) {
                MenuItem mniManuallyEnableTimer = CreateMenuItem("Start timer", mniManuallyEnableTimer_Click);
                cm.Items.Add(mniManuallyEnableTimer);
            }

            MenuItem mniContactCreator = CreateMenuItem("Contact creator", mniContactCreator_Click);
            cm.Items.Add(mniContactCreator);

            cm.Items.Add(new Separator());

            MenuItem mniExit = CreateMenuItem("Exit", mniExit_Click);
            cm.Items.Add(mniExit);

            _tbm.Tb.ContextMenu = cm;
        }

        private void mniShowMainWindow_Click(object sender, RoutedEventArgs e) {
            _wndM.Show();
            _wndM.ShowInTaskbar = true;
        }

        private void mniHideMainWindow_Click(object sender, RoutedEventArgs e) {
            _wndM.Hide();
            _wndM.ShowInTaskbar = false;
        }

        private void mniManuallyEnableTimer_Click(object sender, RoutedEventArgs e) {
            _manuallyEnableTimerVisual = false;
            MyLolClientVisualHelper.StartTimer();
        }

        private void mniContactCreator_Click(object sender, RoutedEventArgs e) {
            if(_wndCC == null) {
                _wndCC = new wndContactCreator();
                _wndCC.Show();
            } else {
                _wndCC.Show();
            }
        }

        private void mniExit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        #endregion TaskbarIcon

        public void UpdateClientOverlay() {
            if(_wndCO != null) {
                if(_wndCO.IsLoaded) {
                    _wndCO.Close();
                }
            }
            _wndCO = new wndClientOverload(MyGroupManager, _lcvh._MyPinvokeLolClient);
            MyLolClientVisualHelper._wndCO = _wndCO;

            MyTaskbarManager.DisplayPopup("Your lolclient.exe process just got updated.", false, null);
        }
    }
}