using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	class Inserter
	{
		public String Name { get; private set; }
		public float RotationSpeed { get; set; }
		public Bitmap Icon { get; set; }
		private String friendlyName;
		public String FriendlyName
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(friendlyName))
				{
					return friendlyName;
				}
				else
				{
					return Name;
				}
			}
			set
			{
				friendlyName = value;
			}
		}

		public Inserter(String name)
		{
			Name = name;
		}
	}
}
