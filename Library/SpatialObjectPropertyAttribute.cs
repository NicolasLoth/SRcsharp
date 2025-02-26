using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public enum SpatialObjectPropertyType { Unknown, DeclaredSpatial, DeclaredNonSpatial, Derived }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SpatialObjectPropertyAttribute : System.Attribute
    {
        public SpatialObjectPropertyType Type;
        public bool SearchNested;

        public SpatialObjectPropertyAttribute(SpatialObjectPropertyType type, bool searchNested)
        {
            Type = type;
            SearchNested = searchNested;
        }

        public SpatialObjectPropertyAttribute() { }
    }

}
