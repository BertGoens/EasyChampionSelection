using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Forms;

namespace EasyChampionSelection.ECS  {
    /// <summary>
    /// A Singleton class
    /// Static graphical helper for the lolClient.exe process
    /// </summary>
    public sealed class StaticLolClientGraphics {

        #region Properties & Attributes
        /// <summary>
        /// Your absolute position of your league client on your desktop.
        /// Mines X160 Y30 W1140 H830
        /// </summary>
        private Rectangle rectLolBounds;

        /// <summary>
        /// Your league client search bar coords inside the client (not absolute)
        /// Mines X860, Y120, W140, H40
        /// </summary>
        private Rectangle rectSearchBar = new Rectangle(860, 120, 140, 40);

        /// <remarks>
        /// Even after taking a screenshot reqesting this size of the client. It will automatically adjust it to 711,578. 
        /// It fills the rest with whitespace.
        /// Anyways, your bitmap looks distorted as it isn't your usual high-res client
        /// </remarks>
        private Point pntClientSize = new Point(800, 600);
        /// <remarks>White search textbox coördinates</remarks>
        private Point pntBitmapSearchLeftEdge = new Point(480, 90);
        private Point pntBitmapSearchRightEdge = new Point(550,90);
        /// <remarks>White Team type text searchbox coördinates</remarks>
        private Point pntBitmapTeamTypeLeftEdge = new Point(210, 530); // 160 didnt't work
        private Point pntBitmapTeamTypeRightEdge = new Point(490, 530);

        private static Process processLolClient;
        private static StaticLolClientGraphics _instance;

        #endregion Properties & Attributes

        #region Private DLL Import & Related structures
        // Fire a mouse event
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        //Required for taking screenshots & stuff
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        //Source: http://www.pinvoke.net/default.aspx/user32/getwindowrect.html
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rectangle lpRect);

