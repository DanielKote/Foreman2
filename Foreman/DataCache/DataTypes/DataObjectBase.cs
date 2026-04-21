using System;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public interface DataObjectBase : IComparable<DataObjectBase> {
        DataCache Owner { get; }

        string Name { get; }
        string LFriendlyName { get; }
        string FriendlyName { get; }

        bool Available { get; }
        bool Enabled { get; set; }

        Bitmap Icon { get; }
        Color AverageColor { get; }
        void SetIconAndColor(IconColorPair icp);
    }

    public abstract class DataObjectBasePrototype : DataObjectBase {
        private static readonly char[] OrderSeparators = ['[', ']'];

        public DataCache Owner { get; private set; }

        public string Name { get; private set; }
        public string LFriendlyName { get; private set; }
        public string FriendlyName { get; private set; }

        public virtual bool Available { get; set; }
        public bool Enabled { get; set; }

        private string[] _orderCompareArray;

        public DataObjectBasePrototype(DataCache dCache, string name, string friendlyName, string order) {
            Owner = dCache;
            Name = name;
            FriendlyName = friendlyName;
            LFriendlyName = friendlyName.ToLower();

            Available = true;
            Enabled = true;

            Icon = DataCache.UnknownIcon;
            AverageColor = Color.Black;

            _orderCompareArray = order.Split(OrderSeparators).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        public void SetIconAndColor(IconColorPair icp) {
            Icon = icp.Icon ?? DataCache.UnknownIcon;
            AverageColor = icp.Color;
        }

        public Color AverageColor { get; private set; }
        public Bitmap Icon { get; private set; }

        public override bool Equals(object obj) {
            return obj as DataObjectBasePrototype == this;
        }

        public static bool operator ==(DataObjectBasePrototype doBase1, DataObjectBasePrototype doBase2) {
            if (ReferenceEquals(doBase1, doBase2))
                return true;
            if ((object) doBase1 == null || (object) doBase2 == null)
                return false;
            if (doBase1.GetType() != doBase2.GetType())
                return false;
            return doBase1.Name == doBase2.Name;
        }

        public static bool operator !=(DataObjectBasePrototype recipe1, DataObjectBasePrototype recipe2) {
            return !(recipe1 == recipe2);
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public int CompareTo(DataObjectBase other) {
            // order comparison is apparently quite convoluted - any time we have brackets ([ or ]), it signifies a different order part.
            // each part is compared char-by-char, and in the case of the longer string it goes first.
            // in terms of sections, the sorter section goes first (ex: a[0] goes before a[0]-1)

            if (other is not DataObjectBasePrototype otherP)
                return 0;

            for (var i = 0; i < _orderCompareArray.Length && i < otherP._orderCompareArray.Length; i++) {
                for (var j = 0; j < _orderCompareArray[i].Length && j < otherP._orderCompareArray[i].Length; j++) {
                    var result = _orderCompareArray[i][j].CompareTo(otherP._orderCompareArray[i][j]);
                    if (result != 0)
                        return result;
                }

                if (_orderCompareArray[i].Length != otherP._orderCompareArray[i].Length)
                    return _orderCompareArray[i].Length > otherP._orderCompareArray[i].Length ? -1 : 1;
            }

            if (_orderCompareArray.Length != otherP._orderCompareArray.Length)
                return _orderCompareArray.Length < otherP._orderCompareArray.Length ? -1 : 1;

            return string.Compare(LFriendlyName, otherP.LFriendlyName, StringComparison.Ordinal);
        }
    }
}