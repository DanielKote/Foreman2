using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public interface Quality : DataObjectBase {
        Quality? NextQuality { get; }
        Quality? PrevQuality { get; }
        double NextProbability { get; }

        bool IsMissing { get; }

        int Level { get; } //'power' of the quality
        double BeaconPowerMultiplier { get; }
        double MiningDrillResourceDrainMultiplier { get; }

        IReadOnlyCollection<Technology> MyUnlockTechnologies { get; }
        IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; }
    }

    public class QualityPrototype : DataObjectBasePrototype, Quality {
        public Quality? NextQuality { get; internal set; }
        public Quality? PrevQuality { get; internal set; }
        public double NextProbability { get; set; }

        public bool IsMissing { get; private set; }

        public int Level { get; internal set; }
        public double BeaconPowerMultiplier { get; set; }
        public double MiningDrillResourceDrainMultiplier { get; set; }

        public IReadOnlyCollection<Technology> MyUnlockTechnologies { get { return myUnlockTechnologies; } }
        public IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; set; }

        internal HashSet<TechnologyPrototype> myUnlockTechnologies { get; private set; }

        public QualityPrototype(DataCache dCache, string name, string friendlyName, string order, bool isMissing = false) : base(dCache, name, friendlyName, order) {
            Enabled = true;
            IsMissing = isMissing;

            NextProbability = 0;
            NextQuality = null;
            PrevQuality = null;

            Level = 0;
            BeaconPowerMultiplier = 1;
            MiningDrillResourceDrainMultiplier = 1;

            myUnlockTechnologies = new HashSet<TechnologyPrototype>();
            MyUnlockSciencePacks = new List<List<Item>>();
        }

        public override string ToString() { return string.Format("Quality T{0}: {1}", Level, Name); }
    }
}