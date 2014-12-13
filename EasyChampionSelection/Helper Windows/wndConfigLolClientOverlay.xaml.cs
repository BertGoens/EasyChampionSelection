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

        private void rdbOnChecked(object sender, RoutedEventArgs e) {
            //Old position rectangle
            RadioButton rdbSender = (RadioButton)sender;
            Rectangle oldPosition = new Rectangle();
            if(rdbSender.Equals(rdbChampionSearchbar)) {
                oldPosition = _ecsSettings.ChampionSearchbarRelativePos;
            } else if(rdbSender.Equals(rdbTeamChat)) {
                oldPosition = _ecsSettings.TeamChatRelativePos;
            } else if(rdbSender.Equals(rdbClientOverlay)) {
                oldPosition = _ecsSettings.ClientOverlayRelativePos;
            }

            if(oldPosition.Width > 0 && oldPosition.Height > 0) {
                txtNewThumbWidth.Text = (oldPosition.Width - oldPosition.X).ToString();
                txtNewThumbHeight.Text = (oldPosition.Height - oldPosition.Y).ToString();
                transformRectangle(thmbPos, oldPosition);
            } else {
                txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
                txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
                createNewThumb();
            }
            
            thmbPos.Visibility = System.Windows.Visibility.Visible;
        }

        private void rdbChampionSearchbar_Unchecked(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(thmbPos), (int)Canvas.GetTop(thmbPos), (int)thmbPos.Width, (int)thmbPos.Height);
            _ecsSettings.ChampionSearchbarRelativePos = repositionRectangle;
        }

        private void rdbTeamChat_Unchecked(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(thmbPos), (int)Canvas.GetTop(thmbPos), (int)thmbPos.Width, (int)thmbPos.Height);
            _ecsSettings.TeamChatRelativePos = repositionRectangle;
        }

        private void rdbClientOverlay_Unchecked(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(thmbPos), (int)Canvas.GetTop(thmbPos), (int)thmbPos.Width, (int)thmbPos.Height);
            _ecsSettings.ClientOverlayRelativePos = repositionRectangle;
        }

        private void txtNewThumbWidth_TextChanged(object sender, TextChangedEventArgs e) {
            int newWidth = 0;
            if(int.TryParse(txtNewThumbWidth.Text, out newWidth)) {
                newWidth =  Math.Abs(newWidth);
                thmbPos.Width = newWidth;
            } else {
                thmbPos.Width = _cBaseThumbWidth;
            }
        }

        private void txtNewThumbHeight_TextChanged(object sender, TextChangedEventArgs e) {
            int newHeight = 0;
            if(int.TryParse(txtNewThumbHeight.Text, out newHeight)) {
                thmbPos.Height = Math.Abs(newHeight);
            } else {
                thmbPos.Height = _cBaseThumbHeight;
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
            Canvas.SetLeft(thmbPos, 0d);
            Canvas.SetTop(thmbPos, 0d);

            int tWidth;
            if(int.TryParse(txtNewThumbWidth.Text, out tWidth)) {
                thmbPos.Width = Math.Abs(tWidth);
            } else {
                thmbPos.Width = _cBaseThumbWidth;
                txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
            }

            int tHeight;
            if(int.TryParse(txtNewThumbHeight.Text, out tHeight)) {
                thmbPos.Height = Math.Abs(tHeight);
            } else {
                thmbPos.Height = _cBaseThumbHeight;
                txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            }
        }

        private void transformRectangle(Thumb rectRepos, Rectangle newPosition) {
            rectRepos.Width = newPosition.Width;
            rectRepos.Height = newPosition.Height;
            Canvas.SetLeft(rectRepos, newPosition.X);
            Canvas.SetTop(rectRepos, newPosition.Y);
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e) {
            this.Visibility = System.Windows.Visibility.Hidden;
            this.ShowInTaskbar = false;
            e.Cancel = true;
        }

    }
}