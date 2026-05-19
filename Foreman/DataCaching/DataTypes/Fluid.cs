namespace Foreman.DataCaching.DataTypes {
    public interface IFluid : IItem {
        bool IsTemperatureDependent { get; }
        double DefaultTemperature { get; }
        double SpecificHeatCapacity { get; }
        double GasTemperature { get; }
        double MaxTemperature { get; }

        string GetTemperatureRangeFriendlyName(FRange tempRange);
        string GetTemperatureFriendlyName(double temperature);
    }

    public class FluidPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : ItemPrototype(dCache, name, friendlyName, subgroup, order, isMissing), IFluid {
        public bool IsTemperatureDependent { get; internal set; }
        public double DefaultTemperature { get; internal set; }
        public double SpecificHeatCapacity { get; internal set; }
        public double GasTemperature { get; internal set; }
        public double MaxTemperature { get; internal set; }

        public string GetTemperatureRangeFriendlyName(FRange tempRange) {
            if (tempRange.Ignore)
                return FriendlyName;

            string name = FriendlyName;
            bool includeMin = tempRange.Min >= double.MinValue;
            bool includeMax = tempRange.Max <= double.MaxValue;

            if (tempRange.Min == tempRange.Max)
                name += string.Format(DisplayCulture.Format, " ({0}°c)", tempRange.Min.ToString("0", DisplayCulture.Format));
            else if (includeMin && includeMax)
                name += string.Format(DisplayCulture.Format, " ({0}-{1}°c)", tempRange.Min.ToString("0", DisplayCulture.Format), tempRange.Max.ToString("0", DisplayCulture.Format));
            else if (includeMin)
                name += string.Format(DisplayCulture.Format, " (min {0}°c)", tempRange.Min.ToString("0", DisplayCulture.Format));
            else if (includeMax)
                name += string.Format(DisplayCulture.Format, " (max {0}°c)", tempRange.Max.ToString("0", DisplayCulture.Format));
            else
                name += "(any°)";

            return name;
        }

        public string GetTemperatureFriendlyName(double temperature) {
            return string.Format(DisplayCulture.Format, "{0} ({1}°c)", FriendlyName, temperature.ToString("0", DisplayCulture.Format));
        }


        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Item: {0}", Name); }
    }
}
