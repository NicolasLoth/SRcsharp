using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public struct ObjectConfidence
    {

		private float _pose = 0.0f;

		public float Pose
		{
			get { return _pose; }
			set { _pose = value; }
		}

		private float _dimension = 0.0f;

		public float Dimension
		{
			get { return _dimension; }
			set { _dimension = value; }
		}

		public ObjectConfidence(float pose, float dimension)
		{
			_pose = pose;
			_dimension = dimension;

		}


	}
}
