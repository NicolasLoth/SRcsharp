using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static SRcsharp.Library.SREnums;

namespace SRcsharp.Library
{
    public class SpatialObject
    {

        #region Non-Spatial Characteristics
        private string _id = string.Empty;
        [SpatialObjectProperty]
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private SpatialExistence _existence = SpatialExistence.Real;
        [SpatialObjectProperty]
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
        [SpatialObjectProperty]
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        private string _type = string.Empty;
        [SpatialObjectProperty]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _supertype = string.Empty;
        [SpatialObjectProperty]
        public string Supertype
        {
            get { return _supertype; }
            set { _supertype = value; }
        }

        private string _look = string.Empty;
        [SpatialObjectProperty]
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
        [SpatialObjectProperty]
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private float _height;
        [SpatialObjectProperty]
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private float _depth;
        [SpatialObjectProperty]
        public float Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        private float _angle;
        [SpatialObjectProperty]
        public float Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        private bool _immobile;
        [SpatialObjectProperty]
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
        [SpatialObjectProperty]
        public ObjectConfidence Confidence
        {
            get { return _confidence; }
            set { _confidence = value; }
        }

        private ObjectShape _shape;
        [SpatialObjectProperty]
        public ObjectShape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        private bool _visible;
        [SpatialObjectProperty]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        private bool _focused;
        [SpatialObjectProperty]
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
        [SpatialObjectProperty]
        public SCNVector3 Position {
            get { return _position; }
            set
            {
                if (UpdateInterval > 0.003 && !_immobile)
                    _velocity = (value - _position) / (float)UpdateInterval;
                _position = value;
            }
        }

        #endregion

        #region Derived Attributes
        [SpatialObjectProperty]
        public SCNVector3 Center
        {
            get
            {
                return _position + new SCNVector3(0.0f, _height / 2.0f, 0.0f);
            }
            set
            {
                Position = new SCNVector3(value.X, value.Y - (_height / 2), value.Z);
            }
        }

        [SpatialObjectProperty]
        public double Yaw
        {
            get { return _angle * 180.0 / Math.PI; }
            set { _angle = (float)(value * Math.PI / 180f); }
        }
        [SpatialObjectProperty]
        public float Azimuth
        {
            get
            {
                if (_context != null)
                    return -(float)(Yaw + (float)Math.Truncate((Math.Atan2((float)_context.North.DY, (float)_context.North.DX) * 180.0 / Math.PI) - 90.0) % 360);
                return 0.0f;

            }
        }
        [SpatialObjectProperty]
        public bool Thin { get { return CalcThinAxis(null) > 0; } }
        [SpatialObjectProperty]
        public bool Long { get { return CalcLongAxis(null) > 0; } }
        [SpatialObjectProperty]
        public bool Equilateral { get { return CalcLongAxis(1.1f) == 0; } }
        [SpatialObjectProperty]
        public bool Real { get { return _existence == SpatialExistence.Real; } }
        [SpatialObjectProperty]
        public bool Virtual { get { return _existence == SpatialExistence.Virtual; } }
        [SpatialObjectProperty]
        public bool Conceptual { get { return _existence == SpatialExistence.Conceptual; } }
        [SpatialObjectProperty]
        public float Perimeter { get { return (_depth + _width) * 2; } }
        [SpatialObjectProperty]
        public float Footprint { get { return _depth * _width; } }
        [SpatialObjectProperty]
        public float Frontface { get { return _height * _width; } }
        [SpatialObjectProperty]
        public float Sideface { get { return _height * _depth; } }
        [SpatialObjectProperty]
        public float Surface { get { return (_height * _width + _depth * _width + _height * _depth) * 2.0f; } }
        [SpatialObjectProperty]
        public float Volume { get { return _depth * _width * _height; } }
        [SpatialObjectProperty]
        public float Radius { get { return new SCNVector3(_width / 2.0f, _depth / 2.0f, _height / 2.0f).Length; } }
        [SpatialObjectProperty]
        public float BaseRadius { get { return new CGPoint(_width / 2.0f, _depth / 2.0f).Length; } }

        public MotionState Motion
        {
            get
            {
                if (_immobile)
                    return MotionState.Stationary;
                if (_confidence.Spatial > 0.5)
                {
                    if (_velocity.Length > Adjustment.MaxGap)
                        return MotionState.Moving;
                    else
                        return MotionState.Idle;
                }
                return MotionState.Unknown;
            }
        }
        [SpatialObjectProperty]
        public bool Moving { get { return Motion == MotionState.Moving; } }
        [SpatialObjectProperty]
        public float Speed { get { return _velocity.Length; } }
        [SpatialObjectProperty]
        public bool Observing { get { return _cause == ObjectCause.SelfTracked; } }
        [SpatialObjectProperty]
        public float Length
        {
            get
            {
                var alignment = CalcLongAxis(1.1f);
                if (alignment == 1)
                    return _width;
                else if (alignment == 2)
                    return _height;
                return Depth;
            }
        }

