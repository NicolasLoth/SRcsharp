using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SRcsharp.Library.SREnums;

namespace SRcsharp.Library
{
	public class SpatialObject
	{

		#region Non-Spatial Characteristics
		private string _id = string.Empty;

		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private SpatialExistence _existence = SpatialExistence.Real;

		public SpatialExistence Existence
		{
			get { return _existence; }
			set { _existence = value; }
		}

		private ObjectCause _cause;

		public ObjectCause Cause
		{
			get { return _cause; }
			set { _cause = value; }
		}

		private string _label = string.Empty;

		public string Label
		{
			get { return _label; }
			set { _label = value; }
		}

		private string _type = string.Empty;

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		private string _superType = string.Empty;

		public string SuperType
		{
			get { return _superType; }
			set { _superType = value; }
		}

		private string _look = string.Empty;

		public string Look
		{
			get { return _look; }
			set { _look = value; }
		}

		private Dictionary<string, object>? _data = null;

		public Dictionary<string, object>? Data
		{
			get { return _data; }
			set { _data = value; }
		}

		private DateTime _created;

		public DateTime Created
		{
			get { return _created; }
			set { _created = value; }
		}

		private DateTime _updated;

		public DateTime Updated
		{
			get { return _updated; }
			set { _updated = value; }
		}

		#endregion

		#region Spatial Characteristics

		private float _width = 0.0f;

		public float Width
		{
			get { return _width; }
			set { _width = value; }
		}

		private float _height;

		public float Height
		{
			get { return _height; }
			set { _height = value; }
		}

		private float _depth;

		public float Depth
		{
			get { return _depth; }
			set { _depth = value; }
		}

		private float _angle;

		public float Angle
		{
			get { return _angle; }
			set { _angle = value; }
		}

		private bool _immobile;

		public bool Immobile
		{
			get { return _immobile; }
			set { _immobile = value; }
		}

		private SCNVector3 _velocity = new SCNVector3();

		public SCNVector3 Velocity
		{
			get { return _velocity; }
			set { _velocity = value; }
		}

		private ObjectConfidence _confidence;

		public ObjectConfidence Confidence
		{
			get { return _confidence; }
			set { _confidence = value; }
		}

		private ObjectShape _shape;

		public ObjectShape Shape
		{
			get { return _shape; }
			set { _shape = value; }
		}

		private bool _visible;

		public bool Visible
		{
			get { return _visible; }
			set { _visible = value; }
		}

		private bool _focused;

		public bool Focused
		{
			get { return _focused; }
			set { _focused = value; }
		}

		private SpatialReasoner? _context;

		public SpatialReasoner? Context
		{
			get { return _context; }
			set { _context = value; }
		}

		private SCNVector3 _position = new SCNVector3();
		public SCNVector3 Position { get { return _position; } }

		#endregion

		#region Derived Attributes

		public SCNVector3 Center
		{
			get
			{
				return _position + new SCNVector3(0.0f, _height / 2.0f, 0.0f);
			}
		}

		public double Yaw
		{
			get { return _angle * 180.0 / Math.PI; }
		}

		public float Azimuth
		{
			get
			{
				if (_context != null)
					return -(float)(Yaw + (float)Math.Truncate((Math.Atan2((float)_context.North.DY, (float)_context.North.DX) * 180.0 / Math.PI) - 90.0) % 360));
				return 0.0f;

			}
		}

		public bool Thin { get { return CalcThin() > 0; } }

        public bool Long { get { return CalcLong() > 0; } }

        public bool Equilateral { get { return CalcLong(1.1f) == 0; } }

        public bool Real { get { return _existence == SpatialExistence.Real; } }

        public bool Virtual { get { return _existence == SpatialExistence.Virtual; } }

        public bool Conceptual { get { return _existence == SpatialExistence.Conceptual; } }

        public float Perimeter { get { return (_depth+_width)*2; } }

        public float Footprint { get { return _depth * _width; } }

        public float Frontface { get { return _height * _width; } }

        public float Sideface { get { return _height * _depth; } }

        public float Surface { get { return (_height * _width + _depth * _width + _height * _depth) * 2.0f; } }

        public float Volume { get { return _depth * _width * _height; } }

        public float Radius { get { return new SCNVector3(_width / 2.0f, _depth / 2.0f, _height / 2.0f).Length; } }

        public float BaseRadius { get { return new CGPoint(_width / 2.0f, _depth / 2.0f).Length; } }

        public MotionState Motion 
		{
			get 
			{
				if (_immobile)
					return MotionState.Stationary;
				if(_confidence.Spatial > 0.5)
				{
					if (_velocity.Length > Adjustment.MaxGap)
						return MotionState.Moving;
					else
						return MotionState.Idle;
				}
				return MotionState.Unknown;
			} 
		}

        public bool Moving { get { return Motion == MotionState.Moving; } }

        public float Speed { get { return _velocity.Length; } }

        public bool Observing { get { return _cause == ObjectCause.SelfTracked; } }

        public float Length
        {
            get
            {
				var alignment = CalcLong(1.1f);
				if(alignment == 1)
					return _width;
				else if(alignment == 2)
					return _height;
				return Depth;
            }
        }

        public double Lifespan { get { return DateTime.Now.Subtract(_created).TotalSeconds; } }

        public double UpdateInterval { get { return DateTime.Now.Subtract(_updated).TotalSeconds; } }

