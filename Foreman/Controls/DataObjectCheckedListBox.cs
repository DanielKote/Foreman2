using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    // slight modification from:
    // https://social.msdn.microsoft.com/Forums/en-US/46ab566a-5937-415c-9f80-578937d21b64/how-to-change-color-of-specific-items-in-checkedlistbox-in-c?forum=windowsgeneraldevelopmentissues
    public class DataObjectCheckedListBox : CheckedListBox {
        public DataObjectCheckedListBox() {
            DoubleBuffered = true;
        }

        public List<Brush> ItemBrushes = [];

        protected override void OnDrawItem(DrawItemEventArgs e) {
            var checkSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
            var dx = (e.Bounds.Height - checkSize.Width) / 2;
            e.DrawBackground();
            var isChecked = e.Index >= Items.Count || GetItemChecked(e.Index);
            var text = e.Index < Items.Count ? ((DataObjectBase) Items[e.Index]).FriendlyName : Name;
            var brush = e.Index < ItemBrushes.Count ? ItemBrushes[e.Index] : Brushes.Black;
            CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(dx, e.Bounds.Top + dx),
                isChecked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);

            var myFont = e.Font;
            e.Graphics.DrawString(text, myFont, brush, new Rectangle(e.Bounds.Height, e.Bounds.Top, e.Bounds.Width - e.Bounds.Height, e.Bounds.Height),
                StringFormat.GenericDefault);
        }
    }
}