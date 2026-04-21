using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    // https://social.msdn.microsoft.com/Forums/windows/en-US/0cc115ea-86cb-4dd0-924f-b5d74d22154c/resetting-the-mousehover-event-resetmouseeventargs-is-useless?forum=winforms
    // need some way of processing custom tool tips on an item list view with different tool tips being set whenever we switch tabs.
    // really not something easy to do, so the answer was to just show/hide them based on mouse-move events (plus whatever item we are currently hoving over)
    // this needs a mouse-hover check, and regular on_mousehover events kind of dont work consequtively (natively). so... why not use a helper class?
    // slight modifications were necessary:
    //  -addition of hover end event
    //  - minimum distance moved before ending check
    //  - switch to non-static design (to allow for better control checks
    //  - optimization of hover calls (only calls the control that was last moused over, not a all-check of all controls)
    public class MouseHoverDetector {
        private Timer _timer;
        private Dictionary<Control, Info> _items;
        // we will use this to ensure the hover start & end only happen to the given control
        private Control _lastMouseMoveControl;

        private TimeSpan _hoverTime;
        private TimeSpan _reshowTime;
        private int _hoverGraceDistance;

        public MouseHoverDetector(int hoverTimeMilliseconds = 200, int reshowTimeMilliseconds = 200, int hoverGraceDistance = 15) {
            _items = new Dictionary<Control, Info>();
            _timer = new Timer { Enabled = false, Interval = 50 };
            _timer.Tick += timer_Tick;
            _lastMouseMoveControl = null;

            _hoverTime = TimeSpan.FromMilliseconds(hoverTimeMilliseconds);
            _reshowTime = TimeSpan.FromMilliseconds(reshowTimeMilliseconds);
            _hoverGraceDistance = hoverGraceDistance;
        }

        public void Add(Control control, MouseEventHandler hoverStartEventHandler, EventHandler hoverEndEventHandler) {
            if (_items.TryGetValue(control, out var info)) {
                info.HoverStartHandler = hoverStartEventHandler;
                info.HoverEndHandler = hoverEndEventHandler;
            } else {
                if (_items.Count == 0)
                    _timer.Enabled = true;

                info = new Info {
                    HoverStartHandler = hoverStartEventHandler,
                    HoverEndHandler = hoverEndEventHandler,
                    IsHovering = false,
                    LastMoveTime = DateTime.Now
                };

                _items.Add(control, info);
                control.MouseMove += control_MouseMove;
                control.HandleDestroyed += control_HandleDestroyed;
            }
        }

        public void Remove(Control control) {
            if (_items.TryGetValue(control, out _)) {
                control.MouseMove -= control_MouseMove;
                control.HandleDestroyed -= control_HandleDestroyed;
                _items.Remove(control);
                if (_items.Count == 0)
                    _timer.Enabled = false;
            }
        }

        private class Info {
            public MouseEventHandler HoverStartHandler;
            public EventHandler HoverEndHandler;
            public DateTime LastMoveTime;
            public bool IsHovering;
            public Point HoverStartPoint;
        }

        private void control_MouseMove(object sender, MouseEventArgs e) {
            _lastMouseMoveControl = (Control) sender;
            var info = _items[(Control) sender];

            if (info.IsHovering) {
                if (Math.Abs(info.HoverStartPoint.X - e.Location.X) + Math.Abs(info.HoverStartPoint.Y - e.Location.Y) > _hoverGraceDistance) {
                    info.IsHovering = false;
                    // add a certain amount to the future counter
                    info.LastMoveTime = DateTime.Now + _reshowTime;
                    info.HoverEndHandler(sender, EventArgs.Empty);
                }
            } else if (info.LastMoveTime < DateTime.Now) {
                info.LastMoveTime = DateTime.Now;
            }
        }

        private void control_HandleDestroyed(object sender, EventArgs e) {
            Remove((Control) sender);
        }

        private void timer_Tick(object sender, EventArgs e) {
            if (_lastMouseMoveControl == null)
                return;

            var now = DateTime.Now;
            //Console.WriteLine(now);

            var info = _items[_lastMouseMoveControl];
            if (!info.IsHovering && now - info.LastMoveTime > _hoverTime) {
                info.IsHovering = true;
                info.HoverStartPoint = _lastMouseMoveControl.PointToClient(Control.MousePosition);
                info.HoverStartHandler(_lastMouseMoveControl, new MouseEventArgs(MouseButtons.None, 0, info.HoverStartPoint.X, info.HoverStartPoint.Y, 0));
            }
        }

        public void Dispose() {
            _timer.Dispose();
        }
    }
}