        public int MainDirection { get { return CalcLongAxis(null); } }

        [SpatialObjectProperty]
        public double Lifespan { get { return DateTime.Now.Subtract(_created).TotalSeconds; } }
        [SpatialObjectProperty]
        public double UpdateInterval { get { return DateTime.Now.Subtract(_updated).TotalSeconds; } }

        public SpatialAdjustment Adjustment { get { return _context != null ? _context.Adjustment : SpatialAdjustment.DefaultAdjustment; } }

        #endregion

        //public static string[] BooleanAttributesArray = new string[] { "Immobile", "Moving", "Focused", "Visible", "Equilateral", "Thin", "Long", "Real", "Virtual", "Conceptual" };
        //public static string[] NumericAttributesArray = new string[] { "Width", "Height", "Depth", "W", "H", "D", "Position", "X", "Y", "Z", "Angle", "Confidence" };
        //public static string[] StringAttributesArray = new string[] { "Id", "Label", "Type", "Supertype", "Existence", "Cause", "Shape", "Look" };

        public static Dictionary<string, PropertyInfo> BooleanAttributes;
        public static Dictionary<string, PropertyInfo> NumericAttributes;
        public static Dictionary<string, PropertyInfo> StringAttributes;

        private static Dictionary<string, PropertyInfo> LoadAttributes(Type[] types)
        {
            var props = typeof(SpatialObject).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SpatialObjectPropertyAttribute)) && types.Contains(prop.PropertyType)).
                ToDictionary(prop => prop.Name);
            return props;
        }

        static SpatialObject()
        {
            BooleanAttributes = LoadAttributes(new Type[] { typeof(bool) });
            NumericAttributes = LoadAttributes(new Type[] { typeof(float), typeof(double), typeof(decimal), typeof(byte), typeof(short), typeof(int), typeof(long) });
            StringAttributes = LoadAttributes(new Type[] { typeof(string), typeof(Enum) });
        }

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
            return BooleanAttributes.ContainsKey(attribute);
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
                Existence = SpatialExistence.Virtual,
                Immobile = false,
            };
            obj._confidence.Spatial = 1.0f;
            return obj;
        }

        public static SpatialObject CreateBuildingElement(string id, SCNVector3 position, string type = "", float width = 1.0f, float height = 1.0f, float depth = 1.0f)
        {
            var obj = new SpatialObject(id, position, width, height, depth)
            {
                Label = type.ToLower(),
                Type = type,
                Supertype = "Building Element",
                Cause = ObjectCause.PlaneDetected,
                Existence = SpatialExistence.Real,
                Immobile = true,
                Shape = ObjectShape.Cubical
            };
            obj._confidence.Value = 0.5f;
            return obj;
        }

        public static SpatialObject CreateBuildingElement(string id, SCNVector3 from, SCNVector3 to, string type = "", float height = 1.0f, float depth = 0.25f)
        {
            var midVector = new SCNVector3((to.X - from.X) / 2.0f, (to.Y - from.Y) / 2.0f, (to.Z - from.Z) / 2.0f);
            float midVectorLength = midVector.Length;
            float factor = depth / midVectorLength / 2.0f;
            var normal = new CGPoint(midVector.X * factor, midVector.Z * factor).Rotate(((float)(Math.PI / 2.0f)));
            var pos = from + midVector - new SCNVector3(normal.X, 0.0f, normal.Y);
            var obj = new SpatialObject(id, pos, midVectorLength * 2.0f, height, depth)
            {
                Angle = -(float)Math.Atan2(midVector.Z, midVector.X),
                Label = type.ToLower(),
                Type = type,
                Supertype = "Building Element",
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
                Supertype = "Creature",
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

        public string Describe()
        {
            var res = string.Empty;

            if (!string.IsNullOrEmpty(_label) && _label != _id)
            {
                res += "(" + _label + "), ";
            }
            if (!string.IsNullOrEmpty(_type))
            {
                res += "(" + _type + "), ";
            }
            if (!string.IsNullOrEmpty(_supertype))
            {
                res += "(" + _supertype + "), ";
            }

            res += string.Format("\n{0:F2} {1:F2} {2:F2}\n", _position.X, _position.Y, _position.Z);
            res += string.Format("\n{0:F2} {1:F2} {2:F2}\n", _width, _depth, _height);
            res += string.Format("\n𝜶{0:F2}°} \n", Yaw);
            return res;

        }

        public void RotShift(float rad, float dx, float dy = 0.0f, float dz = 0.0f)
        {
            var rotsin = Math.Sin(rad);
            var rotcos = Math.Cos(rad);
            var rx = dx * rotcos - dz * rotsin;
            var rz = dx * rotsin + dz * rotcos;
            _position += new SCNVector3((float)rx, dy, (float)rz);
        }

        private int CalcLongAxis(float? ratio)
        {
            if (!ratio.HasValue)
                ratio = SpatialAdjustment.DefaultAdjustment.LongRatio;

            float[] values = { _width, _height, _depth };

            var max = values.Max();
            var min = values.Min();

            if (max > 0.0f && max >= min * ratio)
            {
                if (_width < max)
                {
                    if (_height < max)
                        return 3;
                    else
                        return 2;
                }
                else
                {
                    return 1;
                }
            }
            return 0;
        }

        private int CalcThinAxis(float? ratio)
        {
            if (!ratio.HasValue)
                ratio = SpatialAdjustment.DefaultAdjustment.LongRatio;

            float[] values = { _width, _height, _depth };

            var max = values.Max();
            var min = values.Min();

            if (max > 0.0f && max >= min * ratio)
            {
                if (_width < max)
                {
                    if (_height < max)
                        return 3;
                    else
                        return 2;
                }
                else
                {
                    return 1;
                }
            }
            return 0;
        }

        public SCNVector3[] CalcLowerPoints(bool local = false)
        {
            var p0 = new CGPoint(_width / 2.0f, _depth / 2.0f);
            var p1 = new CGPoint(-_width / 2.0f, _depth / 2.0f);
            var p2 = new CGPoint(-_width / 2.0f, -_depth / 2.0f);
            var p3 = new CGPoint(_width / 2.0f, -_depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1, p2, p3 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            var res = points.Select(p => new SCNVector3(p.X + vector.X, vector.Y, p.Y + vector.Z));
            return res.ToArray();
        }

        public SCNVector3[] CalcUpperPoints(bool local = false)
        {
            var p0 = new CGPoint(_width / 2.0f, _depth / 2.0f);
            var p1 = new CGPoint(-_width / 2.0f, _depth / 2.0f);
            var p2 = new CGPoint(-_width / 2.0f, -_depth / 2.0f);
            var p3 = new CGPoint(_width / 2.0f, -_depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1, p2, p3 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            var res = points.Select(p => new SCNVector3(p.X + vector.X, vector.Y + _height, p.Y + vector.Z));
            return res.ToArray();
        }

        public SCNVector3[] CalcFrontPoints(bool local = false)
        {
            var p0 = new CGPoint(_width / 2.0f, _depth / 2.0f);
            var p1 = new CGPoint(-_width / 2.0f, _depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            SCNVector3[] res = new SCNVector3[4];
            res[0] = new SCNVector3(p0.X + vector.X, vector.Y, p0.Y + vector.Z);
            res[1] = new SCNVector3(p1.X + vector.X, vector.Y, p1.Y + vector.Z);
            res[2] = new SCNVector3(p1.X + vector.X, vector.Y + _height, p1.Y + vector.Z);
            res[3] = new SCNVector3(p0.X + vector.X, vector.Y + _height, p0.Y + vector.Z);
            return res;
        }

        public SCNVector3[] CalcBackPoints(bool local = false)
        {
            var p0 = new CGPoint(_width / 2.0f, -_depth / 2.0f);
            var p1 = new CGPoint(-_width / 2.0f, -_depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            SCNVector3[] res = new SCNVector3[4];
            res[0] = new SCNVector3(p0.X + vector.X, vector.Y, p0.Y + vector.Z);
            res[1] = new SCNVector3(p1.X + vector.X, vector.Y, p1.Y + vector.Z);
            res[2] = new SCNVector3(p1.X + vector.X, vector.Y + _height, p1.Y + vector.Z);
            res[3] = new SCNVector3(p0.X + vector.X, vector.Y + _height, p0.Y + vector.Z);
            return res;
        }

        public SCNVector3[] CalcRightPoints(bool local = false)
        {
            var p0 = new CGPoint(-_width / 2.0f, _depth / 2.0f);
            var p1 = new CGPoint(-_width / 2.0f, -_depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            SCNVector3[] res = new SCNVector3[4];
            res[0] = new SCNVector3(p0.X + vector.X, vector.Y, p0.Y + vector.Z);
            res[1] = new SCNVector3(p1.X + vector.X, vector.Y, p1.Y + vector.Z);
            res[2] = new SCNVector3(p1.X + vector.X, vector.Y + _height, p1.Y + vector.Z);
            res[3] = new SCNVector3(p0.X + vector.X, vector.Y + _height, p0.Y + vector.Z);
            return res;
        }

        public SCNVector3[] CalcLeftPoints(bool local = false)
        {
            var p0 = new CGPoint(_width / 2.0f, _depth / 2.0f);
            var p1 = new CGPoint(_width / 2.0f, -_depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            SCNVector3[] res = new SCNVector3[4];
            res[0] = new SCNVector3(p0.X + vector.X, vector.Y, p0.Y + vector.Z);
            res[1] = new SCNVector3(p1.X + vector.X, vector.Y, p1.Y + vector.Z);
            res[2] = new SCNVector3(p1.X + vector.X, vector.Y + _height, p1.Y + vector.Z);
            res[3] = new SCNVector3(p0.X + vector.X, vector.Y + _height, p0.Y + vector.Z);
            return res;
        }

        public SCNVector3[] CalcPoints(bool local = false)
        {
            var p0 = new CGPoint(_width / 2.0f, _depth / 2.0f);
            var p1 = new CGPoint(-_width / 2.0f, _depth / 2.0f);
            var p2 = new CGPoint(-_width / 2.0f, -_depth / 2.0f);
            var p3 = new CGPoint(_width / 2.0f, -_depth / 2.0f);

            var vector = new SCNVector3();
            var points = new List<CGPoint>() { p0, p1, p2, p3 };
            if (!local)
            {
                points.ForEach(p => p.Rotate(-_angle));
                vector = _position;
            }
            SCNVector3[] res = new SCNVector3[8];
            res[0] = new SCNVector3(p0.X + vector.X, vector.Y, p0.Y + vector.Z);
            res[1] = new SCNVector3(p1.X + vector.X, vector.Y, p1.Y + vector.Z);
            res[2] = new SCNVector3(p2.X + vector.X, vector.Y, p2.Y + vector.Z);
            res[3] = new SCNVector3(p3.X + vector.X, vector.Y, p3.Y + vector.Z);
            res[4] = new SCNVector3(p0.X + vector.X, vector.Y + _height, p0.Y + vector.Z);
            res[5] = new SCNVector3(p1.X + vector.X, vector.Y + _height, p1.Y + vector.Z);
            res[6] = new SCNVector3(p2.X + vector.X, vector.Y + _height, p2.Y + vector.Z);
            res[7] = new SCNVector3(p3.X + vector.X, vector.Y + _height, p3.Y + vector.Z);
            return res;
        }

        public float CalcDistance(SCNVector3 to)
        {
            return (to - Center).Length;
        }

        public float CalcBaseDistance(SCNVector3 to)
        {
            to.Y = _position.Y;
            return (to - _position).Length;
        }

        public SCNVector3 ConvertIntoLocal(SCNVector3 pt)
        {
            var vx = pt.X - _position.X;
            var vz = pt.Z - _position.Z;
            var rotsin = Math.Sin(_angle);
            var rotcos = Math.Cos(_angle);
            var x = vx * rotcos - vz * rotsin;
            var z = vx * rotsin + vz * rotcos;
            return new SCNVector3((float)x, pt.Y - _position.Y, (float)z);
        }

        public SCNVector3[] ConvertIntoLocal(SCNVector3[] pts)
        {
            var ptsList = pts.ToList();
            ptsList.ForEach(pt => ConvertIntoLocal(pt));
            return ptsList.ToArray();
        }

        public SCNVector3 Rotate(SCNVector3 pt, float angle)
        {
            var rotsin = Math.Sin(_angle);
            var rotcos = Math.Cos(_angle);
            var x = pt.X * rotcos - pt.Z * rotsin;
            var z = pt.X * rotsin + pt.Z * rotcos;
            return new SCNVector3((float)x, pt.Y, (float)z);
        }

        public SCNVector3[] Rotate(SCNVector3[] pts, float angle)
        {
            var ptsList = pts.ToList();
            ptsList.ForEach(pt => Rotate(pt, angle));
            return ptsList.ToArray();
        }

        public BBoxSectors IsSectorOf(SCNVector3 point, bool nearBy = false, float epsilon = -100.0f)
        {
            var zone = BBoxSectors.None;

            if (nearBy)
            {
                var pt = point;
                pt.Y = pt.Y - (float)(_height / 2.0);
                float distance = pt.Length;
                if (distance > CalcNearbyRadius())
                {
                    return zone;
                }
            }

            float delta = epsilon > -99.0f ? epsilon : (float)(_context?.Adjustment.MaxGap ?? SpatialAdjustment.DefaultAdjustment.MaxGap);
            if (point.X <= _width / 2.0f + delta && -point.X <= _width / 2.0f + delta &&
            point.Z <= _depth / 2.0f + delta && -point.Z <= _depth / 2.0f + delta &&
                point.Y <= _height + delta && point.Y >= -delta)
            {
                zone |= BBoxSectors.Inside;
                return zone;
            }

            if (point.X + delta > _width / 2.0f)
            {
                zone |= BBoxSectors.Left;
            }
            else if (-point.X + delta > _width / 2.0f)
            {
                zone |= BBoxSectors.Right;
            }

            if (point.Z + delta > _depth / 2.0f)
            {
                zone |= BBoxSectors.Ahead;
            }
            else if (-point.Z + delta > _depth / 2.0f)
            {
                zone |= BBoxSectors.Behind;
            }

            if (point.Y + delta > _height)
            {
                zone |= BBoxSectors.Over;
            }
            else if (point.Y - delta < 0.0f)
            {
                zone |= BBoxSectors.Under;
            }

            return zone;
        }

        public float CalcNearbyRadius()
        {
            switch(Adjustment.NearbySchema)
            {
                case NearbySchema.Fixed:
                    return Adjustment.NearbyFactor;
                case NearbySchema.Circle:
                    return Math.Min(BaseRadius * Adjustment.NearbyFactor, Adjustment.NearbyLimit);
                case NearbySchema.Sphere:
                    return Math.Min(Radius * Adjustment.NearbyFactor, Adjustment.NearbyLimit);
                case NearbySchema.Perimeter:
                    return Math.Min((_height + _width) * Adjustment.NearbyFactor, Adjustment.NearbyLimit);
                case NearbySchema.Area:
                    return Math.Min(_height * _width * Adjustment.NearbyFactor, Adjustment.NearbyLimit);
            }
            return 0.0f;
        }

        public SCNVector3 SectorLengths(BBoxSectors sector = BBoxSectors.Inside)
        {
            SCNVector3 result = new SCNVector3(_width, _depth, _height);

            if (sector.HasFlag(BBoxSectors.Ahead) || sector.HasFlag(BBoxSectors.Behind))
            {
                switch (Adjustment.SectorSchema)
                {
                    case SectorSchema.Fixed:
                        result.Z = Adjustment.SectorFactor;
                        break;
                    case SectorSchema.Area:
                        result.Z = Math.Min(_height * _width * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Dimension:
                        result.Z = Math.Min(_depth * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Perimeter:
                        result.Z = Math.Min(_height + _width * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Nearby:
                        result.Z = Math.Min(CalcNearbyRadius(), Adjustment.SectorLimit);
                        break;
                }
            }

            if (sector.HasFlag(BBoxSectors.Left) || sector.HasFlag(BBoxSectors.Right))
            {
                switch (Adjustment.SectorSchema)
                {
                    case SectorSchema.Fixed:
                        result.X = Adjustment.SectorFactor;
                        break;
                    case SectorSchema.Area:
                        result.X = Math.Min(_height * _depth * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Dimension:
                        result.X = Math.Min(_width * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Perimeter:
                        result.X = Math.Min(_height + _depth * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Nearby:
                        result.X = Math.Min(CalcNearbyRadius(), Adjustment.SectorLimit);
                        break;
                }
            }

            if (sector.HasFlag(BBoxSectors.Over) || sector.HasFlag(BBoxSectors.Under))
            {
                switch (Adjustment.SectorSchema)
                {
                    case SectorSchema.Fixed:
                        result.Y = Adjustment.SectorFactor;
                        break;
                    case SectorSchema.Area:
                        result.Y = Math.Min(_width * _depth * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Dimension:
                        result.Y = Math.Min(_height * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Perimeter:
                        result.Y = Math.Min(_width + _depth * Adjustment.SectorFactor, Adjustment.SectorLimit);
                        break;
                    case SectorSchema.Nearby:
                        result.Y = Math.Min(CalcNearbyRadius(), Adjustment.SectorLimit);
                        break;
                }
            }

            return result;
        }

    }

}
