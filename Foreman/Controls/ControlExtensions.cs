using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	static class ControlExtensions
	{
		static public void UIThread(this Control control, Action code)
		{
			if (control.InvokeRequired)
			{
				control.BeginInvoke(code);
				return;
			}
			code.Invoke();
		}

		static public void UIThreadInvoke(this Control control, Action code)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(code);
				return;
			}
			code.Invoke();
		}
	}
}
