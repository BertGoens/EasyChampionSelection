using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EasyChampionSelection.Themes.Controls.MyThumb {
    public class ResizeThumb : System.Windows.Controls.Primitives.Thumb {
        private const int minWidth = 10;
        private const int minHeight = 10;

        public ResizeThumb() {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e) {
            Control designerItem = this.DataContext as Control;

            if(designerItem != null) {
                double deltaVertical, deltaHorizontal;

                switch(VerticalAlignment) {
                    case VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                        if(designerItem.Height - deltaVertical > minHeight - 1) {
                            designerItem.Height -= deltaVertical;
                        } else {
                            designerItem.Height = minHeight;
                        }
                        break;
                    case VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                        if(designerItem.Height - deltaVertical > minHeight - 1) {
                            Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + deltaVertical);
                            designerItem.Height -= deltaVertical;
                        } else {
                            designerItem.Height = minHeight;
                        }
                        break;
                    default:
                        break;
                }

                switch(HorizontalAlignment) {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                        if(designerItem.Width - deltaHorizontal > minWidth - 1) {
                            Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + deltaHorizontal);
                            designerItem.Width -= deltaHorizontal;
                        } else {
                            designerItem.Width = minWidth;
                        }
                        break;
                    case HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                        if(designerItem.Width - deltaHorizontal > minWidth - 1) {
                            designerItem.Width -= deltaHorizontal;
                        } else {
                            designerItem.Width = minWidth;
                        }
                        break;
                    default:
                        break;
                }
            }

            e.Handled = true;
        }
    }
}
