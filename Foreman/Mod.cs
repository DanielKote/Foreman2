using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public class Mod
	{
        public String Id = "";
		public String Name = "";
		public String title = "";
		public String version = "";
		public Version parsedVersion;
		public String dir = "";
		public String description = "";
		public String author = "";
		public List<String> dependencies = new List<String>();
		public List<ModDependency> parsedDependencies = new List<ModDependency>();
		public bool Enabled = true;

		public bool SatisfiesDependency(ModDependency dep)
		{
			if (Name != dep.ModName)
			{
				return false;
			}
			if (dep.Version != null)
			{
				if (dep.VersionType == DependencyType.EqualTo
					&& parsedVersion != dep.Version)
				{
					return false;
				}
				if (dep.VersionType == DependencyType.GreaterThan
					&& parsedVersion <= dep.Version)
				{
					return false;
				}
				if (dep.VersionType == DependencyType.GreaterThanOrEqual
					&& parsedVersion < dep.Version)
				{
					return false;
				}
			}
			return true;
		}

		public bool DependsOn(Mod mod, bool ignoreOptional)
		{
			IEnumerable<ModDependency> depList;
			if (ignoreOptional)
			{
				depList = parsedDependencies.Where(d => !d.Optional);
			} else {
				depList = parsedDependencies;
			}
			foreach (ModDependency dep in depList)
			{
				if (mod.SatisfiesDependency(dep))
				{
					return true;
				}
			}
			return false;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class ModDependency
	{
		public String ModName = "";
		public Version Version;
		public bool Optional = false;
		public DependencyType VersionType = DependencyType.EqualTo;

		public override string ToString()
		{
            String type = "";
            switch (VersionType)
            {
                case DependencyType.EqualTo:
                    type = "=";
                    break;
                case DependencyType.GreaterThan:
                    type = ">";
                    break;
                case DependencyType.GreaterThanOrEqual:
                    type = ">=";
                    break;
            }
			return ModName + " " + type + " " + Version;
		}
	}

	public enum DependencyType
	{
		EqualTo,
		GreaterThan,
		GreaterThanOrEqual
	}
}
