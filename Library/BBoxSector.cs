﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{

    [Flags]
    public enum BBoxSectors
    {
        None = 0,
        Inside = 1 << 0,
        Ahead = 1 << 2,
        Behind = 1 << 3,
        Left = 1 << 4,
        Right = 1 << 5,
        Above = 1 << 6,
        Below = 1 << 7
    };

    // Directional 3x3x3 BBox Sector Matrix (27 object-related boundary sectors)
    //Migration comment:
    public static class BBoxSector
    { 

        public const BBoxSectors AL = BBoxSectors.Ahead | BBoxSectors.Left;
        public const BBoxSectors AR = BBoxSectors.Ahead | BBoxSectors.Right;
        public const BBoxSectors BL = BBoxSectors.Behind | BBoxSectors.Left;
        public const BBoxSectors BR = BBoxSectors.Behind | BBoxSectors.Right;
        public const BBoxSectors AO = BBoxSectors.Ahead | BBoxSectors.Above;
        public const BBoxSectors AU = BBoxSectors.Ahead | BBoxSectors.Below;
        public const BBoxSectors BO = BBoxSectors.Behind | BBoxSectors.Above;
        public const BBoxSectors BU = BBoxSectors.Behind | BBoxSectors.Below;
        public const BBoxSectors LO = BBoxSectors.Left | BBoxSectors.Above;
        public const BBoxSectors LU = BBoxSectors.Left | BBoxSectors.Below;
        public const BBoxSectors RO = BBoxSectors.Right | BBoxSectors.Above;
        public const BBoxSectors RU = BBoxSectors.Right | BBoxSectors.Below;
        public const BBoxSectors ALO = BBoxSectors.Ahead | BBoxSectors.Left | BBoxSectors.Above;
        public const BBoxSectors ARO = BBoxSectors.Ahead | BBoxSectors.Right | BBoxSectors.Above;
        public const BBoxSectors BLO = BBoxSectors.Behind | BBoxSectors.Left | BBoxSectors.Above;
        public const BBoxSectors BRO = BBoxSectors.Behind | BBoxSectors.Right | BBoxSectors.Above;
        public const BBoxSectors ALU = BBoxSectors.Ahead | BBoxSectors.Left | BBoxSectors.Below;
        public const BBoxSectors ARU = BBoxSectors.Ahead | BBoxSectors.Right | BBoxSectors.Below;
        public const BBoxSectors BLU = BBoxSectors.Behind | BBoxSectors.Left | BBoxSectors.Below;
        public const BBoxSectors BRU = BBoxSectors.Ahead | BBoxSectors.Right | BBoxSectors.Below;


        public static int GetDivergencies(this BBoxSectors bboxSectors)
        {
            if (bboxSectors.HasFlag(BBoxSectors.Inside))
                return 0;
            return BitOperations.PopCount((ulong)bboxSectors);
        }

        public static string ToString(this BBoxSectors bboxSectors)
        {
            return string.Concat(bboxSectors.GetFlags().Select(bbox => bbox.ToString().First()));
        }

        public static string GetDescription(this BBoxSectors bboxSectors)
        {
            var desc = bboxSectors.ToString();
            if (string.IsNullOrEmpty(desc))
                desc = "no sector";

            return desc;
        }
        
    }
}
