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
        private const int _cBaseThumbWidth = 50;
        private const int _cBaseThumbHeight = 50;

        public wndConfigLolClientOverlay(StaticLolClientGraphics lcg) {
            if(lcg != null) {
                _lcg = lcg;
            } else {
                throw new ArgumentNullException();
            }

            InitializeComponent();
            txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
        }

        private void btnGetCurrentClientImage_Click(object sender, RoutedEventArgs e) {
            BitmapSource lolClientImage = _lcg.SaveClientImage();
            imgClientImage.Width = lolClientImage.Width;
            imgClientImage.Height = lolClientImage.Height;
            imgClientImage.Source = lolClientImage;
            cvRectangles.Width = lolClientImage.Width;
            cvRectangles.Height = lolClientImage.Height;
            this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }

        private void rdbChampionSearchbar_Checked(object sender, RoutedEventArgs e) {
            //Old position rectangle
            Rectangle oldCSBP = _lcg.getChampionSearchBarPosition();
            if(chkShowOldPosition.IsChecked == true) {
                if(oldCSBP != null) {
                    transformRectangle(thmbOldPos, oldCSBP);
                    txtNewThumbWidth.Text = (oldCSBP.Width - oldCSBP.X).ToString();
                    txtNewThumbHeight.Text = (oldCSBP.Height - oldCSBP.Y).ToString();
                } else {
                    thmbOldPos.Visibility = System.Windows.Visibility.Hidden;
                    txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
                    txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
                }
            }

            //New position rectangle
            if(oldCSBP != null && oldCSBP.Width > 0 && oldCSBP.Height > 0) {
                transformRectangle(thmbNewPos, oldCSBP);
            } else {
                createNewThumb();
            }

            imgClientImage.Visibility = System.Windows.Visibility.Hidden;
        }

        private void rdbTeamChat_Checked(object sender, RoutedEventArgs e) {
            //Old position rectangle
            Rectangle oldTTP = _lcg.getTeamChatPosition();
            if(chkShowOldPosition.IsChecked == true) {
                if(oldTTP != null) {
                    transformRectangle(thmbOldPos, oldTTP);
                    txtNewThumbWidth.Text = (oldTTP.Width - oldTTP.X).ToString();
                    txtNewThumbHeight.Text = (oldTTP.Height - oldTTP.Y).ToString();
                } else {
                    thmbOldPos.Visibility = System.Windows.Visibility.Hidden;
                    txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
                    txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
                }
            }

            //New position rectangle
            if(oldTTP != null) {
                transformRectangle(thmbNewPos, oldTTP);
            } else {
                createNewThumb();
            }
        }

        private void rdbClientOverlay_Checked(object sender, RoutedEventArgs e) {
            Rectangle oldCOP = _lcg.getClientOverlayPosition();
            //Old position rectangle
            if(chkShowOldPosition.IsChecked == true) {
                if(oldCOP != null) {
                    transformRectangle(thmbOldPos, oldCOP);
                    txtNewThumbWidth.Text = (oldCOP.Width - oldCOP.X).ToString();
                    txtNewThumbHeight.Text = (oldCOP.Height - oldCOP.Y).ToString();
                } else {
                    thmbOldPos.Visibility = System.Windows.Visibility.Hidden;
                    txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
                    txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
                }
            }

            //New position rectangle
            if(oldCOP != null) {
                transformRectangle(thmbNewPos, oldCOP);
            } else {
                createNewThumb();
            }
        }

        private void transformRectangle(Thumb rectRepos, Rectangle newPosition) {
            rectRepos.Width = newPosition.Width - newPosition.X;
            rectRepos.Height = newPosition.Bottom - newPosition.Y;
            Canvas.SetLeft(rectRepos, newPosition.X);
            Canvas.SetTop(rectRepos, newPosition.Y);
            rectRepos.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle
                = new Rectangle((int)thmbNewPos.Margin.Left, (int)thmbNewPos.Margin.Top, (int)thmbNewPos.Width, (int)thmbNewPos.Height);
            
            if(rdbChampionSearchbar.IsChecked == true) {
                _lcg.setChampionSearchBarPosition(repositionRectangle);
            } else if(rdbTeamChat.IsChecked == true) {
                _lcg.setTeamChatPosition(repositionRectangle);
            } else if(rdbClientOverlay.IsChecked == true) {
                _lcg.setClientOverlayPosition(repositionRectangle);
            }
        }

        private void thmbRectangle_OnDragDelta(object sender, DragDeltaEventArgs e) {
                Canvas.SetLeft((Thumb)sender, Canvas.GetLeft((Thumb)sender) + e.HorizontalChange);
                Canvas.SetTop((Thumb)sender, Canvas.GetTop((Thumb)sender) + e.VerticalChange);    
        }

        private void createNewThumb() {
            Canvas.SetLeft(thmbNewPos, 0d);
            Canvas.SetTop(thmbNewPos, 0d);

            int tWidth;
            if(int.TryParse(txtNewThumbWidth.Text, out tWidth)) {
                thmbNewPos.Width = tWidth;
            } else {
                thmbNewPos.Width = _cBaseThumbWidth;
                txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
            }

            int tHeight;
            if(int.TryParse(txtNewThumbHeight.Text, out tHeight)) {
                thmbNewPos.Height = tHeight;
            } else {
                thmbNewPos.Height = _cBaseThumbHeight;
                txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            }
        }
    }
}