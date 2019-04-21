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
            this.tooltip.InitialDelay = 200;
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int newIndex = this.IndexFromPoint(e.Location);
            if (tooltipIndex != newIndex)
            {
                UpdateToolTip(newIndex);
            }

            base.OnMouseMove(e);
        }

        private void UpdateToolTip(int newIndex)
        {
            tooltipIndex = newIndex;
            if (tooltipIndex > -1 && errors.ContainsKey(tooltipIndex))
            {
                tooltip.Active = true;
                tooltip.SetToolTip(this, errors[tooltipIndex]);
            }
            else
            {
                tooltip.Active = false;
            }
        }
    }
}
