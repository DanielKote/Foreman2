using System;

namespace Foreman.DataCaching.DataTypes {

    public interface IBeacon : IEntityObjectBase {
        double GetBeaconEffectivity(IQuality quality, double beaconCount);
    }

    internal class BeaconPrototype : EntityObjectBasePrototype, IBeacon {

        public double GetBeaconEffectivity(IQuality quality, double beaconCount) {
            if (beaconCount <= 0)
                return 0;
            if (beaconCount > 999)
                beaconCount = 999;

            int baseCount = (int)Math.Truncate(beaconCount);
            double remainder = beaconCount - baseCount;

            double lowerBeaconEffectivity = baseCount * GetBeaconEffectivityBase(quality, baseCount);
            double upperBeaconEffectivity = (baseCount + 1) * GetBeaconEffectivityBase(quality, baseCount + 1);

            return (((1 - remainder) * lowerBeaconEffectivity) + (remainder * upperBeaconEffectivity)) / beaconCount;
            //just to explain - we need to calculate the 'average' beacon effectivity for a beacon count of (for example) 1.6:
            //this means that for every 10 assemblers you have 6 with 2 beacons and 4 with 1 beacon. So calculate the total bonus and divide it by beaconCount.
            //therefore 10 assemblers with beacon count of 1.6 will produce exactly the same amount as 6 assemblers with 2 beacons + 4 assemblers with 1 beacon
        }

        private double GetBeaconEffectivityBase(IQuality quality, int beaconCount) {
            return profile[beaconCount] * (DistributionEffectivity + (quality.Level * DistributionEffectivityQualityBoost));
        }

        internal double DistributionEffectivity { get; set; }
        internal double DistributionEffectivityQualityBoost { get; set; }

        internal double[] profile { get; private set; } //off by 1 from factorio (or more akin same index as LUA starts from 0) ==> profile[x] is the multiplier for x beacons (so profile[0] is multiplier for 0 beacons...)

        public BeaconPrototype(DataCache dCache, string name, string friendlyName, EnergySource source, bool isMissing = false) : base(dCache, name, friendlyName, EntityType.Beacon, source, isMissing) {
            profile = new double[1000];
            for (int i = 1; i < profile.Length; i++) { profile[i] = 0.5f; }
            DistributionEffectivity = 0.5f;
            DistributionEffectivityQualityBoost = 0f;
        }

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Beacon: {0}", Name); }
    }
}
