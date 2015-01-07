using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EasyChampionSelection.Themes.Controls.MyThumb {
    public class MoveThumb : System.Windows.Controls.Primitives.Thumb {
        public MoveThumb() {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e) {
            Control designerItem = this.DataContext as Control;

            if(designerItem != null) {
                double left = Canvas.GetLeft(designerItem);
                double top = Canvas.GetTop(designerItem);

                if(left + e.HorizontalChange > 0) {
                    Canvas.SetLeft(designerItem, left + e.HorizontalChange);
                } else {
                    Canvas.SetLeft(designerItem, 0);
                }

                if(top + e.VerticalChange > 0) {
                    Canvas.SetTop(designerItem, top + e.VerticalChange);
                } else {
                    Canvas.SetTop(designerItem, 0);
                }


            }
        }
    }
}
