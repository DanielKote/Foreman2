using Foreman.DataCaching;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.Forms {
    /// <summary>Deduplicates bitmap references when populating the settings enabled-objects <see cref="ImageList"/>.</summary>
    internal sealed class EnabledObjectsIconIndex {
        private readonly ImageList iconList;
        private readonly Dictionary<Bitmap, int> indexByBitmap;

        public EnabledObjectsIconIndex(ImageList iconList) {
            this.iconList = iconList;
            iconList.Images.Clear();
            iconList.Images.Add(DataCache.UnknownIcon);
            indexByBitmap = new Dictionary<Bitmap, int>(ReferenceEqualityComparer.Instance) {
                [DataCache.UnknownIcon] = 0,
            };
        }

        public int GetImageIndex(Bitmap? icon) {
            if (icon is null)
                return 0;
            if (indexByBitmap.TryGetValue(icon, out int index))
                return index;
            iconList.Images.Add(icon);
            index = iconList.Images.Count - 1;
            indexByBitmap[icon] = index;
            return index;
        }

        public int ImageCount => iconList.Images.Count;
    }
}
