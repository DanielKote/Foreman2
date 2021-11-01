using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{
	public interface Fluid : Item
	{
		bool IsTemperatureDependent { get; }
		double DefaultTemperature { get; }
		double SpecificHeatCapacity { get; }

		string GetTemperatureRangeFriendlyName(fRange tempRange);
		string GetTemperatureFriendlyName(double temperature);
	}

	public class FluidPrototype : ItemPrototype, Fluid
	{
		public bool IsTemperatureDependent { get; internal set; } //true if not all recipes can accept each other (ex: fluid produced in R1 is at 10*c, and is required to be at 20+*c as ingredient at R2)
		public double DefaultTemperature { get; internal set; }
		public double SpecificHeatCapacity { get; internal set; }

		public FluidPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, subgroup, order, isMissing)
		{
			IsTemperatureDependent = false;
			DefaultTemperature = 0;
			SpecificHeatCapacity = 0;
		}

		public string GetTemperatureRangeFriendlyName(fRange tempRange)
		{
			if (tempRange.Ignore)
				return FriendlyName;

			string name = FriendlyName;
			bool includeMin = tempRange.Min >= double.MinValue;
			bool includeMax = tempRange.Max <= double.MaxValue;

			if (tempRange.Min == tempRange.Max)
				name += string.Format(" ({0}°c)", tempRange.Min.ToString("0"));
			else if (includeMin && includeMax)
				name += string.Format(" ({0}-{1}°c)", tempRange.Min.ToString("0"), tempRange.Max.ToString("0"));
			else if (includeMin)
				name += string.Format(" (min {0}°c)", tempRange.Min.ToString("0"));
			else if (includeMax)
				name += string.Format(" (max {0}°c)", tempRange.Max.ToString("0"));
			else
				name += "(any°)";

			return name;
		}

		public string GetTemperatureFriendlyName(double temperature)
		{
			return string.Format("{0} ({1}°c)", FriendlyName, temperature.ToString("0"));
		}


		public override string ToString() { return string.Format("Item: {0}", Name); }
	}
}
