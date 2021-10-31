
namespace Foreman
{
	public interface Beacon : EntityObjectBase
	{
		double BeaconEffectivity { get; }
	}

	internal class BeaconPrototype : EntityObjectBasePrototype, Beacon
	{
		public double BeaconEffectivity { get; set; }

		public BeaconPrototype(DataCache dCache, string name, string friendlyName, EnergySource source, bool isMissing = false) : base(dCache, name, friendlyName, EntityType.Beacon, source, isMissing)
		{
			BeaconEffectivity = 0.5f;
		}

		public override string ToString() { return string.Format("Beacon: {0}", Name); }
	}
}
