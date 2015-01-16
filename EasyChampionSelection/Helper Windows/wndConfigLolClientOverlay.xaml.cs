using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.AppRuntimeResources;
using EasyChampionSelection.ECS.AppRuntimeResources.LolClient;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndConfigLolClientOverlay.xaml
    /// </summary>
    public partial class wndConfigLolClientOverlay : Window {
        private StaticPinvokeLolClient _lcg;
        private EcsSettings _ecsSettings;
        private const int _cBaseThumbWidth = 50;
        private const int _cBaseThumbHeight = 50;
        private const int _cBaseImageWidth = 800;
        private const int _cBaseImageHeight = 600;
        private Bitmap _ClientBitmap;
        private BitmapSource _lolClientImage;
        private bool recursiveFlag = false;
        private Action<string> _displayPopup;

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int SetActiveWindow(int hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);
        private enum ShowWindowEnum {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11
        };

        private void BringWindowToFront() {
            //get the process
            Process[] bProcess = Process.GetProcessesByName("lolclient");
            //check if the process is nothing or not.
            if(bProcess.Length > 0) {
                //get the (int) hWnd of the process
                int hwnd = (int)bProcess[0].MainWindowHandle;
                //check if its nothing
                if(hwnd != 0) {
                    //if the handle is other than 0, then set the active window
                    SetActiveWindow(hwnd);
                } else {
                    //we can assume that it is fully hidden or minimized, so lets show it!
                    ShowWindow(bProcess[0].Handle, ShowWindowEnum.Restore);
                    SetActiveWindow((int)bProcess[0].MainWindowHandle);
                }
            }
        }

        private wndConfigLolClientOverlay() {
            InitializeComponent();
        }

        public wndConfigLolClientOverlay(StaticPinvokeLolClient lcg, EcsSettings ecsSettings, Action<string> DisplayPopup)
            : this() {
            if(ecsSettings == null || DisplayPopup == null) {
                throw new ArgumentNullException();
            }

            _lcg = lcg;
            _ecsSettings = ecsSettings;
            _displayPopup = DisplayPopup;

            if(File.Exists(StaticSerializer.FullPath_ClientImage)) {
                _ClientBitmap = new Bitmap(StaticImageUtilities.LoadImageFromFile(StaticSerializer.FullPath_ClientImage));
                _lolClientImage = StaticImageUtilities.BitmapToBitmapSource(_ClientBitmap);
                Visualize_lolClientImage();
            } else {
                btnGetCurrentClientImage_Click(null, null);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //Save last rdb
            if(rdbChampionSearchbar.IsChecked == true) {
                rdbChampionSearchbar_SavePosition(null, null);
            } else if(rdbTeamChat.IsChecked == true) {
                rdbTeamChat_SavePosition(null, null);
            } else if(rdbClientOverlay.IsChecked == true) {
                rdbClientOverlay_SavePosition(null, null);
            }

            if(_lolClientImage != null) {
                try {
                    _ClientBitmap.Save(StaticSerializer.FullPath_ClientImage);
                    _ClientBitmap.Dispose();
                } catch(Exception ex) {
                    ex.ToString();
                }
            }
        }

        private void btnGetCurrentClientImage_Click(object sender, RoutedEventArgs e) {
            if(_lcg != null) {
                _ClientBitmap = _lcg.GetLeagueClientAsBitmap(); // Get bitmap of league client
                _lolClientImage = StaticImageUtilities.BitmapToBitmapSource(_ClientBitmap); //Convert bitmap to bitmapsource

                if(_lolClientImage != null) {
                    Visualize_lolClientImage();
                }
            } else {
                _displayPopup("No league client found. if you just started one please re-open this window.");
            }
        }

        private void rdbOnChecked(object sender, RoutedEventArgs e) {
            //Saved position rectangle
            RadioButton rdbSender = (RadioButton)sender;
            Rectangle SavedPosition = new Rectangle();
            if(rdbSender.Equals(rdbChampionSearchbar)) {
                SavedPosition = _ecsSettings.ChampionSearchbarRelativePos;
            } else if(rdbSender.Equals(rdbTeamChat)) {
                SavedPosition = _ecsSettings.TeamChatRelativePos;
            } else if(rdbSender.Equals(rdbClientOverlay)) {
                SavedPosition = _ecsSettings.ClientOverlayRelativePos;
            }

            if(SavedPosition.Width > 0 && SavedPosition.Height > 0) {
                TransformRectangle(ccPos, SavedPosition);
            } else {
                CreateNewThumb();
            }

            ccPos.Visibility = System.Windows.Visibility.Visible;
        }

        private void rdbChampionSearchbar_SavePosition(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(ccPos), (int)Canvas.GetTop(ccPos), (int)ccPos.Width, (int)ccPos.Height);
            _ecsSettings.ChampionSearchbarRelativePos = repositionRectangle;
        }

        private void rdbTeamChat_SavePosition(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(ccPos), (int)Canvas.GetTop(ccPos), (int)ccPos.Width, (int)ccPos.Height);
            _ecsSettings.TeamChatRelativePos = repositionRectangle;
        }

        private void rdbClientOverlay_SavePosition(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(ccPos), (int)Canvas.GetTop(ccPos), (int)ccPos.Width, (int)ccPos.Height);
            _ecsSettings.ClientOverlayRelativePos = repositionRectangle;
        }

        private void thmbPos_OnDragDelta(object sender, DragDeltaEventArgs e) {
            Thumb tSender = (Thumb)sender;

            int tLeft = (int)Canvas.GetLeft(tSender);
            int tRight = tLeft + (int)tSender.Width;

            if(tLeft + e.HorizontalChange > -1) {
                if(tRight + e.HorizontalChange < cvRectangles.Width) {
                    Canvas.SetLeft(tSender, tLeft + e.HorizontalChange);
                }
            }

            int tTop = (int)Canvas.GetTop(tSender);
            int tBot = tTop + (int)tSender.Height;
            if(tTop + e.VerticalChange > -1) {
                if(tBot + e.VerticalChange < cvRectangles.Height) {
                    Canvas.SetTop(tSender, tTop + e.VerticalChange);
                }
            }
        }

        private void Visualize_lolClientImage() {
            imgClientImage.Width = _lolClientImage.Width;
            imgClientImage.Height = _lolClientImage.Height;
            imgClientImage.Source = _lolClientImage;
            cvRectangles.Width = _lolClientImage.Width;
            cvRectangles.Height = _lolClientImage.Height;

            expOptions.Visibility = Visibility.Visible;

            this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }

        private void CreateNewThumb() {
            double widthMiddle = _ClientBitmap.Width / 2;
            double heightMiddle = _ClientBitmap.Height / 2;

            Canvas.SetLeft(ccPos, widthMiddle);
            Canvas.SetTop(ccPos, heightMiddle);
            ccPos.Width = _cBaseThumbWidth;


            ccPos.Height = _cBaseThumbHeight;
        }

        private void TransformRectangle(ContentControl rectRepos, Rectangle newPosition) {
            rectRepos.Width = newPosition.Width;
            rectRepos.Height = newPosition.Height;
            Canvas.SetLeft(rectRepos, newPosition.X);
            Canvas.SetTop(rectRepos, newPosition.Y);
        }

        private void ccPos_LayoutUpdated(object sender, EventArgs e) {
            if(recursiveFlag) {
                return;
            }
            recursiveFlag = true; //Ensures we don't go into a recursive loop as this event will be called on every layout update

            if(_ClientBitmap != null) {
                if(ccPos.Width > cvRectangles.Width) {
                    ccPos.Width = _ClientBitmap.Width;
                }
                if(ccPos.Height > cvRectangles.Height) {
                    ccPos.Height = _ClientBitmap.Height;
                }

                if(sender != null) {
                    ContentControl tSender = (ContentControl)sender;

                    int MostRightPixel = (int)Canvas.GetRight(tSender);
                    if(MostRightPixel > _ClientBitmap.Width) {
                        Canvas.SetLeft(tSender, _ClientBitmap.Width - (_ClientBitmap.Width - MostRightPixel));

                    }

                    int MostBottomPixel = (int)Canvas.GetBottom(tSender);
                    if(MostBottomPixel > _ClientBitmap.Height) {
                        Canvas.SetTop(tSender, _ClientBitmap.Height - (_ClientBitmap.Height - MostBottomPixel));
                    }
                }
            }

            recursiveFlag = false; //Don't forget to disable it
        }

    }
}