        //Source: http://www.pinvoke.net/default.aspx/user32/GetWindowDC.html
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        //Source: http://www.pinvoke.net/default.aspx/gdi32/StretchBlt.html
        [DllImport("gdi32.dll")]
        private static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            TernaryRasterOperations dwRop);

        private enum TernaryRasterOperations {
            SRCCOPY = 0x00CC0020, /* dest = source*/
            SRCPAINT = 0x00EE0086, /* dest = source OR dest*/
            SRCAND = 0x008800C6, /* dest = source AND dest*/
            SRCINVERT = 0x00660046, /* dest = source XOR dest*/
            SRCERASE = 0x00440328, /* dest = source AND (NOT dest )*/
            NOTSRCCOPY = 0x00330008, /* dest = (NOT source)*/
            NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
            MERGECOPY = 0x00C000CA, /* dest = (source AND pattern)*/
            MERGEPAINT = 0x00BB0226, /* dest = (NOT source) OR dest*/
            PATCOPY = 0x00F00021, /* dest = pattern*/
            PATPAINT = 0x00FB0A09, /* dest = DPSnoo*/
            PATINVERT = 0x005A0049, /* dest = pattern XOR dest*/
            DSTINVERT = 0x00550009, /* dest = (NOT dest)*/
            BLACKNESS = 0x00000042, /* dest = BLACK*/
            WHITENESS = 0x00FF0062, /* dest = WHITE*/
        };

        #endregion Private DLL Import & Related structures

        #region Constructor
        private StaticLolClientGraphics(Process pLolClient) { 
            processLolClient = pLolClient;
        }
        #endregion Constructor

        #region Getters & Setters
        public Process GetProcessLolClient() {
            return processLolClient;
        }
        #endregion Getters & Setters

        #region Singleton Setup

        public static StaticLolClientGraphics GetInstance(Process lolClient) {
            if(_instance == null || lolClient != processLolClient ) {
                _instance = new StaticLolClientGraphics(lolClient);
            }
            return _instance;
        }

        #endregion Singleton Setup

        public void TypeInSearchBar(String textToEnter) {
            ClickInSearchBar(textToEnter);
        }

        private void ClickInSearchBar(String textToEnter) {
            Point oldPos = System.Windows.Forms.Cursor.Position;

            // get screen coordinates
            Point pntSearchBarAbsolute = new Point(rectLolBounds.Width - (rectLolBounds.Width - rectSearchBar.X ), rectLolBounds.Y + rectSearchBar.Y - 5);
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

        public bool isProcessFocussed(Process p) {
            IntPtr foreGroundWindow = GetForegroundWindow();
            int foreGroundWindowProcessId;
            GetWindowThreadProcessId(foreGroundWindow, out foreGroundWindowProcessId);
            bool isTrue = (p.Id == foreGroundWindowProcessId);
            return isTrue;
        }

        public bool isLolClientFocussed() {
            IntPtr foreGroundWindow = GetForegroundWindow();
            int foreGroundWindowProcessId;
            GetWindowThreadProcessId(foreGroundWindow, out foreGroundWindowProcessId);
            bool isTrue = (processLolClient.Id == foreGroundWindowProcessId);
            return isTrue;
        }

        public bool isEasyChampionSelectionFoccussed() {
            IntPtr foreGroundWindow = GetForegroundWindow();
            int foreGroundWindowProcessId;
            GetWindowThreadProcessId(foreGroundWindow, out foreGroundWindowProcessId);

            return Process.GetCurrentProcess().Id == foreGroundWindowProcessId;
        }

        private Bitmap GetLeagueClientAsBitmap() {
            Bitmap clientBitmap = new Bitmap(pntClientSize.X, pntClientSize.Y, PixelFormat.Format32bppArgb);
            Graphics clientGraphics = Graphics.FromImage(clientBitmap);

            // get the size of the client window 
            GetWindowRect(processLolClient.MainWindowHandle, out rectLolBounds);

            // win32 stuff we need
            IntPtr ClientDeviceContext = GetWindowDC(processLolClient.MainWindowHandle);
            try {
                IntPtr MemoryHdc = clientGraphics.GetHdc();
                StretchBlt(MemoryHdc, 0, 0, clientBitmap.Width, clientBitmap.Height, ClientDeviceContext, 0, 0, rectLolBounds.Width, rectLolBounds.Height, TernaryRasterOperations.SRCCOPY);
                clientGraphics.ReleaseHdc(MemoryHdc);
            } catch(ArgumentException) {}

            //copy, this function takes the clientGraphics to draw the content of the complete Client DC (0,0,rect.Width,rect.Height)
            //and copies it into (0,0,clientBitmap.Width,clientBitmap.Height).
            //all my win32 stuff is in a seperate static class PInvoke, thats why that is there.
            //note that some colors could get mixed up if you resize your client.
            //clientBitmap.Save("clientBitmap.png");
            return clientBitmap;
        }

        public bool isInChampSelect() {
            Bitmap picOfClient = GetLeagueClientAsBitmap();

            Color white = Color.FromArgb(255, 255, 255);

            bool isWhite = true;

            if(!(picOfClient.GetPixel(pntBitmapSearchLeftEdge.X, pntBitmapSearchLeftEdge.Y).ToArgb().Equals(white.ToArgb()))) {
                isWhite = false;
            } else { // debugger 'hack'
                isWhite = true;
            }

            if(!(picOfClient.GetPixel(pntBitmapSearchRightEdge.X, pntBitmapSearchRightEdge.Y).ToArgb().Equals(white.ToArgb()))) {
                isWhite = false;
            }
            if(!(picOfClient.GetPixel(pntBitmapTeamTypeLeftEdge.X, pntBitmapTeamTypeLeftEdge.Y).ToArgb().Equals(white.ToArgb()))) {
                isWhite = false;
            }
            if(!(picOfClient.GetPixel(pntBitmapTeamTypeRightEdge.X, pntBitmapTeamTypeRightEdge.Y).ToArgb().Equals(white.ToArgb()))) {
                isWhite = false;
            }

            return isWhite;
        }

        public Rectangle GetClientOverlayPosition() {
            //Start value (relative position within window) /Resources/ClientOverloadPositions.jpg for an explanation
            // Riot hardcoded old above skins : Rectangle result = new Rectangle(350, 60, 50, 585);  
            // Riot hardcoded position next to the skins tab
            Rectangle result = new Rectangle(513, 114, 50, 585);  
            //Now we need to add the absolute X and Y of client to it
            result.X += rectLolBounds.X;
            result.Y += rectLolBounds.Y;
            return result;
        }
    }
}
