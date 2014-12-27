using EasyChampionSelection.Helper_Windows;
using RiotSharp;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection.ECS {
    public class AppRuntimeResources {
        public const string AppName = "Easy Champion Selection";

        private StaticTaskbarManager _tbm;
        private Action<string, bool, Window> _displayPopup;

        private StaticGroupManager _gm;
        private Settings _s;
        private ChampionList _allChampions;

        private LolClientVisualHelper _lcvh;

        private wndMain _wndM;
        private wndContactCreator _wndCC;

        #region Getters & Setters

        private StaticTaskbarManager MyTaskbarManager {
            get { return _tbm; }
            set 
            {
                _tbm = value;
                _displayPopup = _tbm.DisplayPopup;
            }
        }

        /// <summary>
        /// Routed method from StaticTaskbarManager
        /// </summary>
        public Action<string, bool, Window> DisplayPopup {
            get { return _displayPopup; }
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

        /// <summary>
        /// Routed property from LolClientVisualHelper
        /// </summary>
        public wndClientOverload Window_ClientOverlay {
            get
            {
                if(MyLolClientVisualHelper == null) {
                    return null;
                }
                if(MyLolClientVisualHelper.Window_ClientOverlay == null) {
                    return null;
                }
                return MyLolClientVisualHelper.Window_ClientOverlay;
            }
            private set 
            {
                if(MyLolClientVisualHelper != null) {
                    MyLolClientVisualHelper.Window_ClientOverlay = value; 
                }
            }
        }

        public wndMain Window_Main {
            get { return _wndM; }
            private set { _wndM = value; }
        }

        private wndContactCreator Window_ContactCreator {
            get { return _wndCC; }
            set { _wndCC = value; }
        }

        /// <summary>
        /// Routed property from LolClientVisualHelper
        /// </summary>
        public bool ManuallyEnableTimerVisual {
            get { return MyLolClientVisualHelper.ManuallyEnableTimerVisual; }
            set { MyLolClientVisualHelper.ManuallyEnableTimerVisual = value; }
        }

        #endregion

        public AppRuntimeResources() {
            MyTaskbarManager = StaticTaskbarManager.getInstance(AppName);
            MyTaskbarManager.MyTaskbarIcon.PreviewTrayContextMenuOpen += Tb_PreviewTrayContextMenuOpen; //Taskbar ContextMenu binding

            LoadSettings(); //Load this first
            LoadSerializedGroupManager();
            LoadAllChampions(); //Load all champions (Riot api or local)

            MyLolClientVisualHelper = new LolClientVisualHelper(MySettings, UpdateClientOverlay, DisplayPopup);

            Window_Main = new wndMain(this);
        }

        public void SaveSerializedData() {
            StaticSerializer.SerializeObject(MyGroupManager, StaticSerializer.FullPath_GroupManager);
            StaticSerializer.SerializeObject(AllChampions, StaticSerializer.FullPath_AllChampions);
            StaticSerializer.SerializeObject(MySettings, StaticSerializer.FullPath_Settings);
        }

        public void Dispose() {
            MyTaskbarManager.Dispose();
        }

        #region LoadSerializedData

        private void LoadSettings() {
            if(File.Exists(StaticSerializer.FullPath_Settings)) {
                MySettings = (Settings)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_Settings);
                if(MySettings == null) {
                    MySettings = new Settings();
                }
            } else {
                MySettings = new Settings();
            }
        }

        private void LoadAllChampions() {
            AllChampions = new ChampionList("AllChampions");
            LoadAllChampionsRiotApi(); //Use api to get all champions
            if(AllChampions.getCount() < 1) {
                LoadAllChampionsLocal();
            }
        }

        public async void LoadAllChampionsRiotApi() {
            DisplayPopup("Loading champions with API", false, null);
            AllChampions.RemoveAllChampions();
            if(MySettings.UserApiKey.Length == 36) {
                try {
                    StaticRiotApi staticApi = StaticRiotApi.GetInstance(_s.UserApiKey);
                    RiotSharp.StaticDataEndpoint.ChampionListStatic champions = await staticApi.GetChampionsAsync(RiotSharp.Region.euw, RiotSharp.StaticDataEndpoint.ChampionData.info, RiotSharp.Language.en_US);

                    for(int i = 0; i < champions.Champions.Count; i++) {
                        string ChampionName = champions.Champions.Values.ElementAt(i).Name;
                        AllChampions.AddChampion(ChampionName);
                    }

                } catch(RiotSharpException ex) {
                    DisplayPopup("Trouble loading trough the api: \n" + ex.ToString(), true, null);
                } catch(NullReferenceException) {
                }
            } else {
               DisplayPopup("No correct API key found, get one at https://developer.riotgames.com/", true, null);
            }

        }

        public void LoadAllChampionsLocal() {
            if(File.Exists(StaticSerializer.FullPath_AllChampions)) {
                AllChampions = (ChampionList)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_AllChampions);
            } else {
                AllChampions = new ChampionList("AllChamps");
            }
        }

        private void NewGroupManager() {
            MyGroupManager = StaticGroupManager.GetInstance();
        }

        private void LoadSerializedGroupManager() {
            if(File.Exists(StaticSerializer.FullPath_GroupManager)) {
                MyGroupManager = (StaticGroupManager)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_GroupManager);
                if(MyGroupManager == null) {
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
            MyTaskbarManager.MyTaskbarIcon.ContextMenu = null;
            System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();

            if(Window_Main == null) {
                Window_Main = new wndMain(this);
                MenuItem mniHideMainWindow = CreateMenuItem("Show", mniHideMainWindow_Click);
                cm.Items.Add(mniHideMainWindow);
            } else {
                if(Window_Main.IsLoaded == false) { //Closed
                    Window_Main = new wndMain(this);
                    MenuItem mniShowMainWindow = CreateMenuItem("Show", mniShowMainWindow_Click);
                    cm.Items.Add(mniShowMainWindow);
                } else { //Not closed
                    if(Window_Main.IsVisible) { //Visible
                        MenuItem mniHideMainWindow = CreateMenuItem("Hide", mniHideMainWindow_Click);
                        cm.Items.Add(mniHideMainWindow);
                    } else { //Hidden in tray icon
                        MenuItem mniShowMainWindow = CreateMenuItem("Show", mniShowMainWindow_Click);
                        cm.Items.Add(mniShowMainWindow);
                    }
                }
            }

            cm.Items.Add(new Separator());
            if(ManuallyEnableTimerVisual) {
                MenuItem mniManuallyEnableTimer = CreateMenuItem("Start timer", mniManuallyEnableTimer_Click);
                cm.Items.Add(mniManuallyEnableTimer);
            }

            MenuItem mniContactCreator = CreateMenuItem("Contact creator", mniContactCreator_Click);
            cm.Items.Add(mniContactCreator);

            cm.Items.Add(new Separator());

            MenuItem mniExit = CreateMenuItem("Exit", mniExit_Click);
            cm.Items.Add(mniExit);

            MyTaskbarManager.MyTaskbarIcon.ContextMenu = cm;
        }

        private void mniShowMainWindow_Click(object sender, RoutedEventArgs e) {
            Window_Main.Show();
            Window_Main.ShowInTaskbar = true;
        }

        private void mniHideMainWindow_Click(object sender, RoutedEventArgs e) {
            Window_Main.Hide();
            Window_Main.ShowInTaskbar = false;
        }

        private void mniManuallyEnableTimer_Click(object sender, RoutedEventArgs e) {
            ManuallyEnableTimerVisual = false;
            MyLolClientVisualHelper.StartTimer();
        }

        private void mniContactCreator_Click(object sender, RoutedEventArgs e) {
            if(Window_ContactCreator == null) {
                Window_ContactCreator = new wndContactCreator();
                Window_ContactCreator.Show();
            } else {
                Window_ContactCreator.Show();
            }
        }

        private void mniExit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        #endregion TaskbarIcon

        public void UpdateClientOverlay() {
            if(Window_ClientOverlay != null) {
                if(Window_ClientOverlay.IsLoaded) {
                    Window_ClientOverlay.Close();
                }
            }

            Window_ClientOverlay = new wndClientOverload(MyGroupManager, MyLolClientVisualHelper.MyPinvokeLolClient, DisplayPopup);

            DisplayPopup("Your lolclient.exe process just got updated.", false, null);
        }
    }
}