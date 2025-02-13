﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{

    //Wrapper for renaming and extending Float-Vector2
    public class CGPoint
    {

		private Vector2 _rawValue;

		public Vector2 RawValue
		{
			get { return _rawValue; }
		}

		public float X { get { return _rawValue.X; } set { _rawValue.X = value; } }
        public float Y { get { return _rawValue.Y; } set { _rawValue.Y = value; } }

        public CGPoint()
        {
            _rawValue = new Vector2();
        }

        public CGPoint(float x, float y)
		{
			_rawValue = new Vector2(x, y);
		}

        public float Length { get { return _rawValue.Length(); } }

        public CGPoint Rotate(float radians)
        {
            return Rotate(radians, Vector2.Zero);
        }

        public CGPoint Rotate(float radians, Vector2 pivot)
        {
            var rotationSin = Math.Sin(radians);
            var rotationCos = Math.Cos(radians);
            var x = (X - pivot.X) * rotationCos - (Y - pivot.Y) * rotationSin + pivot.X;
            var y = (X - pivot.X) * rotationSin - (Y - pivot.Y) * rotationCos + pivot.Y;
            return new CGPoint((float)x, (float)y);
        }



    }
}
