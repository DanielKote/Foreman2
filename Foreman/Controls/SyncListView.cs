using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
    //pretty much this \/ . Didnt want to bother with something more complicated.
    //https://stackoverflow.com/questions/14726146/scrolling-list-view-when-another-list-view-is-scrolled
    //NOTE: using the 'sendmessage' approached failed, so had to switch to a 'set-top-index' approach
    class SyncListView : ListView
    {
        public SyncListView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public SyncListView Buddy { get; set; }

        private static bool scrolling;   // In case buddy tries to scroll us

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Trap WM_VSCROLL message and set the top item of buddy to be the same index as top item of this. (message cloning was proven not to work with mouse-scrollbar situations)
            if ((m.Msg == 0x115 || m.Msg == 0xb5 || m.Msg == 0x20a) && !scrolling && Buddy != null && Buddy.IsHandleCreated)
            {
                scrolling = true;
                Buddy.TopItem = Buddy.Items[this.TopItem.Index];
                scrolling = false;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

    }

    class FFListView : ListView
    {
        public FFListView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
