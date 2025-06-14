﻿
using System.Numerics;
using System;

namespace SRcsharp.Library
{
    public struct CGVector
    {


        private Vector2 _rawValue;
        public Vector2 RawValue
        {
            get { return _rawValue; }
        }

        public float DX { get { return _rawValue.X; } set { _rawValue.X = value; } }
        public float DY { get { return _rawValue.Y; } set { _rawValue.Y = value; } }


        public CGVector(float x, float y)
        {
            _rawValue = new Vector2(x, y);
        }

        public CGVector(Vector2 vec)
        {
            _rawValue = vec;
        }


    }
}
