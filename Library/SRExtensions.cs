using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SRcsharp.Library.SREnums;

namespace SRcsharp.Library
{
    public static class SRExtensions
    {
        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

        public static SCNVector3 ToSNCVector3(this Vector3 vector)
        {
            return new SCNVector3(vector);
        }


    }
}
