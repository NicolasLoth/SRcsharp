using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

		private float _delta = 0.0f;

		public float Delta
		{
			get { return _delta; }
			set { _delta = value; }
		}

		private double _angle = 0.0f;

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

		public string Desc
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

				return str;

				//str.Concat(" " + SpatialTerms.termWithVerbAndPreposition(_predicate) + "");

				////TO migrate:
				//		str = str + " " + SpatialTerms.termWithVerbAndPreposition(predicate) + " "
				//if !object.label.isEmpty {
				//			str = str + object.label
				//}
				//		else if !object.type.isEmpty {
				//			str = str + object.type
				//}
				//		else
				//		{
				//			str = str + object.id
				//}
				//		str = str + String(format: " (\(predicate.rawValue) 𝛥:%.2f  𝜶:%.1f°)", delta, yaw)
				//return str

            } 
		}

    }
}
