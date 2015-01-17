using System.Windows;

namespace EasyChampionSelection.ECS {
    /// <summary>
    /// Utilities for <c>System.Windows</c>
    /// </summary>
    public static class StaticWindowUtilities {

        /// <summary>
        /// Ensure our window is visible (I hate it when other programs splash over a user wants to open this program)
        /// </summary>
        public static void EnsureVisibility(Window w) {
            w.Activate();
            w.Topmost = true;
            w.Topmost = false;
            w.Focus();
        }
    }
}
