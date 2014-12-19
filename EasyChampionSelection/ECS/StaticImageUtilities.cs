using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EasyChampionSelection.ECS {
    public static class StaticImageUtilities {

        /// <summary>
        /// Bitmap to BitmapSource helper
        /// </summary>
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Returns an RGB BitmapSource of the given Bitmap
        /// </summary>
        public static System.Windows.Media.Imaging.BitmapSource BitmapToBitmapSource(Bitmap bmi) {
            if(bmi == null) {
                return null;
            }

            IntPtr ip;
            try {
                ip = bmi.GetHbitmap();
            } catch(ArgumentException) {
                return null;
            }

            System.Windows.Media.Imaging.BitmapSource bs = null;

            try {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, System.Windows.Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            } finally {
                DeleteObject(ip);
            }

            return bs;
        }

    }
}
