using EasyChampionSelection.ECS;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndConfigLolClientOverlay.xaml
    /// </summary>
    public partial class wndConfigLolClientOverlay : Window {
        private StaticLolClientGraphics _lcg;
        private Settings _ecsSettings;
        private const int _cBaseThumbWidth = 50;
        private const int _cBaseThumbHeight = 50;

        public wndConfigLolClientOverlay(StaticLolClientGraphics lcg, Settings ecsSettings) {
            if(lcg != null && ecsSettings != null) {
                _lcg = lcg;
                _ecsSettings = ecsSettings;
            } else {
                throw new ArgumentNullException();
            }

            InitializeComponent();
            txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
        }

        private void btnGetCurrentClientImage_Click(object sender, RoutedEventArgs e) {
            BitmapSource lolClientImage = _lcg.GetLeagueClientAsBitmapSource();
            imgClientImage.Width = lolClientImage.Width;
            imgClientImage.Height = lolClientImage.Height;
            imgClientImage.Source = lolClientImage;
            cvRectangles.Width = lolClientImage.Width;
            cvRectangles.Height = lolClientImage.Height;
            this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }

        private void chkShowOldPosition_Checked(object sender, RoutedEventArgs e) {
            if(imgClientImage.Width > 100) {
                thmbOldPos.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void chkShowOldPosition_Unchecked(object sender, RoutedEventArgs e) {
            thmbOldPos.Visibility = System.Windows.Visibility.Hidden;
        }

        private void rdbOnChecked(object sender, RoutedEventArgs e) {
            //Old position rectangle
            RadioButton rdbSender = (RadioButton)sender;
            Rectangle oldPosition = new Rectangle();
            if(rdbSender.Equals(rdbChampionSearchbar)) {
                oldPosition = _ecsSettings.ChampionSearchbarRelativePos;
            } else if(rdbSender.Equals(rdbTeamChat)) {
                oldPosition = _ecsSettings.TeamChatRelativePos;
            } else if(rdbSender.Equals(rdbClientOverlay)) {
                oldPosition = _ecsSettings.TeamChatRelativePos;
            }

            transformRectangle(thmbOldPos, oldPosition);
            if(oldPosition.Width > 0 && oldPosition.Height > 0) {
                txtNewThumbWidth.Text = (oldPosition.Width - oldPosition.X).ToString();
                txtNewThumbHeight.Text = (oldPosition.Height - oldPosition.Y).ToString();
            } else {
                txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
                txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            }
            

            //New position rectangle
            if(oldPosition != null && oldPosition.Width > 0 && oldPosition.Height > 0) {
                transformRectangle(thmbNewPos, oldPosition);
            } else {
                createNewThumb();
            }
            thmbNewPos.Visibility = System.Windows.Visibility.Visible;
        }


        private void txtNewThumbWidth_TextChanged(object sender, TextChangedEventArgs e) {
            int newWidth = 0;
            if(int.TryParse(txtNewThumbWidth.Text, out newWidth)) {
                thmbNewPos.Width = Math.Abs(newWidth);
            } else {
                thmbNewPos.Width = _cBaseThumbWidth;
            }
        }

        private void txtNewThumbHeight_TextChanged(object sender, TextChangedEventArgs e) {
            int newHeight = 0;
            if(int.TryParse(txtNewThumbHeight.Text, out newHeight)) {
                thmbNewPos.Height = Math.Abs(newHeight);
            } else {
                thmbNewPos.Height = _cBaseThumbHeight;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle
                = new Rectangle((int)Canvas.GetLeft(thmbNewPos), (int)Canvas.GetTop(thmbNewPos), (int)thmbNewPos.Width, (int)thmbNewPos.Height);

            if(rdbChampionSearchbar.IsChecked == true) {
                _ecsSettings.ChampionSearchbarRelativePos = repositionRectangle;
            } else if(rdbTeamChat.IsChecked == true) {
                _ecsSettings.TeamChatRelativePos = repositionRectangle;
            } else if(rdbClientOverlay.IsChecked == true) {
                _ecsSettings.ClientOverlayRelativePos = repositionRectangle;
            }
        }

        private void thmbRectangle_OnDragDelta(object sender, DragDeltaEventArgs e) {
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

        private void createNewThumb() {
            Canvas.SetLeft(thmbNewPos, 0d);
            Canvas.SetTop(thmbNewPos, 0d);

            int tWidth;
            if(int.TryParse(txtNewThumbWidth.Text, out tWidth)) {
                thmbNewPos.Width = Math.Abs(tWidth);
            } else {
                thmbNewPos.Width = _cBaseThumbWidth;
                txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
            }

            int tHeight;
            if(int.TryParse(txtNewThumbHeight.Text, out tHeight)) {
                thmbNewPos.Height = Math.Abs(tHeight);
            } else {
                thmbNewPos.Height = _cBaseThumbHeight;
                txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            }
        }

        private void transformRectangle(Thumb rectRepos, Rectangle newPosition) {
            rectRepos.Width = newPosition.Width;
            rectRepos.Height = newPosition.Height;
            Canvas.SetLeft(rectRepos, newPosition.X);
            Canvas.SetTop(rectRepos, newPosition.Y);
        }
    }
}