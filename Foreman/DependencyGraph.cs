using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foreman;

namespace Foreman
{
	class DependencyGraph
	{
		private List<Mod> mods = new List<Mod>();
		public int[,] adjacencyMatrix;

		public DependencyGraph(List<Mod> mods)
		{
			this.mods = mods;
		}

		public void DisableUnsatisfiedMods()
		{
			bool changeMade = true;
			while (changeMade)
			{
				changeMade = false;
				foreach (Mod mod in mods)
				{
					foreach (ModDependency dep in mod.parsedDependencies)
					{
						if (!DependencySatisfied(dep))
						{
							if (mod.Enabled)
							{
								changeMade = true;
								mod.Enabled = false;
							}
						}
					}
				}
			}
		}

		//Assumes no dependency cycles
		public List<Mod> SortMods()
		{
			UpdateAdjacency();

			List<Mod> L = new List<Mod>();
			HashSet<Mod> S = new HashSet<Mod>();

			// Get all mods with no incoming dependencies
			for (int i = 0; i < mods.Count; i++)
			{
				bool dependency = false;
				for (int j = 0; j < mods.Count(); j++)
				{
					if (adjacencyMatrix[j, i] == 1)
					{
						dependency = true;
						break;
					}
				}
				if (!dependency)
				{
					S.Add(mods[i]);
				}
			}

			while (S.Any())
			{
				Mod n = S.First();
				S.Remove(n);

				L.Add(n);
				int nIndex = mods.IndexOf(n);

				for (int m = 0; m < mods.Count; m++)
				{
					if (adjacencyMatrix[nIndex, m] == 1)
					{
						adjacencyMatrix[nIndex, m] = 0;
						
						bool incoming = false;
						for (int i = 0; i < mods.Count; i++)
						{
							if (adjacencyMatrix[i, m] == 1)
							{
								incoming = true;
								break;
							}
						}
						if (!incoming)
						{
							S.Add(mods[m]);
						}
					}
				}
			}

			//Should be no edges (dependencies) left by here

			L.Reverse();
			return L;
		}

		public void UpdateAdjacency()
		{
			adjacencyMatrix = new int[mods.Count(), mods.Count()];

			for (int i = 0; i < mods.Count; i++)
			{
				for (int j = 0; j < mods.Count; j++)
				{
					if (mods[i].DependsOn(mods[j], false))
					{
						adjacencyMatrix[i, j] = 1;
					}
					else
					{
						adjacencyMatrix[i, j] = 0;
					}
				}
			}
		}

		public bool DependencySatisfied(ModDependency dep)
		{
			if (dep.Optional)
			{
				return true;
			}

			foreach (Mod mod in mods.Where(m => m.Enabled))
			{
				if (mod.SatisfiesDependency(dep))
				{
					return true;
				}
			}
			return false;
		}
	}
}
