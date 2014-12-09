using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Forms;
using System.Collections.Generic;

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
        public event StaticLolClientGraphicsHandler OnLeagueClientReposition;

        /// <summary>
        /// Occurs when the league of legends client is reseized
        /// </summary>
        public event StaticLolClientGraphicsHandler OnLeagueClientResized;
        #endregion Events

        #region Properties & Attributes

        /// <summary>
        /// The client size follows this linear function: 0 + 0,625x = Y
        /// This is only provided as a check too see if your values are right
        /// </summary>
        private const double lfClientSizeY = 0.6225;

        /// <summary>
        /// The SkinsTabEndPointY follows this linear function: 0 + 0,2515 * ClientSize.X = Y
        /// </summary>
        private const double lfSkinsTabEndY = 0.22;

        /// <summary>
        /// The SkinsTabEndPointX follows this linear function: 0 + 0,4 * ClientSize.X = X
        /// </summary>
        private const double lfSkinsTabEndX = 0.4;

        /// <summary>
        /// The ChampionSearchBarY follows this linear function: 0 + 0,1375 * ClientSize.X = Y
        /// </summary>
        private const double lfChampionSearchBarY = 0.1375;

        /// <summary>
        /// The ChampionSearchBarX follows this linear function: 0 + 0,66585 * ClientSize.X = X
        /// </summary>
        private const double lfChampionSearchBarX = 0.66585;

        /// <summary>
        /// The TeamChatBar follows this linear function: 0 + 0,2515 * ClientSize.X = Y
        /// </summary>
        private const double lfTeamChatBarY = 0.2515;

        /// <summary>
        /// The TeamChatBarX follows this linear function: 0 + 0,0 * ClientSize.X = X
        /// </summary>
        private const double lfTeamChatBarX = 0.22692;

        /// <summary>
        /// Your absolute position and size of your league client on your desktop.
        /// </summary>
        private Rectangle rectLolBounds;

        private static Process processLolClient;
        private static StaticLolClientGraphics _instance;

        #endregion Properties & Attributes

        #region Private DLL Import & Related structures
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
            if(_instance == null || lolClient != processLolClient) {
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

            //Calculate ChampionSearchBar position
            int ChampionSearchBarXpos = (int)(lfChampionSearchBarX * (double)rectLolBounds.Width - rectLolBounds.X);
            int ChampionSearchBarYpos = (int)(lfChampionSearchBarY * (double)rectLolBounds.Width - rectLolBounds.X);

            // get screen coordinates
            Point pntSearchBarAbsolute = new Point(rectLolBounds.Width - (rectLolBounds.Width - ChampionSearchBarXpos), rectLolBounds.Y + ChampionSearchBarYpos);
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
            bool isTrue = Process.GetCurrentProcess().Id == foreGroundWindowProcessId;
            return isTrue;
        }

        private Bitmap GetLeagueClientAsBitmap() {
            // Old lol bounds
            Rectangle origLolBounds = new Rectangle(rectLolBounds.X, rectLolBounds.Y, rectLolBounds.Width, rectLolBounds.Height);

            // New lol bounds
            GetWindowRect(processLolClient.MainWindowHandle, out rectLolBounds);

            // Check if client has repositioned
            if(rectLolBounds.X != origLolBounds.X || rectLolBounds.Y != origLolBounds.Y) {
                if(OnLeagueClientReposition != null) {
                    OnLeagueClientReposition(this, new EventArgs());
                }
            }

            // Check if client has been resized
            if(rectLolBounds.Width != origLolBounds.Width || rectLolBounds.Height != origLolBounds.Height) {
                if(OnLeagueClientResized != null) {
                    OnLeagueClientResized(this, new EventArgs());
                }
            }

            // Create bitmap of correct size
            Bitmap lolClientSized_Bitmap = new Bitmap(rectLolBounds.Width - rectLolBounds.X, rectLolBounds.Height - rectLolBounds.Y, PixelFormat.Format24bppRgb);
            Graphics lolClientSized_Graphics = Graphics.FromImage(lolClientSized_Bitmap);

            // Get source handle device context
            IntPtr lolClient_WindowDeviceContext = GetWindowDC(processLolClient.MainWindowHandle);

            try {
                // Get target handle device context
                IntPtr lolClient_MemoryHandleDeviceContext = lolClientSized_Graphics.GetHdc();

                // Copy source into target
                StretchBlt(lolClient_MemoryHandleDeviceContext, 0, 0, lolClientSized_Bitmap.Width, lolClientSized_Bitmap.Height,
                    lolClient_WindowDeviceContext, 0, 0, rectLolBounds.Width - rectLolBounds.X, rectLolBounds.Height - rectLolBounds.Y,
                    TernaryRasterOperations.SRCCOPY);

                // Release handle
                lolClientSized_Graphics.ReleaseHdc();

            } catch(ArgumentException) { }

            //If you wanna check how your in memory bitmap is correct (saved in: EasyChampionSelection\EasyChampionSelection\bin\Debug\)
            //lolClientSized_Bitmap.Save("processLeagueOfLegendsClientSizedBitmap.png");

            return lolClientSized_Bitmap;
        }

        public bool isInChampSelect() {
            Bitmap picOfClient = GetLeagueClientAsBitmap();

            Color white = Color.FromArgb(255, 255, 255);

            bool isWhite = false;

            if(rectLolBounds.Width < 0) {
                return false;
            }

            int clientWidth = rectLolBounds.Width - rectLolBounds.X;
            double clientCalculatedSizeX = lfClientSizeY * clientWidth;
            double clientWrongCalculatedSize = lfClientSizeY * rectLolBounds.Width;

            //Search if the ChampionSearchBar is there
            int ChampionSearchBarXpos = (int)(lfChampionSearchBarX * clientWidth);
            int ChampionSearchBarYpos = (int)(lfChampionSearchBarY * clientWidth);

            /*
            if(!(picOfClient.GetPixel(ChampionSearchBarXpos, ChampionSearchBarYpos).Equals(white))) {
                isWhite = false;
            } else { //Debugger hack, only do this because it's the first point to check so the outcome won't change
                isWhite = true;
            } */

            //Search if the TeamChatBar is there
            int TeamChatBarXpos = (int)(lfTeamChatBarX * clientWidth);
            int TeamChatBarYpos = (int)(lfTeamChatBarY * clientWidth);
            /*
            if(!(picOfClient.GetPixel(TeamChatBarXpos, TeamChatBarYpos).Equals(white))) {
                isWhite = false;
            } */

            List<Point> lP = new List<Point>();
            lP.Add(new Point(ChampionSearchBarXpos, ChampionSearchBarYpos)); //ChampionSearchBar
            lP.Add(new Point(TeamChatBarXpos, TeamChatBarYpos)); //TeamChat
            //ClientOverlayPosition
            lP.Add(new Point((int)(lfSkinsTabEndX * clientWidth), (int)(lfSkinsTabEndY * clientWidth)));
            MarkPointsOnBitmap(picOfClient, lP, 3);

            return isWhite;
        }

        public Rectangle GetClientOverlayPosition() {
            // Calculate where the skins tab is (not absolute! only position within client)
            int SkinsTabEndXpos = (int)(lfSkinsTabEndX * (double)rectLolBounds.Width - rectLolBounds.X);
            int SkinsTabEndYpos = (int)(lfSkinsTabEndY * (double)rectLolBounds.Width - rectLolBounds.X);

            Rectangle SkinsTabEndPosition = new Rectangle(SkinsTabEndXpos, SkinsTabEndYpos, 50, 300);

            //Now we need to add the absolute X and Y of client to it
            SkinsTabEndPosition.X += rectLolBounds.X;
            SkinsTabEndPosition.Y += rectLolBounds.Y;

            return SkinsTabEndPosition;
        }

        /// <summary>
        /// Debugger only function to see where your points actually are on the client
        /// </summary>
        /// <param name="image">Bitmap of client</param>
        /// <param name="points">Point(s) to mark</param>
        /// <param name="circles">How much circles to draw around it</param>
        private void MarkPointsOnBitmap(Bitmap image, List<Point> points, int circles) {
            //For each point in points[]
            for(int p = 0; p < points.Count; p++) {

                int MarkXstart, MarkYstart, MarkXend, MarkYend;
                MarkXstart = points[p].X - circles;
                MarkYstart = points[p].Y - circles;
                MarkXend = points[p].X + circles;
                MarkYend = points[p].Y + circles;

                int indexX, indexY;
                indexX = MarkXstart;
                indexY = MarkYstart;
                do {
                    do {
                        try {
                            image.SetPixel(indexX, indexY, Color.Green);
                        } catch(Exception e) {
                        }
                    
                        indexX++;
                    } while(indexX <= MarkXend);
                    indexY++;
                    indexX = MarkXstart;
                } while(indexY < MarkYend);
            }
            image.Save("ChampSelect_MarkedPoints_ " + points.Count + ".jpg", ImageFormat.Jpeg);
        }
    }
}
