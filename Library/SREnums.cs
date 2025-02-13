using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    
    public static class SREnums
    {
        public enum NearbySchema
        {
            Fixed, // use nearbyFactor as fix nearby radius
            Circle, // use base circle radius of bbox multiplied with nearbyFactor
            Sphere, // use sphere radius of bbox multiplied with nearbyFactor
            Perimeter, // use base perimeter multiplied with nearbyFactor
            Area // use area multiplied with nearbyFactor
        }

        // Calculation schema to determine sector size for extruding bbox area
        public enum SectorSchema
        {
            Fixed, // use sectorFactor as fix sector lenght for extruding area
            Dimension, // use same dimension as object bbox multiplied with sectorFactor
            Perimeter, // use base perimeter multiplied with sectorFactor
            Area, // use area multiplied with sectorFactor
            Nearby // use nearby settings of spatial adjustment for extruding
        }

        public enum SpatialPredicatedCategories { None, Topology, Connectivity, Comparability, Similarity, Sectoriality, Visbility, Geography}

    }
}
