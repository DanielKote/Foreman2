using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Foreman
{
	public class CheckboxListWithErrors : CheckedListBox
	{
        public Dictionary<int, string> errors = new Dictionary<int, string>();
        private int tooltipIndex = -1;
        private ToolTip tooltip;

        public CheckboxListWithErrors()
        {
            this.tooltip = new ToolTip();
        }

        public void setError(int index, string error)
        {
            errors.Add(index, error);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Color foreColor = e.ForeColor;
            if (errors.ContainsKey(e.Index))
            {
                foreColor = Color.Red;
            }

            // Copy the original event args, just tweaking the fore color.
            var tweakedEventArgs = new DrawItemEventArgs(
                e.Graphics,
                e.Font,
                e.Bounds,
                e.Index,
                e.State,
                foreColor,
                e.BackColor);

            // Call the original OnDrawItem, but supply the tweaked color.
            base.OnDrawItem(tweakedEventArgs);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            int newIndex = this.IndexFromPoint(this.PointToClient(MousePosition));
            if (tooltipIndex != newIndex)
                ShowToolTip(newIndex);

            base.OnMouseHover(e);
        }

        private void ShowToolTip(int newIndex)
        {
            tooltipIndex = newIndex;
            if (tooltipIndex > -1 && errors.ContainsKey(tooltipIndex))
            {
                tooltip.SetToolTip(this, errors[tooltipIndex]);
            }
        }
    }
}
