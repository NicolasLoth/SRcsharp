using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public class ObjectConfidence
    {

		private float _pose = 0.0f;
        [SpatialObjectProperty]
        public float Pose
		{
			get { return _pose; }
			set { _pose = value; }
		}

		private float _dimension = 0.0f;
        [SpatialObjectProperty]
        public float Dimension
		{
			get { return _dimension; }
			set { _dimension = value; }
		}

		private float _label = 0.0f;
        [SpatialObjectProperty]
        public float Label
		{
			get { return _label; }
			set { _label = value; }
		}

		private float _look = 0.0f;
        [SpatialObjectProperty]
        public float Look
		{
			get { return _look; }
			set { _look = value; }
		}

  //      [SpatialObjectProperty]
  //      public float Value { 
		//	get { return (_pose + _dimension + _label) / 3; }
		//	set { _pose = value; _dimension = value; _label = value; }
		//}

        [SpatialObjectProperty]
        public float Spatial
        {
            get { return (_pose + _dimension) / 2; }
            set { _pose = value; _dimension = value; }
        }

		public Dictionary<string,float> AsDict
		{
			get 
			{
				return new Dictionary<string, float>()
				{
					{ "pose", _pose },
					{ "dimension", _dimension },
					{ "label", _label },
					{ "look", _look }
				};
			}
		}

		public ObjectConfidence() { }


        public ObjectConfidence(float pose, float dimension)
		{
			_pose = pose;
			_dimension = dimension;

		}


	}
}
