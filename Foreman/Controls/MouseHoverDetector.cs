using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
	//https://social.msdn.microsoft.com/Forums/windows/en-US/0cc115ea-86cb-4dd0-924f-b5d74d22154c/resetting-the-mousehover-event-resetmouseeventargs-is-useless?forum=winforms
	//need some way of processing custom tool tips on an item list view with different tool tips being set whenever we switch tabs.
	//really not something easy to do, so the answer was to just show/hide them based on mouse-move events (plus whatever item we are currently hoving over)
	//this needs a mouse-hover check, and regular on_mousehover events kind of dont work consequtively (natively). so... why not use a helper class?
	//slight modifications were necessary:
	//  -addition of hover end event
	//  - minimum distance moved before ending check
	//  - switch to non-static design (to allow for better control checks
	//  - optimization of hover calls (only calls the control that was last moused over, not a all-check of all controls)
	public class MouseHoverDetector
	{
		private Timer timer;
		private Dictionary<Control, Info> items;
		private Control lastMouseMoveControl; //we will use this to ensure the hover start & end only happen to the given control

		private TimeSpan HoverTime;
		private TimeSpan ReshowTime;
		private int HoverGraceDistance;

		public MouseHoverDetector(int hoverTimeMilliseconds = 200, int reshowTimeMilliseconds = 200, int hoverGraceDistance = 15)
		{
			items = new Dictionary<Control, Info>();
			timer = new Timer { Enabled = false, Interval = 50 };
			timer.Tick += new EventHandler(timer_Tick);
			lastMouseMoveControl = null;

			HoverTime = TimeSpan.FromMilliseconds(hoverTimeMilliseconds);
			ReshowTime = TimeSpan.FromMilliseconds(reshowTimeMilliseconds);
			HoverGraceDistance = hoverGraceDistance;
		}

		public void Add(Control control, MouseEventHandler hoverStartEventHandler, EventHandler hoverEndEventHandler)
		{
			Info info;
			if (items.TryGetValue(control, out info))
			{
				info.HoverStartHandler = hoverStartEventHandler;
				info.HoverEndHandler = hoverEndEventHandler;
			}
			else
			{
				if (items.Count == 0)
					timer.Enabled = true;

				info = new Info
				{
					HoverStartHandler = hoverStartEventHandler,
					HoverEndHandler = hoverEndEventHandler,
					IsHovering = false,
					LastMoveTime = DateTime.Now
				};

				items.Add(control, info);
				control.MouseMove += new MouseEventHandler(control_MouseMove);
				control.HandleDestroyed += new EventHandler(control_HandleDestroyed);
			}
		}

		public void Remove(Control control)
		{
			Info info;
			if (items.TryGetValue(control, out info))
			{
				control.MouseMove -= new MouseEventHandler(control_MouseMove);
				control.HandleDestroyed -= new EventHandler(control_HandleDestroyed);
				items.Remove(control);
				if (items.Count == 0)
					timer.Enabled = false;
			}
		}

		private class Info
		{
			public MouseEventHandler HoverStartHandler;
			public EventHandler HoverEndHandler;
			public DateTime LastMoveTime;
			public bool IsHovering;
			public Point HoverStartPoint;
		}

		private void control_MouseMove(object sender, MouseEventArgs e)
		{
			lastMouseMoveControl = (Control)sender;
			Info info = items[(Control)sender];

			if (info.IsHovering)
			{
				if (Math.Abs(info.HoverStartPoint.X - e.Location.X) + Math.Abs(info.HoverStartPoint.Y - e.Location.Y) > HoverGraceDistance)
				{
					info.IsHovering = false;
					info.LastMoveTime = DateTime.Now + ReshowTime; //add a certain amount to the future counter
					info.HoverEndHandler(sender, EventArgs.Empty);
				}
			}
			else if (info.LastMoveTime < DateTime.Now)
			{
				info.LastMoveTime = DateTime.Now;
			}
		}

		private void control_HandleDestroyed(object sender, EventArgs e)
		{
			Remove((Control)sender);
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (lastMouseMoveControl == null)
				return;

			DateTime now = DateTime.Now;
			//Console.WriteLine(now);

			Info info = items[lastMouseMoveControl];
			if (!info.IsHovering && now - info.LastMoveTime > HoverTime)
			{
				info.IsHovering = true;
				info.HoverStartPoint = lastMouseMoveControl.PointToClient(Control.MousePosition);
				info.HoverStartHandler(lastMouseMoveControl, new MouseEventArgs(MouseButtons.None, 0, info.HoverStartPoint.X, info.HoverStartPoint.Y, 0));
			}
		}

		public void Dispose()
		{
			timer.Dispose();
		}
	}
}
