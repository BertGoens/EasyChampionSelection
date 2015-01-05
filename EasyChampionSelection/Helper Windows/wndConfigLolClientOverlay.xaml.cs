﻿using EasyChampionSelection.ECS;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndConfigLolClientOverlay.xaml
    /// </summary>
    public partial class wndConfigLolClientOverlay : Window {
        private StaticPinvokeLolClient _lcg;
        private Settings _ecsSettings;
        private const int _cBaseThumbWidth = 50;
        private const int _cBaseThumbHeight = 50;
        private const int _cBaseImageWidth = 800;
        private const int _cBaseImageHeight = 600;
        private Bitmap _ClientBitmap;
        private BitmapSource _lolClientImage;

        public wndConfigLolClientOverlay(StaticPinvokeLolClient lcg, Settings ecsSettings) {
            if(ecsSettings != null) {
                _lcg = lcg;
                _ecsSettings = ecsSettings;
            }

            InitializeComponent();
            txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
            txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();

            if(File.Exists(StaticSerializer.FullPath_ClientImage)) {
                _ClientBitmap = new Bitmap(StaticSerializer.FullPath_ClientImage);
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
                    _ClientBitmap.Save(StaticSerializer.FullPath_ClientImage, ImageFormat.Jpeg);
                } catch(Exception ex) {
                    ex.ToString();
                }
            }
        }

        private void btnGetCurrentClientImage_Click(object sender, RoutedEventArgs e) {
            if(_lcg != null) {
                _ClientBitmap = _lcg.GetLeagueClientAsBitmap(); // Get bitmap of league client
                _lolClientImage =  StaticImageUtilities.BitmapToBitmapSource(_ClientBitmap); //Convert bitmap to bitmapsource

                if(_lolClientImage != null) {
                    Visualize_lolClientImage();
                }
            } 
        }

        private void rdbOnChecked(object sender, RoutedEventArgs e) {
            spThumbSizeInfo.Visibility = System.Windows.Visibility.Visible;

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
                txtNewThumbWidth.Text = Math.Abs(SavedPosition.Width).ToString();
                txtNewThumbHeight.Text = Math.Abs(SavedPosition.Height).ToString();
                TransformRectangle(thmbPos, SavedPosition);
            } else {
                txtNewThumbWidth.Text = _cBaseThumbWidth.ToString();
                txtNewThumbHeight.Text = _cBaseThumbHeight.ToString();
                CreateNewThumb();
            }

            thmbPos.Visibility = System.Windows.Visibility.Visible;
        }

        private void rdbChampionSearchbar_SavePosition(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(thmbPos), (int)Canvas.GetTop(thmbPos), (int)thmbPos.Width, (int)thmbPos.Height);
            _ecsSettings.ChampionSearchbarRelativePos = repositionRectangle;
        }

        private void rdbTeamChat_SavePosition(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(thmbPos), (int)Canvas.GetTop(thmbPos), (int)thmbPos.Width, (int)thmbPos.Height);
            _ecsSettings.TeamChatRelativePos = repositionRectangle;
        }

        private void rdbClientOverlay_SavePosition(object sender, RoutedEventArgs e) {
            Rectangle repositionRectangle = new Rectangle((int)Canvas.GetLeft(thmbPos), (int)Canvas.GetTop(thmbPos), (int)thmbPos.Width, (int)thmbPos.Height);
            _ecsSettings.ClientOverlayRelativePos = repositionRectangle;
        }

        private void txtNewThumbWidth_TextChanged(object sender, TextChangedEventArgs e) {
            int newWidth = 0;
            if(int.TryParse(txtNewThumbWidth.Text, out newWidth)) {
                newWidth = Math.Abs(newWidth);
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

            Canvas.SetLeft(thmbPos, widthMiddle);
            Canvas.SetTop(thmbPos, heightMiddle);

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

        private void TransformRectangle(Thumb rectRepos, Rectangle newPosition) {
            rectRepos.Width = newPosition.Width;
            rectRepos.Height = newPosition.Height;
            Canvas.SetLeft(rectRepos, newPosition.X);
            Canvas.SetTop(rectRepos, newPosition.Y);
        }

    }
}