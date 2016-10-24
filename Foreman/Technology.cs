using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Foreman
{
    class Technology
    {
        public String Name { get; private set; }
        public Bitmap Icon { get; set; }
        public List<Recipe> unlocks { get; set; }
        public List<Technology> Prerequisites { get; set; }   //TODO: Interpret these in DataCache.InterpretTechnologies
        public Boolean Enabled { get; set; }

        public String FriendlyName
        {
            get
            {
                if (DataCache.LocaleFiles["technology-name"].ContainsKey(Name))
                {
                    return DataCache.LocaleFiles["recipe-name"][Name];
                }
                else
                {
                    return Name;
                }
            }
        }

        public Technology(String Name)
        {
            this.Name = Name;
            this.unlocks = new List<Recipe>();
            this.Enabled = true;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Technology))
            {
                return false;
            }

            return (obj as Technology) == this;
        }

        public static bool operator ==(Technology technology1, Technology technology2)
        {
            if (object.ReferenceEquals(technology1, technology2))
            {
                return true;
            }

            if ((object)technology1 == null || (object)technology2 == null)
            {
                return false;
            }

            return technology1.Name == technology2.Name;
        }

        public static bool operator !=(Technology technology1, Technology technology2)
        {
            return !(technology1 == technology2);
        }
    }
}