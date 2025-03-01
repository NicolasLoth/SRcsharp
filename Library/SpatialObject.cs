using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using static SRcsharp.Library.SpatialPredicate;
using static SRcsharp.Library.SREnums;
using System.Reflection.Metadata;

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

        private string _label;
        [SpatialObjectProperty]
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        private string _type;
        [SpatialObjectProperty]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _supertype;
        [SpatialObjectProperty]
        public string Supertype
        {
            get { return _supertype; }
            set { _supertype = value; }
        }

        private string _look;
        [SpatialObjectProperty]
        public string Look
        {
            get { return _look; }
            set { _look = value; }
        }

        private Dictionary<string, object> _data = null;

        public Dictionary<string, object> Data
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
        [SpatialObjectProperty(SpatialObjectPropertyType.DeclaredSpatial, true)]
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
                    return -(float)(Yaw + (float)(Math.Atan2((float)_context.North.DY, (float)_context.North.DX) * 180.0 / Math.PI) - 90.0) % 360.0f;
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

        public SpatialAdjustment Adjustment 
        { 
            get 
            { 
                return _context != null ? _context.Adjustment : SpatialAdjustment.DefaultAdjustment; 
            } 
        }

        #endregion

        public static Dictionary<string, PropertyInfo> AllAttributes;
        public static Dictionary<string, List<PropertyInfo>> NestedAttributes;
        public static Dictionary<string, PropertyInfo> BooleanAttributes;
        public static Dictionary<string, PropertyInfo> NumericAttributes;
        public static Dictionary<string, PropertyInfo> StringAttributes;

        private static Dictionary<string, PropertyInfo> LoadAttributes(Type[] types)
        {
            var res = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);

            var props = typeof(SpatialObject).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SpatialObjectPropertyAttribute)) && types.Contains(prop.PropertyType)).
                ToDictionary(prop => prop.Name);
            props.ToList().ForEach(kvp => res.Add(kvp.Key, kvp.Value));

            var objProps = typeof(SpatialObject).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SpatialObjectPropertyAttribute)) && prop.GetCustomAttribute<SpatialObjectPropertyAttribute>().SearchNested);
            foreach(var pi in objProps)
            {
                var adds = pi.PropertyType.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SpatialObjectPropertyAttribute)) && types.Contains(prop.PropertyType)).
                ToDictionary(prop => pi.Name+"."+prop.Name);
                adds.ToList().ForEach(x =>
                {
                    res.Add(x.Key, x.Value);
                    NestedAttributes.Add(x.Key, new List<PropertyInfo>() { pi, x.Value });
                });
            }

            return res;
        }

        public object GetAttributeValue(string property)
        {
            if (!AllAttributes.ContainsKey(property)) { return null; }

            object obj = null;
            if (SpatialObject.NestedAttributes.ContainsKey(property))
            {
                obj = this;
                foreach (var pi in SpatialObject.NestedAttributes[property])
                {
                    obj = pi.GetValue(obj);
                }
            }
            else
            {
                obj = AllAttributes[property].GetValue(this);
            }
            return obj;
        }

        static SpatialObject()
        {
            NestedAttributes = new Dictionary<string, List<PropertyInfo>>(StringComparer.InvariantCultureIgnoreCase);
            BooleanAttributes = LoadAttributes(new Type[] { typeof(bool) });
            NumericAttributes = LoadAttributes(new Type[] { typeof(float), typeof(double), typeof(decimal), typeof(byte), typeof(short), typeof(int), typeof(long) });
            StringAttributes = LoadAttributes(new Type[] { typeof(string), typeof(Enum) });
            AllAttributes = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            BooleanAttributes.ToList().ForEach(kvp => AllAttributes.Add(kvp.Key, kvp.Value));
            NumericAttributes.ToList().ForEach(kvp => AllAttributes.Add(kvp.Key, kvp.Value));
            StringAttributes.ToList().ForEach(kvp => AllAttributes.Add(kvp.Key, kvp.Value));
        }

        public SpatialObject(string id, float x=0.0f, float y=0.0f, float z=0.0f, float width = 1.0f, float height = 1.0f, float depth = 1.0f, float angle = 0.0f, string label = "", float confidence = 0.0f)
            : this(id, new SCNVector3(x, y, z), width, height, depth, angle, label, confidence)
        { 
            
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
            _confidence = new ObjectConfidence();
            _confidence.Spatial = confidence;
            _created = DateTime.Now;
            _updated = DateTime.Now;
        }

        public SpatialObject(string id) :
            this(id, SCNVector3.Zero, 1.0f, 1.0f, 1.0f, 0.0f, "", 0.0f)
        {

        }

        public int Index()
        {
            if (_context != null && _context.Objects != null)
            {
                return _context.Objects.ToList().IndexOf(this);
            }
            return -1;
        }


        public static PropertyInfo GetPropertyByName(string attribute)
        {
            if (AllAttributes.ContainsKey(attribute))
                return AllAttributes[attribute];

            return null;
        }


        public object GetPropertyValue(string attribute)
        {
            if (AllAttributes.ContainsKey(attribute))
                return AllAttributes[attribute].GetValue(this);
            return null;
        }

        public bool SetPropertyByName(string attribute,object value)
        {
            if (AllAttributes.ContainsKey(attribute))
            {
                AllAttributes[attribute].SetValue(this, value);
                return true;
            }
            return false;
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

        //public float DataValue(string key)
        //{
        //    if (_data != null && _data.ContainsKey(key))
        //    {
        //        var value = _data[key];
        //        if (value != null)
        //        {
        //            return (float)value;
        //        }
        //        //TODO: convert to NSNumber???
        //        //if let val = value as? NSNumber {
        //        //    return val.floatValue
        //        //}
        //    }
        //    return 0.0f;
        //}

        public Dictionary<string,object> AsDict()
        {
            var res = new Dictionary<string, object>();
            AddAsDict(res, AllAttributes);
            //AddAsDict(res, NumericAttributes);
            //AddAsDict(res, StringAttributes);

            if (_data != null)
            {
                _data.ToList().ForEach(x => res.Add(x.Key, x.Value));
            } 

            return res;
        }

        private void AddAsDict(Dictionary<string,object> values, Dictionary<string,PropertyInfo> properties)
        {
            foreach (var kvp in properties)
            {
                object obj = this;
                string pre = "";
                if (kvp.Key.Contains("."))
                {
                    pre = kvp.Key.Split(".").First();
                    var prop = typeof(SpatialObject).GetProperties().Where(prop => prop.Name == pre).First();
                    pre += ".";
                    obj = prop.GetValue(this);
                }

                values.Add(pre+kvp.Value.Name, kvp.Value.GetValue(obj));
            }
        }

        public Dictionary<string, object> ToAny()
        {
            throw new NotImplementedException();
        }

        public void FromAny(Dictionary<string, object> input)
        {
            string id = input.ContainsKey("Id") ? input["Id"] as string : "";
            if (!string.IsNullOrEmpty(id))
            {
                if (this.Id != id)
                {
                    Console.WriteLine("import/update from another id!");
                }
                this.Id = id;
            }

            float? number = 0.0f;
            SCNVector3 pos = new SCNVector3();

            //if (_position == null)
            //    _position = new SCNVector3();

            var list = input.ContainsKey("Position") ? input["Position"] as List<float> : null;
            if (list != null && list.Count == 3)
            {
                pos.X = (float)list[0];
                pos.Y = (float)list[1];
                pos.Z = (float)list[2];
            }
            else
            {
                var x = input.ContainsKey("X") ? float.Parse(input["X"].ToString()) : this.Position.X;
                //float x = number != null ? number?.Value : this.Position.X;

                var y = input.ContainsKey("Y") ? float.Parse(input["Y"].ToString()) : this.Position.Y;
                //float y = number != null ? number?.Value : this.Position.Y;

                var z = input.ContainsKey("Z") ? float.Parse(input["Z"].ToString()) : this.Position.Z;
                //float z = number != null ? number?.Value : this.Position.Z;

                pos.X = x;
                pos.Y = y;
                pos.Z = z;
            }
            
            Position = pos;

            number = input.ContainsKey("Width") ? float.Parse(input["Width"].ToString()) : input.ContainsKey("W") ? float.Parse(input["W"].ToString()) : null;
            this.Width = number != null ? number.Value : this.Width;

            number = input.ContainsKey("Height") ? float.Parse(input["Height"].ToString()) : input.ContainsKey("H") ? float.Parse(input["H"].ToString()) : null;
            this.Height = number != null ? number.Value : this.Height;

            number = input.ContainsKey("Depth") ? float.Parse(input["Depth"].ToString()) : input.ContainsKey("D") ? float.Parse(input["D"].ToString()) : null;
            this.Depth = number != null ? number.Value : this.Depth;

            number = input.ContainsKey("Angle") ? float.Parse(input["Angle"].ToString()) : null;
            this.Angle = number != null ? number.Value : this.Angle;

            this.Label = input.ContainsKey("Label") ? input["Label"] as string : this.Label;
            this.Type = input.ContainsKey("Type") ? input["Type"] as string : this.Type;
            this.Supertype = input.ContainsKey("Supertype") ? input["Supertype"] as string : this.Supertype;

            number = input.ContainsKey("Confidence") ? float.Parse(input["Confidence"].ToString()) : null;
            float confidence = number != null ? number.Value : this.Confidence.Value;
            this.Confidence.Value = confidence;

            string cause = input.ContainsKey("Cause") ? input["Cause"] as string : this.Cause.ToString();
            this.Cause = Enum.Parse<ObjectCause>(cause);

            string existence = input.ContainsKey("Existence") ? input["Existence"] as string : this.Existence.ToString();
            this.Existence = Enum.Parse<SpatialExistence>(existence);

            this.Immobile = input.ContainsKey("Existence") ? (bool)input["Existence"] : this.Immobile;

            string shape = input.ContainsKey("Shape") ? input["Shape"] as string : this.Shape.ToString();
            this.Shape = Enum.Parse<ObjectShape>(shape);

            this.Look = input.ContainsKey("Look") ? input["Look"] as string : this.Look;

            foreach (var dict in input)
            {
                string key = dict.Key;
                if (!SpatialObject.StringAttributes.ContainsKey(key) &&
                    !SpatialObject.NumericAttributes.ContainsKey(key) &&
                    !SpatialObject.BooleanAttributes.ContainsKey(key))
                {
                    SetData(key, dict.Value);
                }
            }

            this.Updated = DateTime.Now;
        }

        public override string ToString()
        {
            return "SO: "+Id;
        }

        public string Describe()
        {
            var res = string.Empty;

            if (!string.IsNullOrEmpty(_label) && _label != _id)
            {
                res += "(" + _label + "), ";
            }
            else
            {
                res += "(" + _id + "), ";
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
            res += string.Format("\n𝜶{0:F2}° \n", Yaw);
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
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
                p2 = p2.Rotate(-_angle);
                p3 = p3.Rotate(-_angle);
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
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
                p2 = p2.Rotate(-_angle);
                p3 = p3.Rotate(-_angle);
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
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
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
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
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
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
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
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
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
            var points = new CGPoint[] { p0, p1, p2, p3 };
            if (!local)
            {
                p0 = p0.Rotate(-_angle);
                p1 = p1.Rotate(-_angle);
                p2 = p2.Rotate(-_angle);
                p3 = p3.Rotate(-_angle);
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
            return pt.ConvertIntoLocal(_position, _angle);
        }

        public SCNVector3[] ConvertIntoLocal(SCNVector3[] pts)
        {
            var res = new SCNVector3[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                res[i] = pts[i].ConvertIntoLocal(_position, _angle);
            return res;
        }

        public SCNVector3[] Rotate(SCNVector3[] pts, float angle)
        {
            var res = new SCNVector3[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                res[i] = pts[i].Rotate(angle);
            return res;
        }

        public CGPoint[] Rotate(CGPoint[] pts, float angle)
        {
            var res = new CGPoint[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                res[i] = pts[i].Rotate(angle);
            return res;
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
                zone |= BBoxSectors.Above;
            }
            else if (point.Y - delta < 0.0f)
            {
                zone |= BBoxSectors.Below;
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

        public SCNVector3 CalcSectorLengths(BBoxSectors sector = BBoxSectors.Inside)
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

            if (sector.HasFlag(BBoxSectors.Above) || sector.HasFlag(BBoxSectors.Below))
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

        public List<SpatialRelation> CalcTopologies(SpatialObject subject)
        {
            var result = new List<SpatialRelation>();

            var gap = 0.0f;
            var minDistance = 0.0f;

            /// calculations in global world space
            var centerVector = subject.Center - Center;
            var centerDistance = centerVector.Length;
            var radiusSum = Radius + subject.Radius;
            var canNotOverlap = centerDistance > radiusSum;
            var theta = subject.Angle - _angle;
            var isDisjoint = true;
            var isConnected = false;
            var aligned = false;
            var isBeside = false;

            /// calculations in local object space
            var subjectPts = subject.CalcPoints();
            var localPts = ConvertIntoLocal(subjectPts);
            var zones = new List<BBoxSectors>();
            foreach(var pt in localPts)
            {
                zones.Add(IsSectorOf(pt, false, 0.00001f));
            }
            var localCenter = ConvertIntoLocal(subject.Center);
            var centerZone = IsSectorOf(localCenter, false, -Adjustment.MaxGap);

            (gap, minDistance) = CalcAndAddProximities(result, subject, gap, minDistance, centerDistance, theta);
            (gap, minDistance) = CalcAndAddDirectionalities(result, subject, gap, minDistance, localCenter, centerZone, theta);
            (minDistance, canNotOverlap, aligned, isBeside) = CalcAndAddAdjacancies(result, subject, minDistance, canNotOverlap, localPts, localCenter, theta);
            gap = CalcAndAddAssembly(result, subject, gap, minDistance, aligned, isBeside, isDisjoint, canNotOverlap, isConnected, localPts, zones, centerDistance, theta);
            (gap, minDistance) = CalcAndAddOrientations(result, subject, gap, minDistance, localCenter, centerDistance, theta);
            CalcAndAddVisibilities(result, subject, centerDistance);

            return result;

        }

        public void CalcAndAddVisibilities(List<SpatialRelation> result, SpatialObject subject, float centerDistance)
        {
            if(_context == null || _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Visbility))
            {
                if (_type == "Person" || (_cause == ObjectCause.SelfTracked && _existence == SpatialExistence.Real))
                {
                    var rad = Math.Atan2(subject.Center.X, subject.Center.Z);
                    var angle = rad * 180.0f / Math.PI;
                    float hourAngle = 30.0f; // 360.0/12.0
                    if (angle < 0.0f)
                    {
                        angle -= (hourAngle / 2.0f);
                    }
                    else
                    {
                        angle += (hourAngle / 2.0f);
                    }

                    int cnt = (int)Math.Round(angle / hourAngle);
                    bool doit = true;
                    SpatialPredicate pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.TwelveOClock);

                    switch (cnt)
                    {
                        case 4:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.EightOClock);
                            break;
                        case 3:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.NineOClock);
                            break;
                        case 2:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.TenOClock);
                            break;
                        case 1:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.ElevenOClock);
                            break;
                        case 0:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.TwelveOClock);
                            break;
                        case -1:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.OneOClock);
                            break;
                        case -2:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.TwoOClock);
                            break;
                        case -3:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.ThreeOClock);
                            break;
                        case -4:
                            pred = SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.FourOClock);
                            break;
                        default:
                            doit = false;
                            break;
                    }

                    if (doit)
                    {
                        SpatialRelation relation = new SpatialRelation(subject, pred, this, centerDistance, (float)rad);
                        result.Add(relation);

                        // Additional check for tangible proximity (<= 1.25 meters)
                        if (centerDistance <= 1.25f)
                        {
                            relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.Tangible), this, centerDistance, (float)rad);
                            result.Add(relation);
                        }
                    }
                }
            }
        }

        public (float, float) CalcAndAddOrientations(List<SpatialRelation> result, SpatialObject subject, float gap, float minDistance, SCNVector3 localCenter, float centerDistance, float theta)
        {

            // If objects are aligned based on angle (within maxAngleDelta)
            if (Math.Abs(theta) < Adjustment.MaxAngleDelta)
            {
                gap = localCenter.Z;
                SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.Aligned), this, gap, theta);
                result.Add(relation);

                // Check if the objects are front-aligned
                float frontGap = localCenter.Z + subject.Depth / 2.0f - _depth / 2.0f;
                if (Math.Abs(frontGap) < Adjustment.MaxGap)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.FrontAligned), this, frontGap, theta);
                    result.Add(relation);
                }

                // Check if the objects are back-aligned
                float backGap = localCenter.Z - subject.Depth / 2.0f + _depth / 2.0f;
                if (Math.Abs(backGap) < Adjustment.MaxGap)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.BackAligned), this, backGap, theta);
                    result.Add(relation);
                }

                // Check if the objects are right-aligned
                float rightGap = localCenter.X - subject.Width / 2.0f + _width / 2.0f;
                if (Math.Abs(rightGap) < Adjustment.MaxGap)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.RightAligned), this, rightGap, theta);
                    result.Add(relation);
                }

                // Check if the objects are left-aligned
                float leftGap = localCenter.X + subject.Width / 2.0f - _width / 2.0f;
                if (Math.Abs(leftGap) < Adjustment.MaxGap)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.LeftAligned), this, leftGap, theta);
                    result.Add(relation);
                }
            }
            else
            {
                // If not aligned, check for opposite or orthogonal orientations
                gap = centerDistance;

                // Check if objects are opposite to each other
                if (Math.Abs(theta % Math.PI) < Adjustment.MaxAngleDelta)
                {
                    SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.Opposite), this, gap, theta);
                    result.Add(relation);
                }
                // Check if objects are orthogonal to each other
                else if (Math.Abs(theta % (Math.PI / 2.0f)) < Adjustment.MaxAngleDelta)
                {
                    SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateOrientations>((int)SpatialPredicateOrientations.Orthogonal), this, gap, theta);
                    result.Add(relation);
                }
            }

            return (gap, minDistance);
        }

        public float CalcAndAddAssembly(List<SpatialRelation> result, SpatialObject subject, float gap, float minDistance, bool aligned, bool isBeside, bool isDisjoint, bool canNotOverlap, bool isConnected, SCNVector3[] localPts, List<BBoxSectors> zones, float centerDistance, float theta)
        {
            // If the object is beside another object, add a "Beside" relation
            if (isBeside)
            {
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Beside), this, minDistance, theta);
                result.Add(relation);
            }

            // Check if all zones contain 'i' (inside)
            if (zones.All(zone => zone.HasFlag(BBoxSectors.Inside)))
            {
                isDisjoint = false;
                SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Inside), this, centerDistance, theta);
                result.Add(relation);
                if(_context == null || _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Connectivity))
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateContacts>((int)SpatialPredicateContacts.In), this, centerDistance, theta);
                    result.Add(relation);
                }
            }
            else
            {
                // Check if the subject is containing this object based on size and distance
                if ((subject.Radius - Radius) > centerDistance / 2.0f && subject.Width > _width && subject.Height > _height && subject.Depth > _depth)
                {
                    isDisjoint = false;
                    SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Containing), this, 0.0f, theta);
                    result.Add(relation);
                }
                else
                {
                    int cnt = zones.Count(zone => zone.HasFlag(BBoxSectors.Inside));
                    if (cnt > 0 && !canNotOverlap)
                    {
                        isDisjoint = false;
                        SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Overlapping), this, centerDistance, theta);
                        result.Add(relation);
                    }

                    // Compute crossings based on local points
                    var crossings = 0;
                    float minY = localPts[0].Y;
                    float maxY = localPts[localPts.Length - 1].Y;
                    float minX = float.MaxValue;
                    float maxX = -float.MaxValue;
                    float minZ = float.MaxValue;
                    float maxZ = -float.MaxValue;

                    foreach (var pt in localPts)
                    {
                        minX = Math.Min(minX, pt.X);
                        maxX = Math.Max(maxX, pt.X);
                        minZ = Math.Min(minZ, pt.Z);
                        maxZ = Math.Max(maxZ, pt.Z);
                    }

                    if (!canNotOverlap)
                    {
                        if (minX < -_width / 2.0f && maxX > _width / 2.0f && minZ < _depth / 2.0f && maxZ > -_depth / 2.0f && minY < _height && maxY > 0)
                        {
                            crossings += 1;
                        }
                        if (minZ < -_depth / 2.0f && maxZ > _depth / 2.0f && minX < _width / 2.0f && maxX > -_width / 2.0f && minY < _height && maxY > 0)
                        {
                            crossings += 1;
                        }
                        if (minY < 0.0f && maxY > _height && minX < _width / 2.0f && maxX > -_width / 2.0f && minZ < _depth / 2.0f && maxZ > -_depth / 2.0f)
                        {
                            crossings += 1;
                        }
                        if (crossings > 0)
                        {
                            isDisjoint = false;
                            SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Crossing), this, centerDistance, theta);
                            result.Add(relation);
                        }
                    }

                    // Compute overlap in each dimension
                    float ylap = _height;
                    if (maxY < _height && minY > 0) {
                        ylap = maxY - minY;
                    }
                    else
                    {
                        if (minY > 0)
                            ylap = Math.Abs(_height - minY);
                        else
                            ylap = Math.Abs(maxY);
                    }
                    float xlap = ComputeOverlap(minX, maxX, _width, true);
                    float zlap = ComputeOverlap(minZ, maxZ, _depth, false);

                    // Check for touching or meeting relations based on overlap
                    if (minY < _height + Adjustment.MaxGap && maxY > -Adjustment.MaxGap)
                    {
                        gap = Math.Min(xlap, zlap);
                        if (!aligned && canNotOverlap && gap > 0.0f && gap < Adjustment.MaxGap)
                        {
                            if ((maxX < -_width / 2.0f + Adjustment.MaxGap) || (minX > _width / 2.0f - Adjustment.MaxGap) ||
                                (maxZ < -_depth / 2.0f + Adjustment.MaxGap) || (minZ > _depth / 2.0f - Adjustment.MaxGap))
                            {
                                SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Touching), this, gap, theta);
                                result.Add(relation);
                                if (!isConnected && (_context == null || _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Connectivity)))
                                {
                                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateContacts>((int)SpatialPredicateContacts.By), this, gap, theta);
                                    result.Add(relation);
                                    isConnected = true;
                                }
                            }
                            else
                            {
                                // Handling for rotated bounding boxes (not yet implemented)
                                throw new NotImplementedException("OOPS, rotated bbox might cross: assembly relations by shortest distance not yet implemented!");
                            }
                        }
                        else
                        {
                            if (xlap >= 0.0f && zlap >= 0.0f)
                            {
                                if (ylap > Adjustment.MaxGap && gap < Adjustment.MaxGap) // Beside
                                {
                                    if (xlap > Adjustment.MaxGap || zlap > Adjustment.MaxGap)
                                    {
                                        SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Meeting), this, Math.Max(xlap, zlap), theta);
                                        result.Add(relation);
                                        if (!isConnected && (_context == null || _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Connectivity)) && subject.Volume < Volume)
                                        {
                                            relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateContacts>((int)SpatialPredicateContacts.At), this, gap, theta);
                                            result.Add(relation);
                                            isConnected = true;
                                        }
                                    }
                                    else
                                    {
                                        SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Touching), this, gap, theta);
                                        result.Add(relation);
                                        if (!isConnected && (_context == null || _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Connectivity)))
                                        {
                                            relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateContacts>((int)SpatialPredicateContacts.By), this, gap, theta);
                                            result.Add(relation);
                                            isConnected = true;
                                        }
                                    }
                                }
                                else // On top or underneath
                                {
                                    gap = ylap;
                                    if (xlap > Adjustment.MaxGap && zlap > Adjustment.MaxGap)
                                    {
                                        SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Meeting), this, gap, theta);
                                        result.Add(relation);
                                    }
                                    else
                                    {
                                        SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Touching), this, gap, theta);
                                        result.Add(relation);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (isDisjoint)
            {
                gap = centerDistance;
                SpatialRelation relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Disjoint), this, gap, theta);
                result.Add(relation);
            }


            return gap;
        }

        public (float, bool, bool, bool) CalcAndAddAdjacancies(List<SpatialRelation> result, SpatialObject subject, float minDistance, bool canNotOverlap, SCNVector3[] localPts, SCNVector3 localCenter, float theta)
        {
            var aligned = false;
            var isBeside = false;
            var centerZone = IsSectorOf(localCenter, true, -Adjustment.MaxGap);

            // Check if the center zone is not 'I' (neutral/inside zone)
            if (centerZone != BBoxSectors.Inside)
            {
                // Check if the angle is aligned within a certain tolerance
                //if (Math.Abs(Math.Truncate(theta/(Math.PI / 2.0f))) < Adjustment.MaxAngleDelta)
                if (Math.Abs(theta % (Math.PI / 2.0f)) < Adjustment.MaxAngleDelta)
                {
                    aligned = true;
                }

                var min = float.MaxValue;

                // Compare the zone and compute the minimal distances
                if (centerZone == BBoxSectors.Left)
                {
                    foreach (var pt in localPts)
                    {
                        min = Math.Min(min, pt.X - _width / 2.0f);
                    }
                    if (min >= 0.0f)
                    {
                        canNotOverlap = true;
                        isBeside = true;
                        //gap = minDistance;
                        minDistance = min;
                        var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.LeftSide), this, min, theta);
                        result.Add(relation);
                    }
                }
                else if (centerZone == BBoxSectors.Right)
                {
                    foreach (var pt in localPts)
                    {
                        min = Math.Min(min, -pt.X - Width / 2.0f);
                    }
                    if (min >= 0.0f)
                    {
                        canNotOverlap = true;
                        isBeside = true;
                        minDistance = min;
                        var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.RightSide), this, min, theta);
                        result.Add(relation);
                    }
                }
                else if (centerZone == BBoxSectors.Above)
                {
                    foreach (var pt in localPts)
                    {
                        min = Math.Min(min, pt.Y - Height);
                    }
                    if (min >= 0.0f)
                    {
                        canNotOverlap = true;
                        minDistance = min;
                        //gap = minDistance;
                        var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.OnTop), this, min, theta);
                        result.Add(relation);
                        if(_context == null || _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Connectivity))
                        {
                            relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateContacts>((int)SpatialPredicateContacts.On), this, min, theta);
                            result.Add(relation);
                        }
                        relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.UpperSide), this, min, theta);
                        result.Add(relation);
                    }
                }
                else if (centerZone == BBoxSectors.Below)
                {
                    foreach (var pt in localPts)
                    {
                        min = Math.Min(min, -pt.Y);
                    }
                    if (min >= 0.0f)
                    {
                        canNotOverlap = true;
                        //gap = minDistance;
                        minDistance = min;
                        var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.Beneath), this, min, theta);
                        result.Add(relation);
                        relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.LowerSide), this, min, theta);
                        result.Add(relation);
                    }
                }
                else if (centerZone == BBoxSectors.Ahead)
                {
                    foreach (var pt in localPts)
                    {
                        min = Math.Min(min, pt.Z - _depth / 2.0f);
                    }
                    if (min >= 0.0f)
                    {
                        canNotOverlap = true;
                        isBeside = true;
                        //gap = minDistance;
                        minDistance = min;
                        var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.FrontSide), this, min, theta);
                        result.Add(relation);
                    }
                }
                else if (centerZone == BBoxSectors.Behind)
                {
                    foreach (var pt in localPts)
                    {
                        min = Math.Min(min, -pt.Z - _depth / 2.0f);
                    }
                    if (min >= 0.0f)
                    {
                        canNotOverlap = true;
                        isBeside = true;
                        //gap = minDistance;
                        minDistance = min;
                        var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAdjacency>((int)SpatialPredicateAdjacency.BackSide), this, min, theta);
                        result.Add(relation);
                    }
                }

                // If the object is beside another object, add a "Beside" relation
                //if (isBeside)
                //{
                //    var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateAssembly>((int)SpatialPredicateAssembly.Beside), this, minDistance, theta);
                //    result.Add(relation);
                //}
            }

            return (minDistance, canNotOverlap, aligned, isBeside);
        }

        public (float, float) CalcAndAddDirectionalities(List<SpatialRelation> result, SpatialObject subject, float gap, float minDistance, SCNVector3 localCenter, BBoxSectors centerZone, float theta)
        {
            // Check if center zone contains Left
            if (centerZone.HasFlag(BBoxSectors.Left))
            {
                gap = localCenter.X - _width / 2.0f - subject.Width / 2.0f;
                minDistance = gap;
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateDirectionality>((int)SpatialPredicateDirectionality.Left), this, gap, theta);
                result.Add(relation);
            }
            // Check if center zone contains Right
            else if (centerZone.HasFlag(BBoxSectors.Right))
            {
                gap = -localCenter.X - _width / 2.0f - subject.Width / 2.0f;
                minDistance = gap;
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateDirectionality>((int)SpatialPredicateDirectionality.Right), this, gap, theta);
                result.Add(relation);
            }

            // Check if center zone contains Ahead
            if (centerZone.HasFlag(BBoxSectors.Ahead))
            {
                gap = localCenter.Z - _depth / 2.0f - subject.Depth / 2.0f;
                minDistance = gap;
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateDirectionality>((int)SpatialPredicateDirectionality.Ahead), this, gap, theta);
                result.Add(relation);
            }
            // Check if center zone contains Behind
            else if (centerZone.HasFlag(BBoxSectors.Behind))
            {
                gap = -localCenter.Z - _depth / 2.0f - subject.Depth / 2.0f;
                minDistance = gap;
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateDirectionality>((int)SpatialPredicateDirectionality.Behind), this, gap, theta);
                result.Add(relation);
            }

            // Check if center zone contains Above
            if (centerZone.HasFlag(BBoxSectors.Above))
            {
                gap = localCenter.Y - subject.Height / 2.0f - _height;
                minDistance = gap;
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateDirectionality>((int)SpatialPredicateDirectionality.Above), this, gap, theta);
                result.Add(relation);
            }
            // Check if center zone contains Below
            else if (centerZone.HasFlag(BBoxSectors.Below))
            {
                gap = -localCenter.Y - subject.Height / 2.0f;
                minDistance = gap;
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateDirectionality>((int)SpatialPredicateDirectionality.Below), this, gap, theta);
                result.Add(relation);
            }
            return (gap, minDistance);
        }

        public (float,float) CalcAndAddProximities(List<SpatialRelation> result, SpatialObject subject, float gap, float minDistance, float centerDistance, float theta)
        {
            if (centerDistance < subject.CalcNearbyRadius() + CalcNearbyRadius())
            {
                gap = centerDistance;
                minDistance = gap;
                // Assuming SpatialRelation has a constructor that accepts the following arguments
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateProximity>((int)SpatialPredicateProximity.Near), this, gap, theta);
                result.Add(relation);
            }
            else
            {
                var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateProximity>((int)SpatialPredicateProximity.Far), this, centerDistance, theta);
                result.Add(relation);
            }
            return (gap,minDistance);
        }

        public List<SpatialRelation> CalcSimilarities(SpatialObject subject)
        {
            var result = new List<SpatialRelation>();
            SpatialRelation relation;
            float theta = subject.Angle - _angle;
            float val = 0.0f;
            float minVal = 0.0f;
            float maxVal = 0.0f;
            bool sameWidth = false;
            bool sameDepth = false;
            bool sameHeight = false;

            val = (Center - subject.Center).Length;
            if (val < Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameCenter), this, val, theta);
                result.Add(relation);
            }

            val = (_position - subject.Position).Length;
            if (val < Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SamePosition), this, val, theta);
                result.Add(relation);
            }

            val = Math.Abs(_width - subject.Width);
            if (val < Adjustment.MaxGap)
            {
                sameWidth = true;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameWidth), this, val, theta);
                result.Add(relation);
            }

            val = Math.Abs(_depth - subject.Depth);
            if (val < Adjustment.MaxGap)
            {
                sameDepth = true;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameDepth), this, val, theta);
                result.Add(relation);
            }

            val = Math.Abs(_height - subject.Height);
            if (val < Adjustment.MaxGap)
            {
                sameHeight = true;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameHeight), this, val, theta);
                result.Add(relation);
            }

            val = subject.Depth * subject.Width;
            minVal = (_depth - Adjustment.MaxGap) + (_width - Adjustment.MaxGap);
            maxVal = (_depth + Adjustment.MaxGap) + (_width + Adjustment.MaxGap);
            if (val > minVal && val < maxVal)
            {
                float gap = _depth * _width - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SamePerimeter), this, 2.0f * gap, theta);
                result.Add(relation);
            }

            if (sameWidth && sameDepth && sameHeight)
            {
                val = subject.Volume - Volume;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameCuboid), this, val, theta);
                result.Add(relation);
            }

            val = Math.Abs(Length - subject.Length);
            if (val < Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameLength), this, val, theta);
                result.Add(relation);
            }

            val = subject.Height * subject.Width;
            minVal = (_height - Adjustment.MaxGap) * (_width - Adjustment.MaxGap);
            maxVal = (_height + Adjustment.MaxGap) * (_width + Adjustment.MaxGap);
            if (val > minVal && val < maxVal)
            {
                float gap = _height * _width - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameFront), this, gap, theta);
                result.Add(relation);
            }

            val = subject.Height * subject.Depth;
            minVal = (_height - Adjustment.MaxGap) * (_depth - Adjustment.MaxGap);
            maxVal = (_height + Adjustment.MaxGap) * (_depth + Adjustment.MaxGap);
            if (val > minVal && val < maxVal)
            {
                float gap = _height * _depth - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameSide), this, gap, theta);
                result.Add(relation);
            }

            val = subject.Width * subject.Depth;
            minVal = (_width - Adjustment.MaxGap) * (_depth - Adjustment.MaxGap);
            maxVal = (_width + Adjustment.MaxGap) * (_depth + Adjustment.MaxGap);
            if (val > minVal && val < maxVal)
            {
                float gap = _width * _depth - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameFootprint), this, gap, theta);
                result.Add(relation);
            }

            val = (subject.Width * subject.Width) + (subject.Depth * subject.Depth) + (subject.Height * subject.Height);
            minVal = ((_width - Adjustment.MaxGap) * (_width - Adjustment.MaxGap)) + ((_depth - Adjustment.MaxGap) * (_depth - Adjustment.MaxGap)) + ((_height - Adjustment.MaxGap) * (_height - Adjustment.MaxGap));
            maxVal = ((_width + Adjustment.MaxGap) * (_width + Adjustment.MaxGap)) + ((_depth + Adjustment.MaxGap) * (_depth + Adjustment.MaxGap)) + ((_height + Adjustment.MaxGap) * (_height + Adjustment.MaxGap));
            if (val > minVal && val < maxVal)
            {
                float gap = ((_width * _width) + (_depth * _depth) + (_height * _height)) - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameSurface), this, 2.0f * gap, theta);
                result.Add(relation);
            }

            val = subject.Width * subject.Depth * subject.Height;
            minVal = (_width - Adjustment.MaxGap) * (_depth - Adjustment.MaxGap) * (_height - Adjustment.MaxGap);
            maxVal = (_width + Adjustment.MaxGap) * (_depth + Adjustment.MaxGap) * (_height + Adjustment.MaxGap);
            if (val > minVal && val < maxVal)
            {
                float gap = _width * _depth * _height - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameVolume), this, gap, theta);
                result.Add(relation);

                val = (_position - subject.Position).Length;
                float angleDiff = Math.Abs(_angle - subject.Angle);
                if (sameWidth && sameDepth && sameHeight && val < Adjustment.MaxGap && angleDiff < Adjustment.MaxAngleDelta)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.Congruent), this, gap, theta);
                    result.Add(relation);
                }
            }

            if (_shape == subject.Shape && _shape != ObjectShape.Unknown && subject.Shape !=  ObjectShape.Unknown)
            {
                float gap = _width * _depth * _height - val;
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateSimilarity>((int)SpatialPredicateSimilarity.SameShape), this, gap, theta);
                result.Add(relation);
            }

            return result;
        }

        public List<SpatialRelation> CalcComparisons(SpatialObject subject)
        {
            var result = new List<SpatialRelation>();
            SpatialRelation relation;
            float theta = subject.Angle - _angle;
            float objVal = 0.0f;
            float subjVal = 0.0f;
            float diff = 0.0f;

            objVal = Length;
            subjVal = subject.Length;
            diff = subjVal - objVal;
            bool shorterAdded = false;

            if (diff > Adjustment.MaxGap * Adjustment.MaxGap * Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Longer), this, diff, theta);
                result.Add(relation);
            }
            else if (-diff > Adjustment.MaxGap * Adjustment.MaxGap * Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Shorter), this, diff, theta);
                result.Add(relation);
                shorterAdded = true;
            }

            objVal = _height;
            subjVal = subject.Height;
            diff = subjVal - objVal;

            if (diff > Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Taller), this, diff, theta);
                result.Add(relation);
            }
            else if (-diff > Adjustment.MaxGap && !shorterAdded)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Shorter), this, diff, theta);
                result.Add(relation);
            }

            if (subject.MainDirection == 2)
            {
                objVal = Footprint;
                subjVal = subject.Footprint;
                diff = subjVal - objVal;

                if (diff > Adjustment.MaxGap * Adjustment.MaxGap)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Wider), this, diff, theta);
                    result.Add(relation);
                }
                else if (-diff > Adjustment.MaxGap * Adjustment.MaxGap)
                {
                    relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Thinner), this, diff, theta);
                    result.Add(relation);
                }
            }

            objVal = Volume;
            subjVal = subject.Volume;
            diff = subjVal - objVal;

            if (diff > Adjustment.MaxGap * Adjustment.MaxGap * Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Bigger), this, diff, theta);
                result.Add(relation);

                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Exceeding), this, diff, theta);
                result.Add(relation);
            }
            else if (-diff > Adjustment.MaxGap * Adjustment.MaxGap * Adjustment.MaxGap)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Smaller), this, diff, theta);
                result.Add(relation);
            }

            if (_height > subject.Height && Footprint > subject.Footprint)
            {
                relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateComparability>((int)SpatialPredicateComparability.Fitting), this, diff, theta);
                result.Add(relation);
            }

            return result;
        }

        public SpatialRelation CalcSector(SpatialObject subject, bool nearBy = false, float epsilon = 0.0f){
            var centerVector = subject.Center - Center;
            var centerDistance = centerVector.Length;
            var localCenter = ConvertIntoLocal(subject.Center);
            var centerZone = IsSectorOf(localCenter, nearBy, epsilon);
            var theta = subject.Angle - _angle;
            var pred = SpatialPredicate.CreateSpatialPredicateByName(BBoxSector.GetCombinedName(centerZone), false);
            return new SpatialRelation(subject, pred, this, centerDistance, theta);
        }


        public List<SpatialRelation> CalcAsSeen(SpatialObject subject, SpatialObject observer)
        {
            var result = new List<SpatialRelation>();

            var posVector = subject.Position - _position;
            float posDistance = posVector.Length;
            float radiusSum = BaseRadius + subject.BaseRadius;

            // Check for nearby
            if (posDistance < subject.CalcNearbyRadius() + CalcNearbyRadius())
            {
                var centerObject = observer.ConvertIntoLocal(Center);
                var centerSubject = observer.ConvertIntoLocal(subject.Center);

                if (centerSubject.Z > 0.0f && centerObject.Z > 0.0f) // both are ahead of observer
                {
                    // Turn both by view angle to become normal to observer
                    float rad = (float)Math.Atan2(centerObject.X, centerObject.Z);
                    var list = Rotate(new SCNVector3[] { centerObject, centerSubject }, -rad);
                    centerObject = list[0];
                    centerSubject = list[1];

                    float xgap = centerSubject.X - centerObject.X;
                    float zgap = centerSubject.Z - centerObject.Z;

                    if (Math.Abs(xgap) > Math.Min(_width / 2.0f, _depth / 2.0f) && Math.Abs(zgap) < radiusSum)
                    {
                        if (xgap > 0.0f)
                        {
                            var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.SeenLeft), this, Math.Abs(xgap), 0.0f);
                            result.Add(relation);
                        }
                        else
                        {
                            var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.SeenRight), this, Math.Abs(xgap), 0.0f);
                            result.Add(relation);
                        }
                    }

                    if (Math.Abs(zgap) > Math.Min(_width / 2.0f, _depth / 2.0f) && Math.Abs(xgap) < radiusSum)
                    {
                        if (zgap > 0.0f)
                        {
                            var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.AtRear), this, Math.Abs(zgap), 0.0f);
                            result.Add(relation);
                        }
                        else
                        {
                            var relation = new SpatialRelation(subject, SpatialPredicate.CreateSpatialPredicate<SpatialPredicateVisibility>((int)SpatialPredicateVisibility.InFront), this, Math.Abs(zgap), 0.0f);
                            result.Add(relation);
                        }
                    }
                }
            }

            return result;
        }

        public List<SpatialRelation> Relate(SpatialObject subject, bool topology = false, bool similarity = false, bool comparison = false)
        {


            var result = new List<SpatialRelation>();

            // Topology condition
            if (topology || (_context!= null && _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Topology)) || (_context != null && _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Connectivity)))
            {
                result.AddRange(CalcTopologies(subject));
            }

            // Similarity condition
            if (similarity || (_context != null && _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Similarity)))
            {
                result.AddRange(CalcSimilarities(subject));
            }

            // Comparison condition
            if (comparison || (_context != null && _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Comparability)))
            {
                result.AddRange(CalcComparisons(subject));
            }

            // Visibility condition
            if (_context?.Observer != null && (_context != null && _context.DeduceCategories.HasFlag(SpatialPredicatedCategories.Visbility)))
            {
                result.AddRange(CalcAsSeen(subject, _context.Observer));
            }

            return result;
        }

        public object CalcRelationValue(string predicate, string property, List<int> pre)
        {
            var result = 0.0f;

            // FIXME: Take min instead of last?
            foreach (int i in pre)
            {
                var rels = _context?.RelationsWith(i, predicate);
                foreach (var rel in rels)
                {
                    if (rel.Subject == this)
                    {
                        var pi = typeof(SpatialRelation).GetProperties().Where(pi => pi.Name.ToLower() == property.ToLower()).First();
                        if (pi == null)
                            return null;
                        return pi.GetValue(rel);

                    }
                }
            }
            return result;
        }

        private float ComputeOverlap(float min, float max, float size, bool adjust)
        {
            // Compute overlap in one dimension (x, y, or z)
            float overlap = size;
            if (min < size / 2.0f + Adjustment.MaxGap && max > -size / 2.0f - Adjustment.MaxGap)
            {
                if (max < size / 2.0f && min > -size / 2.0f)
                {
                    overlap = max - min;
                }
                else
                {
                    if ((adjust && min > -size / 2.0f - Adjustment.MaxGap) || (!adjust && min > -size / 2.0f))
                    {
                        overlap = Math.Abs(size / 2.0f - min);
                    }
                    else
                    {
                        overlap = Math.Abs(max + size / 2.0f);
                    }
                }
            }
            else
            {
                overlap = -1; // No overlap
            }
            return overlap;
        }


        #region Visualization

        public object BboxCube(Color color)
        {
            throw new NotImplementedException();
        }

        public object NearbySphere(Color color)
        {
            throw new NotImplementedException();
        }

        public object SectorCube(BBoxSectors sector = BBoxSectors.Inside, bool withLabel = false)
        {
            throw new NotImplementedException();
        }

        public object PointNodes(SCNVector3[] pts)
        {
            throw new NotImplementedException();
        }

        public static void Export3D(string to, object[] nodes)
        {
            throw new NotImplementedException();

        }

        #endregion

    }


}
