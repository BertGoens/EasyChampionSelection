using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.AppRuntimeResources;
using EasyChampionSelection.ECS.AppRuntimeResources.LolClient;
using System;
using System.Drawing;
using System.IO;
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
        //Recursive flags (can't make them optional parameters cause it apparantly doens't work with event delegates)
        private bool recursiveFlag_ccPos_LayoutUpdated = false;
        private bool recursiveFlag_btnGetCurrentClientImage_Click = false;
        private Action<string> _displayPopup;

        private wndConfigLolClientOverlay() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }

        public wndConfigLolClientOverlay(StaticPinvokeLolClient lcg, EcsSettings ecsSettings, Action<string> DisplayPopup) : this() {
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
                if(_lcg.ClientState != LolClientState.NoClient) {
                    btnGetCurrentClientImage_Click(null, null);
                }
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
                } catch(Exception ex) {
                    _displayPopup(ex.ToString());
                }
            }
        }

        private void btnGetCurrentClientImage_Click(object sender, RoutedEventArgs e) {
            _ClientBitmap = _lcg.GetLeagueClientAsBitmap(); // Get bitmap of league client

            if(_ClientBitmap == null) {
                if(!recursiveFlag_btnGetCurrentClientImage_Click) {
                    _lcg.BringClientToFront();
                    recursiveFlag_btnGetCurrentClientImage_Click = true;
                    btnGetCurrentClientImage_Click(this, e);
                } else {
                    recursiveFlag_btnGetCurrentClientImage_Click = false;
                    _displayPopup("Can't find active client - clientState: " + _lcg.ClientState.ToString());
                }
            }
            
            _lolClientImage = StaticImageUtilities.BitmapToBitmapSource(_ClientBitmap); //Convert bitmap to bitmapsource

            if(_lolClientImage != null) {
                Visualize_lolClientImage();
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
            if(recursiveFlag_ccPos_LayoutUpdated) {
                return;
            }
            recursiveFlag_ccPos_LayoutUpdated = true; //Ensures we don't go into a recursive loop as this event will be called on every layout update

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

            recursiveFlag_ccPos_LayoutUpdated = false; //Don't forget to disable it
        }

    }
}