using System;

namespace Foreman;

public interface Fluid : Item {
    bool IsTemperatureDependent { get; }
    double DefaultTemperature { get; }
    double SpecificHeatCapacity { get; }
    double GasTemperature { get; }
    double MaxTemperature { get; }

    string GetTemperatureRangeFriendlyName(FRange tempRange);
    string GetTemperatureFriendlyName(double temperature);
}

public class FluidPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false)
    : ItemPrototype(dCache, name, friendlyName, subgroup, order, isMissing), Fluid {
    // true if not all recipes can accept each other
    // (ex: fluid produced in R1 is at 10*c, and is required to be at 20+*c as ingredient at R2)
    public bool IsTemperatureDependent { get; internal set; }

    public double DefaultTemperature { get; internal set; }
    public double SpecificHeatCapacity { get; internal set; }
    public double GasTemperature { get; internal set; }
    public double MaxTemperature { get; internal set; }

    public string GetTemperatureRangeFriendlyName(FRange tempRange) {
        if (tempRange.Ignore)
            return FriendlyName;

        var name = FriendlyName;
        var includeMin = tempRange.Min >= double.MinValue;
        var includeMax = tempRange.Max <= double.MaxValue;

        if (Math.Abs(tempRange.Min - tempRange.Max) < double.Epsilon)
            name += $" ({tempRange.Min:0}°c)";
        else if (includeMin && includeMax)
            name += $" ({tempRange.Min:0}-{tempRange.Max:0}°c)";
        else if (includeMin)
            name += $" (min {tempRange.Min:0}°c)";
        else if (includeMax)
            name += $" (max {tempRange.Max:0}°c)";
        else
            name += "(any°)";

        return name;
    }

    public string GetTemperatureFriendlyName(double temperature) {
        return $"{FriendlyName} ({temperature:0}°c)";
    }


    public override string ToString() {
        return $"Item: {Name}";
    }
}