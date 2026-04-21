using System.Collections.Generic;

namespace Foreman {
    public interface Quality : DataObjectBase {
        Quality NextQuality { get; }
        Quality PrevQuality { get; }
        double NextProbability { get; }

        bool IsMissing { get; }

        // 'power' of the quality
        int Level { get; }
        double BeaconPowerMultiplier { get; }
        double MiningDrillResourceDrainMultiplier { get; }

        IReadOnlyCollection<Technology> MyUnlockTechnologies { get; }
        IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; }
    }

    public class QualityPrototype : DataObjectBasePrototype, Quality {
        public Quality NextQuality { get; internal set; }
        public Quality PrevQuality { get; internal set; }
        public double NextProbability { get; set; }

        public bool IsMissing { get; private set; }

        public int Level { get; internal set; }
        public double BeaconPowerMultiplier { get; set; }
        public double MiningDrillResourceDrainMultiplier { get; set; }

        public IReadOnlyCollection<Technology> MyUnlockTechnologies => myUnlockTechnologies;

        public IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; set; }

        internal HashSet<TechnologyPrototype> myUnlockTechnologies { get; private set; }

        public QualityPrototype(DataCache dCache, string name, string friendlyName, string order, bool isMissing = false) : base(dCache, name, friendlyName,
            order) {
            Enabled = true;
            IsMissing = isMissing;

            NextProbability = 0;
            NextQuality = null;
            PrevQuality = null;

            Level = 0;
            BeaconPowerMultiplier = 1;
            MiningDrillResourceDrainMultiplier = 1;

            myUnlockTechnologies = [];
            MyUnlockSciencePacks = new List<List<Item>>();
        }

        public override string ToString() {
            return $"Quality T{Level}: {Name}";
        }
    }
}