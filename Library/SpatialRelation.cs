using System;

namespace SRcsharp.Library
{
    public struct SpatialRelation
    {

		private SpatialObject _subject;

		public SpatialObject Subject
		{
			get { return _subject; }
			set { _subject = value; }
		}

		private SpatialPredicate _predicate;

		public SpatialPredicate Predicate
		{
			get { return _predicate; }
			set { _predicate = value; }
		}

		private SpatialObject _object;

		public SpatialObject Object
		{
			get { return _object; }
			set { _object = value; }
		}

		private float _delta ;

		public float Delta
		{
			get { return _delta; }
			set { _delta = value; }
		}

		private double _angle;

		public double Angle
		{
			get { return _angle; }
			set { _angle = value; }
		}


        public double Yaw
        {
            get { return _angle * 180.0 / Math.PI; }
            set { _angle = value * Math.PI / 180.0f; }
        }

        public string SubjectId { get { return _subject.Id; } }

        public string ObjectId { get { return _object.Id; } }


        public SpatialRelation(SpatialObject subject, SpatialPredicate pred, SpatialObject obj, float delta = 0.0f, float angle = 0.0f)
		{
			_subject = subject;
			_predicate = pred;
			_object = obj;
			_delta= delta;
			_angle = angle;
		}

		public string Description
        {
            get 
			{
				var str = _subject.Label;

				if(!string.IsNullOrEmpty(_subject.Label))
				{
					str = _subject.Label;
				}
				else if (!string.IsNullOrEmpty(_subject.Type)) 
				{
					str = _subject.Type;
				}


				string.Concat(str," " + SpatialTerms.Instance.GetTermWithVerbAndPreposition(_predicate) + " ");


				if(!string.IsNullOrEmpty(_object.Label)) {
					str = str + _object.Label;
                }
				else if(!string.IsNullOrEmpty(_object.Type)) {
					str = str + _object.Type;
				}
				else
				{
					str = str + _object.Id;
				}
				str = str + string.Format("{0}  𝛥:{1:F1}  𝜶:{2:F1}°)", _predicate.ToString(), _delta, Yaw);
				return str;


			} 
		}

        public override string ToString()
        {
			return string.Format("{0} {1} {2} | {3:F1} {4:F1}", _subject.Id, _predicate, _object.Id, _delta, Yaw);
        }

    }
}
