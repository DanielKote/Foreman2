using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
    //slight modification from:
    //https://social.msdn.microsoft.com/Forums/en-US/46ab566a-5937-415c-9f80-578937d21b64/how-to-change-color-of-specific-items-in-checkedlistbox-in-c?forum=windowsgeneraldevelopmentissues
    public class DataObjectCheckedListBox : CheckedListBox
    {
        public DataObjectCheckedListBox()
        {
            DoubleBuffered = true;
        }
        public List<Brush> ItemBrushes = new List<Brush>();

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Size checkSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
            int dx = (e.Bounds.Height - checkSize.Width) / 2;
            e.DrawBackground();
            bool isChecked = (e.Index < Items.Count)? GetItemChecked(e.Index) : true;
            string text = (e.Index < Items.Count) ? ((DataObjectBase)Items[e.Index]).FriendlyName : Name;
            Brush brush = (e.Index < ItemBrushes.Count) ? ItemBrushes[e.Index] : Brushes.Black;
            CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(dx, e.Bounds.Top + dx), isChecked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);

            Font myFont = e.Font;
            e.Graphics.DrawString(text, myFont, brush, new Rectangle(e.Bounds.Height, e.Bounds.Top, e.Bounds.Width - e.Bounds.Height, e.Bounds.Height), StringFormat.GenericDefault);
        }
    }
}
