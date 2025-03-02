using System.Linq;
using System.Numerics;
using System;

namespace SRcsharp.Library
{
    public struct SCNVector3
    {


        private Vector3 _rawValue;
        public Vector3 RawValue
        {
            get { return _rawValue; }
        }

        public float X { get { return _rawValue.X; } set { _rawValue.X = value; } }
        public float Y { get { return _rawValue.Y; } set { _rawValue.Y = value; } }
        public float Z { get { return _rawValue.Z; } set { _rawValue.Z = value; } } 


        //public SCNVector3()
        //{
        //    _rawValue = new Vector3();
        //}

        public SCNVector3(float x, float y, float z)
        {
            _rawValue = new Vector3(x, y, z);
        }

        public SCNVector3(Vector3 vec)
        {
            _rawValue = vec;
        }

        public float Length { get { return _rawValue.Length(); } }

        public static SCNVector3 Zero { get { var vec = new SCNVector3(0, 0, 0); return vec; } }

        public SCNVector3[] Nearest(SCNVector3[] pts)
        {
            return pts.OrderBy(scnVec => scnVec.Length).ToArray();
        }

        public static SCNVector3 operator +(SCNVector3 left, SCNVector3 right)
        {
            return new SCNVector3(left.RawValue + right.RawValue);
        }

        public static SCNVector3 operator -(SCNVector3 left, SCNVector3 right)
        {
            return new SCNVector3(left.RawValue - right.RawValue);
        }
        public static SCNVector3 operator *(SCNVector3 left, SCNVector3 right)
        {
            return new SCNVector3(left.RawValue * right.RawValue);
        }

        public static SCNVector3 operator /(SCNVector3 left, SCNVector3 right)
        {
            return new SCNVector3(left.RawValue / right.RawValue);
        }

        public static SCNVector3 operator /(SCNVector3 left, float scalar)
        {
            return new SCNVector3(left.RawValue / scalar);
        }


        public SCNVector3 ConvertIntoLocal(SCNVector3 position, float angle)
        {
            var vx = X - position.X;
            var vz = Z - position.Z;
            var rotsin = Math.Sin(angle);
            var rotcos = Math.Cos(angle);
            var x = vx * rotcos - vz * rotsin;
            var z = vx * rotsin + vz * rotcos;

            return new SCNVector3((float)x, Y - position.Y, (float)z);
        }

        public SCNVector3 Rotate(float angle)
        {
            var rotsin = Math.Sin(angle);
            var rotcos = Math.Cos(angle);
            var x = X * rotcos - Z * rotsin;
            var z = X * rotsin + Z * rotcos;

            return new SCNVector3((float)x, Y, (float)z);
        }
    }
}
