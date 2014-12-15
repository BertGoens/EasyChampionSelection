using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace EasyChampionSelection.ECS {
    /// <summary>
    /// A Singleton class
    /// Static graphical helper for the lolClient.exe process
    /// </summary>
    public sealed class StaticLolClientGraphics {

        #region Events
        public delegate void StaticLolClientGraphicsHandler(StaticLolClientGraphics sender, EventArgs e);

        /// <summary>
        /// Occurs when the league of legends client is repositioned.
        /// </summary>
        [field: NonSerialized]
        public event StaticLolClientGraphicsHandler OnLeagueClientReposition;

        /// <summary>
        /// Occurs when the league of legends client is reseized
        /// </summary>
        [field: NonSerialized]
        public event StaticLolClientGraphicsHandler OnLeagueClientResized;
        #endregion Events

        #region Properties & Attributes
        private Rectangle _rectLolBounds;
        private static Settings _ecsSettings;
        private static Process processLolClient;
        private static StaticLolClientGraphics _instance;
        #endregion Properties & Attributes

        #region Private P/Invoke DLL Import & Related structures
        // Fire a mouse event
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        /// <summary>
        /// Win 32 Api wich gives a intPtr of the foreground window
        /// </summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Win 32 Api wich returns a given window' Process
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);


        //Source: http://www.pinvoke.net/default.aspx/user32/getwindowrect.html
        /// <summary>
        /// Returns a rectangle of a window. (X, Y, Width, Height)
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rectangle lpRect);

        //Source: http://www.pinvoke.net/default.aspx/user32/GetWindowDC.html
        /// <summary>
        /// The GetWindowDC function retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars. 
        /// A window device context permits painting anywhere in a window, 
        /// because the origin of the device context is the upper-left corner of the window instead of the client area. 
        /// GetWindowDC assigns default attributes to the window device context each time it retrieves the device context. Previous attributes are lost.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        //Source: http://www.pinvoke.net/default.aspx/gdi32/StretchBlt.html
        /// <summary>
        /// Win32 Api Function that's not documented at all but i'll try to understand it to my best.
        /// </summary>
        /// <param name="hdcDest">Target image</param>
        /// <param name="nXOriginDest">Target image X</param>
        /// <param name="nYOriginDest">Target image Y</param>
        /// <param name="nWidthDest">Target width</param>
        /// <param name="nHeightDest">Target height</param>
        /// <param name="hdcSrc">Source image</param>
        /// <param name="nXOriginSrc">Source image X</param>
        /// <param name="nYOriginSrc">Source image Y</param>
        /// <param name="nWidthSrc">Source image width</param>
        /// <param name="nHeightSrc">Source image height</param>
        /// <param name="dwRop">see <c>TernaryRasterOperations</c></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        private static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            TernaryRasterOperations dwRop);

        private enum TernaryRasterOperations {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,

            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,

            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,

            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,

            /// <summary>dest = source AND (NOT dest )</summary>
            SRCERASE = 0x00440328,

            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,

            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,

            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,

            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,

            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,

            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,

            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,

            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,

            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,

            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
        };

        /// <summary>
        /// Bitmap to BitmapSource helper
        /// </summary>
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        #endregion Private DLL Import & Related structures

        #region Getters & Setters
        public Settings getSettings() {
            return _ecsSettings;
        }

        /// <summary>
        /// Returns the associated League Client process
        /// </summary>
        public Process getProcessLolClient() {
            return processLolClient;
        }

        /// <summary>
        /// Returns the absolute position of where the user wants
        /// </summary>
        /// <returns></returns>
        public Rectangle getClientOverlayPosition() {
            Rectangle pos = new Rectangle();
            pos.Width = _ecsSettings.ClientOverlayRelativePos.Width;
            pos.Height = _ecsSettings.ClientOverlayRelativePos.Height;
            pos.X = _ecsSettings.ClientOverlayRelativePos.X + _rectLolBounds.X;
            pos.Y = _ecsSettings.ClientOverlayRelativePos.Y + _rectLolBounds.Y;
            return pos;
        }
        #endregion Getters & Setters

        #region Constructor
        /// <summary>
        /// Private constructor (singleton)
        /// Please use GetInstance() to use the class.
        /// </summary>
        private StaticLolClientGraphics(Process pLolClient, Settings ecsSettings) {
            _ecsSettings = ecsSettings;
            processLolClient = pLolClient;
        }
        #endregion Constructor

        #region Singleton Setup

        /// <summary>
        /// Create a StaticLolClientGraphics object.
        /// </summary>
        public static StaticLolClientGraphics GetInstance(Process lolClient, Settings ecsSettings) {
            if(lolClient != null && ecsSettings != null) {
                if(_instance == null || lolClient != processLolClient) {
                    _instance = new StaticLolClientGraphics(lolClient, ecsSettings);
                }
            } else {
                throw new ArgumentNullException();
            }

            return _instance;
        }

        #endregion Singleton Setup

        /// <summary>
        /// Used to type a string of champion-names in the search bar to create a filter.
        /// </summary>
        public void TypeInSearchBar(String textToEnter) {
            ClickInSearchBar(textToEnter);
        }

        private void ClickInSearchBar(String textToEnter) {
            //Calculate ChampionSearchBar position
            int ChampionSearchBarXpos = _ecsSettings.ChampionSearchbarRelativePos.X + ((_ecsSettings.ChampionSearchbarRelativePos.Width / 2));
            int ChampionSearchBarYpos = _ecsSettings.ChampionSearchbarRelativePos.Y + ((_ecsSettings.ChampionSearchbarRelativePos.Height / 2));

            //?
            // get screen coordinates
            Point pntSearchBarAbsolute = new Point(ChampionSearchBarXpos, ChampionSearchBarYpos);
            ClientToScreen(processLolClient.MainWindowHandle, ref pntSearchBarAbsolute);

            // set cursor on coords, and press mouse
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(pntSearchBarAbsolute.X, pntSearchBarAbsolute.Y);
            mouse_event(0x00000002, 0, 0, 0, UIntPtr.Zero); /// left mouse button down
            mouse_event(0x00000004, 0, 0, 0, UIntPtr.Zero); /// left mouse button up

            // type
            TypeTextInSearchBar(textToEnter);
        }

        private void TypeTextInSearchBar(String text) {
            //Clear old text (select all - delete)
            System.Windows.Forms.SendKeys.SendWait("^A");
            System.Windows.Forms.SendKeys.SendWait("{BACKSPACE}");
            //Enter new text
            System.Windows.Forms.SendKeys.SendWait(text);
            System.Windows.Forms.SendKeys.Flush();
        }

        /// <summary>
        /// Determines if a given process is focussed (UI focussed)
        /// </summary>
        public bool isProcessFocussed(Process p) {
            IntPtr foreGroundWindow = GetForegroundWindow();
            int foreGroundWindowProcessId;
            GetWindowThreadProcessId(foreGroundWindow, out foreGroundWindowProcessId);
            bool isTrue = (p.Id == foreGroundWindowProcessId);
            return isTrue;
        }

        /// <summary>
        /// Determines if the associated League Client is focussed by the user (UI focussed)
        /// </summary>
        public bool isLolClientFocussed() {
            IntPtr foreGroundWindow = GetForegroundWindow();
            int foreGroundWindowProcessId;
            GetWindowThreadProcessId(foreGroundWindow, out foreGroundWindowProcessId);
            bool isTrue = (processLolClient.Id == foreGroundWindowProcessId);
            return isTrue;
        }

        /// <summary>
        /// Determines if this program is focussed by the user (UI focussed)
        /// </summary>
        public bool isEasyChampionSelectionFoccussed() {
            IntPtr foreGroundWindow = GetForegroundWindow();
            int foreGroundWindowProcessId;
            GetWindowThreadProcessId(foreGroundWindow, out foreGroundWindowProcessId);
            bool isTrue = Process.GetCurrentProcess().Id == foreGroundWindowProcessId;
            return isTrue;
        }

        private Bitmap GetLeagueClientAsBitmap() {
            // Old lol bounds
            Rectangle origLolBounds = new Rectangle(_rectLolBounds.X, _rectLolBounds.Y, _rectLolBounds.Width, _rectLolBounds.Height);

            // New lol bounds
            GetWindowRect(processLolClient.MainWindowHandle, out _rectLolBounds);

            if(_rectLolBounds.Width < 1 || _rectLolBounds.Height < 1) {
                return null;
            }

            // Check if client has repositioned
            if(_rectLolBounds.X != origLolBounds.X || _rectLolBounds.Y != origLolBounds.Y) {
                if(OnLeagueClientReposition != null) {
                    OnLeagueClientReposition(this, new EventArgs());
                }
            }

            // Check if client has been resized
            if(_rectLolBounds.Width != origLolBounds.Width || _rectLolBounds.Height != origLolBounds.Height) {
                if(OnLeagueClientResized != null) {
                    OnLeagueClientResized(this, new EventArgs());
                }
            }

            // Create bitmap of correct size
            Bitmap lolClientSized_Bitmap = new Bitmap(_rectLolBounds.Width - _rectLolBounds.X, _rectLolBounds.Height - _rectLolBounds.Y, PixelFormat.Format24bppRgb);
            Graphics lolClientSized_Graphics = Graphics.FromImage(lolClientSized_Bitmap);

            // Get source handle device context
            IntPtr lolClient_WindowDeviceContext = GetWindowDC(processLolClient.MainWindowHandle);

            try {
                // Get target handle device context
                IntPtr lolClient_MemoryHandleDeviceContext = lolClientSized_Graphics.GetHdc();

                // Copy source into target
                StretchBlt(lolClient_MemoryHandleDeviceContext, 0, 0, lolClientSized_Bitmap.Width, lolClientSized_Bitmap.Height,
                    lolClient_WindowDeviceContext, 0, 0, _rectLolBounds.Width - _rectLolBounds.X, _rectLolBounds.Height - _rectLolBounds.Y,
                    TernaryRasterOperations.SRCCOPY);

                // Release handle
                lolClientSized_Graphics.ReleaseHdc();
                lolClientSized_Graphics.Dispose();
            } catch(ArgumentException) { }

            return lolClientSized_Bitmap;

        }

        /// <summary>
        /// Determines if you are in Champion Select 
        /// (Does so on your used provided <c>Settings</c> parameters: ChampionSearchbar, TeamChat)
        /// </summary>
        public bool isInChampSelect() {
            if(_rectLolBounds.Width < 400) {
                GetWindowRect(processLolClient.MainWindowHandle, out _rectLolBounds);
                if(_rectLolBounds.Width < 400) {
                    return false;
                }
            }

            Bitmap picOfClient = GetLeagueClientAsBitmap();

            if(picOfClient == null) {
                return false;
            }

            Color white = Color.FromArgb(255, 255, 255);

            //ChampionSearchBar Check
            int cBarX = _ecsSettings.ChampionSearchbarRelativePos.X;
            int cBarY = _ecsSettings.ChampionSearchbarRelativePos.Y;
            int cBarWidth = _ecsSettings.ChampionSearchbarRelativePos.Width;
            int cBarHeight = _ecsSettings.ChampionSearchbarRelativePos.Height;
            int championBarWhitePixels = 1;
            int championBarAllPixels = cBarWidth * cBarHeight;

            for(int y = 0; y < cBarHeight + 1; y++) {
                for(int x = 0; x < cBarWidth; x++) {
                    Color pixel = picOfClient.GetPixel(cBarX + x, +cBarY + y);
                    if(pixel.Equals(white)) {
                        championBarWhitePixels++;
                    }
                }
                if(championBarWhitePixels > championBarAllPixels / 2) {
                    break;
                }
            }
            if(championBarWhitePixels < championBarAllPixels / 2) {
                return false;
            }

            //TeamChatBarCheck
            int tChatbarX = _ecsSettings.TeamChatRelativePos.X;
            int tChatbarY = _ecsSettings.TeamChatRelativePos.Y;
            int tChatbarWidth = _ecsSettings.TeamChatRelativePos.Width;
            int tChatbarHeight = _ecsSettings.TeamChatRelativePos.Height;
            int tChatbarWhitePixels = 1;
            int tChatbarAllPixels = tChatbarWidth * tChatbarHeight;

            for(int y = 0; y < tChatbarHeight + 1; y++) {
                for(int x = 0; x < tChatbarWidth; x++) {
                    Color pixel = picOfClient.GetPixel(tChatbarX + x, tChatbarY + y);
                    if(pixel.Equals(white)) {
                        tChatbarWhitePixels++;
                    }
                }

                if(tChatbarWhitePixels > tChatbarAllPixels / 2) {
                    break;
                }
            }
            if(tChatbarWhitePixels < tChatbarAllPixels / 2) {
                return false;
            }

            picOfClient.Dispose();

            return true;
        }

        /// <summary>
        /// Returns an RGB BitmapSource of the Client at this moment
        /// </summary>
        public System.Windows.Media.Imaging.BitmapSource GetLeagueClientAsBitmapSource() {
            //Use the IDisposable so we automatically dispose our image (can soak a lot of memory)
            using(Bitmap lolClientSized_Bitmap = GetLeagueClientAsBitmap()) {

                if(lolClientSized_Bitmap == null) {
                    return null;
                }

                IntPtr ip;
                try {
                    ip = lolClientSized_Bitmap.GetHbitmap();
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
}
