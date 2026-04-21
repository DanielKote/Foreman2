using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Foreman {
    // pretty much this \/ . Didn't want to bother with something more complicated.
    // https://stackoverflow.com/questions/14726146/scrolling-list-view-when-another-list-view-is-scrolled
    // NOTE: using the 'sendmessage' approached failed, so had to switch to a 'set-top-index' approach
    class SyncListView : FfListView {
        public SyncListView Buddy { get; set; }

        [DefaultValue(true)] public bool SyncScrolling { get; set; }

        [DefaultValue(true)] public bool SyncSelection { get; set; }

        // In case buddy tries to scroll us
        private static bool _scrolling;

        public SyncListView() {
            SyncScrolling = true;
            SyncSelection = true;

            ItemSelectionChanged += SyncListView_ItemSelectionChanged;
        }

        private void SyncListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
            if (SyncSelection && Buddy is { IsHandleCreated: true } && Buddy.Items[e.ItemIndex].Selected != e.IsSelected)
                Buddy.Items[e.ItemIndex].Selected = e.IsSelected;
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            // Trap WM_VSCROLL message and set the top item of buddy to be the same index as top item of this.
            // (message cloning was proven not to work with mouse-scrollbar situations)

            if (SyncScrolling && m.Msg is 0x115 or 0xb5 or 0x20a && !_scrolling && Buddy is { IsHandleCreated: true }) {
                _scrolling = true;
                Buddy.TopItem = Buddy.Items[TopItem.Index];
                _scrolling = false;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }

    class FfListView : ListView {
        public FfListView() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}