        public SpatialAdjustment Adjustment { get { return _context!=null? _context.Adjustment: SpatialAdjustment.DefaultAdjustment; } }

		#endregion

		public static string[] BooleanAttributes = new string[] { "Immobile", "Moving", "Focused", "Visible", "Equilateral", "Thin", "Long", "Real", "Virtual", "Conceptual" };
		public static string[] NumericAttributes = new string[] { "Width", "Height", "Depth", "W", "H", "D", "Position", "X", "Y", "Z", "Angle", "Confidence" };
		public static string[] StringAttributes = new string[] { "Id", "Label", "Type", "Supertype", "Existence", "Cause", "Shape", "Look" };

		public SpatialObject(string id, SCNVector3 position, float width = 1.0f, float height = 1.0f, float depth = 1.0f, float angle = 0.0f, string label = "", float confidence = 0.0f)
		{
			_id = id;
			_position = position; 
			_width = width; 
			_height = height; 
			_depth = depth; 
			_angle = angle; 
			_label = label;
			_confidence.Spatial = confidence;
			_created = DateTime.Now;
			_updated = DateTime.Now;
		}

        public int Index()
        {
            if (_context != null && _context.Objects != null)
            {
                return _context.Objects.ToList().IndexOf(this);
            }
            return -1;
        }

        public static bool IsBoolean(string attribute)
        {
			return BooleanAttributes.Contains(attribute);
        }

        public static SpatialObject CreateDetectedObject(string id, string label = "", float width = 1.0f, float height = 1.0f, float depth = 1.0f)
        {
			var obj = new SpatialObject(id, new SCNVector3(0, 0, 0), width, height, depth)
			{
				Label = label.ToLower(),
				Type = label,
				Cause = ObjectCause.ObjectDetected,
				Existence = SpatialExistence.Real,
				Immobile = false,
				Shape = ObjectShape.Unknown
            };
			obj._confidence.Value = 0.25f;
            return obj;
        }

        public static SpatialObject CreateVirtualObject(string id, float width = 1.0f, float height = 1.0f, float depth = 1.0f)
        {
            var obj = new SpatialObject(id, new SCNVector3(0, 0, 0), width, height, depth)
            {
                Cause = ObjectCause.UserGenerated,
                Existence =SpatialExistence.Virtual,
                Immobile = false,
            };
            obj._confidence.Spatial = 1.0f;
            return obj;
        }

        public static SpatialObject CreateBuildingElement(string id, string type = "", SCNVector3 position, float width = 1.0f, float height = 1.0f, float depth = 1.0f)
        {
            var obj = new SpatialObject(id, position, width, height, depth)
            {
                Label = type.ToLower(),
                Type = type,
                SuperType = "Building Element",
                Cause = ObjectCause.PlaneDetected,
                Existence = SpatialExistence.Real,
                Immobile = true,
				Shape = ObjectShape.Cubical
            };
            obj._confidence.Value = 0.5f;
            return obj;
        }

        public static SpatialObject CreateBuildingElement(string id, string type = "", SCNVector3 from, SCNVector3 to, float height = 1.0f, float depth = 0.25f)
        {
            var midVector = new SCNVector3((to.X - from.X) / 2.0f, (to.Y - from.Y) / 2.0f, (to.Z - from.Z) / 2.0f);
            float midVectorLength = midVector.Length;
            float factor = depth / midVectorLength / 2.0f;
            var normal = new CGPoint(midVector.X * factor, midVector.Z * factor).Rotate(((float)(Math.PI / 2.0f));
            var pos = from + midVector - new SCNVector3(normal.X, 0.0f, normal.Y);
            var obj = new SpatialObject(id, pos, midVectorLength * 2.0f, height, depth)
            {
                Angle = -(float)Math.Atan2(midVector.Z, midVector.X),
                Label = type.ToLower(),
                Type = type,
                SuperType = "Building Element",
                Cause = ObjectCause.UserCaptured,
                Existence = SpatialExistence.Real,
				Immobile = true,
				Shape = ObjectShape.Cubical
            };
            obj._confidence.Value = 0.9f;
            return obj;
        }

        public static SpatialObject CreatePerson(string id, SCNVector3 position, string name = "")
        {
            var person = new SpatialObject(id, position, 0.46f, 1.72f, 0.34f)
            {
                Label = name,
                Cause = ObjectCause.SelfTracked,
                Existence = SpatialExistence.Real,
                Immobile = false,
				SuperType = "Creature",
				Type = "Person",
				Shape = ObjectShape.Changing
			};
            person._confidence.Value = 1.0f;
            return person;
        }

        // Set auxiliary data
        public void SetData(string key, object value)
        {
            if (_data == null)
            {
                _data = new Dictionary<string, object>();
            }
            _data[key] = value;
        }

        public float DataValue(string key)
        {
            if (_data != null && _data.ContainsKey(key))
            {
                var value = _data[key];
                if (value != null)
                {
                    return (float)value;
                }
				//TODO: convert to NSNumber???
                //if let val = value as? NSNumber {
                //    return val.floatValue
                //}
            }
            return 0.0f;
        }

    

		private int CalcLong(float v = 1.0f)
        {
            throw new NotImplementedException();
        }

        private int CalcThin()
		{
			throw new NotImplementedException();
		}
	}
}
