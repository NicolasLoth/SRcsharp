using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;


namespace SRcsharp.Library
{
    public struct SpatialPredicate
    {
        private static List<string> _allPredicateNames;

        public static List<string> AllPredicateNames
        {
            get { return _allPredicateNames; }
            set { _allPredicateNames = value; }
        }

        private static List<Type> _allPredicateEnums;

        public static List<Type> AllPredicateEnums
        {
            get { return _allPredicateEnums; }
            set { _allPredicateEnums = value; }
        }

        //private List<Enum> _allPredicateValues;

        //public List<Enum> AllPredicateValues
        //{
        //    get { return _allPredicateValues; }
        //    set { _allPredicateValues = value; }
        //}

        static SpatialPredicate()
        {
            _allPredicateEnums = new List<Type>();
            LoadAllPredicateEnums();
            _allPredicateNames = new List<string>();
            LoadAllPredicateNames();
        }

        //public SpatialPredicate()
        //{
        //    Proximity = SpatialPredicateProximity.Undefined;
        //    Directionality = SpatialPredicateDirectionality.Undefined;
        //    Adjacency = SpatialPredicateAdjacency.Undefined;
        //    Orientations = SpatialPredicateOrientations.Undefined;
        //    Assembly = SpatialPredicateAssembly.Undefined;
        //    Contacts = SpatialPredicateContacts.Undefined;
        //    Comparability = SpatialPredicateComparability.Undefined;
        //    Similarity = SpatialPredicateSimilarity.Undefined;
        //    Visibility = SpatialPredicateVisibility.Undefined;
        //    Geography = SpatialPredicateGeography.Undefined;

        //    //_allPredicateValues = new List<Enum>();
        //    //LoadAllPredicateValues();
        //}

        private static void LoadAllPredicateNames()
        {
            _allPredicateNames = new List<string>();
            _allPredicateEnums.ForEach(predEnum => _allPredicateNames.AddRange(predEnum.GetEnumNames()));        
        }

        private static void LoadAllPredicateEnums()
        {
            _allPredicateEnums = new List<Type>();
            var allEnums = typeof(SpatialPredicate).GetNestedTypes().Where(t => t.GetCustomAttribute<SpatialPredicateEnumAttribute>() != null);
            _allPredicateEnums = allEnums.ToList();
        }



        public List<Enum> AllPredicateValues
        {
            get
            {
                return new List<Enum>
                {
                    Proximity,
                    Directionality,
                    Adjacency,
                    Orientations,
                    Assembly,
                    Contacts,
                    Comparability,
                    Similarity,
                    Visibility,
                    Geography,
                    Sectors
                };
            }
            //_allPredicateValues = new List<Type>
            //{
            //    typeof(SpatialPredicateProximity),
            //    typeof(SpatialPredicateDirectionality),
            //    typeof(SpatialPredicateAdjacency),
            //    typeof(SpatialPredicateOrientations),
            //    typeof(SpatialPredicateAssembly),
            //    typeof(SpatialPredicateContacts),
            //    typeof(SpatialPredicateComparability),
            //    typeof(SpatialPredicateSimilarity),
            //    typeof(SpatialPredicateVisibility),
            //    typeof(SpatialPredicateGeography)
            //};
        }

        public static bool operator ==(SpatialPredicate pred1, SpatialPredicate pred2)
        {
            return pred1.Equals(pred2);
        }

        public static bool operator !=(SpatialPredicate pred1, SpatialPredicate pred2)
        {
            return !pred1.Equals(pred2);
        }

        public static SpatialPredicate CreateSpatialPredicateByName(string name, bool returnUndefinedPredicate = false)
        {
            if (!returnUndefinedPredicate && !_allPredicateNames.Contains(name)) { throw new Exception("SpatialPredicate type not found"); }

            var enumType = SpatialPredicate.GetSpatialPredicateEnumTypeForValue(name);
            var type = SpatialPredicate.GetSpatialPredicateTypeForValue(name);
            var pred = SpatialPredicate.CreateSpatialPredicate(enumType, (int)Enum.Parse(type, name));

            return pred;
        }

        public static SpatialPredicate CreateSpatialPredicateByNames(string[] names, bool returnUndefinedPredicate = false)
        {
            var pred = new SpatialPredicate();

            foreach (string name in names)
            {
                if (!returnUndefinedPredicate && !_allPredicateNames.Contains(name)) { throw new Exception("SpatialPredicate type not found"); }

                var enumType = SpatialPredicate.GetSpatialPredicateEnumTypeForValue(name);
                var type = SpatialPredicate.GetSpatialPredicateTypeForValue(name);
                pred.AddValue(enumType, (int)Enum.Parse(type, name));
                //var pred = SpatialPredicate.CreateSpatialPredicate(enumType, (int)Enum.Parse(type, name));
            }
            return pred;
        }

        public static bool IsDefined(SpatialPredicate pred)
        {
            return pred.AllPredicateValues.Any(e => Convert.ToInt32(e) != 0);
            //return
            //    pred.Proximity != 0 ||
            //    pred.Directionality != 0 ||
            //    pred.Adjacency != 0 ||
            //    pred.Orientations != 0 ||
            //    pred.Assembly != 0 ||
            //    pred.Contacts != 0 ||
            //    pred.Comparability != 0 ||
            //    pred.Similarity != 0 ||
            //    pred.Visibility != 0 ||
            //    pred.Geography != 0;
        }

        public bool IsDefined()
        {
            return IsDefined(this);
        }

        public static bool IsMultiDefined(SpatialPredicate pred)
        {
            return pred.AllPredicateValues.Where(e => Convert.ToInt32(e) != 0).Count() > 1;

        }

        public List<string> GetDefinedPredicateValueNames()
        {
            //return AllPredicateValues.SelectMany(e => e.GetFlagNames()).ToList();
            return AllPredicateValues.Where(e=> Convert.ToInt32(e) != 0).SelectMany(e => e.GetFlagNames()).ToList();
        }

        public string GetDefinedPredicateValueName(bool throwErrorIfMultiDefined = false)
        {
            var values = GetDefinedPredicateValueNames();
            if (throwErrorIfMultiDefined && values.Count() > 1)
                throw new Exception("Error: Multi defined Spatial Predicate");
            return values.FirstOrDefault();
        }

        public string RawValue { get { return GetDefinedPredicateValueName(true); } }

        //public List<string> RawValues { get { return GetDefinedPredicateValueNames(); } }

        public override string ToString()
        {
            return string.Join('|', RawValue);
        }

        public bool IsDefined(SpatialPredicateTypes type)
        {
            switch(type)
            {
                case SpatialPredicateTypes.Proximity:
                    return Proximity != 0;
                case SpatialPredicateTypes.Directionality:
                    return Directionality != 0;
                case SpatialPredicateTypes.Adjacency:
                    return Adjacency != 0;
                case SpatialPredicateTypes.Orientations:
                    return Orientations != 0;
                case SpatialPredicateTypes.Assembly:
                    return Assembly != 0;
                case SpatialPredicateTypes.Contacts:
                    return Contacts != 0;
                case SpatialPredicateTypes.Comparability:
                    return Comparability != 0;
                case SpatialPredicateTypes.Similarity:
                    return Similarity != 0;
                case SpatialPredicateTypes.Visibility:
                    return Visibility != 0;
                case SpatialPredicateTypes.Geography:
                    return Geography != 0;
                case SpatialPredicateTypes.Sectors:
                    return Sectors != 0;
            }
            return false;
        }

        public void AddValue(SpatialPredicateTypes type, int value)
        {
            switch (type)
            {
                case SpatialPredicateTypes.Proximity:
                    Proximity |= (SpatialPredicateProximity)value;
                    break;
                case SpatialPredicateTypes.Directionality:
                    Directionality |= (SpatialPredicateDirectionality)value;
                    break;
                case SpatialPredicateTypes.Adjacency:
                    Adjacency |= (SpatialPredicateAdjacency)value;
                    break;
                case SpatialPredicateTypes.Orientations:
                    Orientations |= (SpatialPredicateOrientations)value;
                    break;
                case SpatialPredicateTypes.Assembly:
                    Assembly |= (SpatialPredicateAssembly)value;
                    break;
                case SpatialPredicateTypes.Contacts:
                    Contacts |= (SpatialPredicateContacts)value;
                    break;
                case SpatialPredicateTypes.Comparability:
                    Comparability |= (SpatialPredicateComparability)value;
                    break;
                case SpatialPredicateTypes.Similarity:
                    Similarity |= (SpatialPredicateSimilarity)value;
                    break;
                case SpatialPredicateTypes.Visibility:
                    Visibility |= (SpatialPredicateVisibility)value;
                    break;
                case SpatialPredicateTypes.Geography:
                    Geography |= (SpatialPredicateGeography)value;
                    break;
                case SpatialPredicateTypes.Sectors:
                    Sectors |= (SpatialPredicateSectors)value;
                    break;
            }
        }

        public static SpatialPredicate CreateSpatialPredicate(SpatialPredicateTypes type, int value)
        {
            var pred = new SpatialPredicate();
            switch (type)
            {
                case SpatialPredicateTypes.Proximity:
                    pred.Proximity = (SpatialPredicateProximity)value;
                    break;
                case SpatialPredicateTypes.Directionality:
                    pred.Directionality = (SpatialPredicateDirectionality)value;
                    break;
                case SpatialPredicateTypes.Adjacency:
                    pred.Adjacency = (SpatialPredicateAdjacency)value;
                    break;
                case SpatialPredicateTypes.Orientations:
                    pred.Orientations = (SpatialPredicateOrientations)value;
                    break;
                case SpatialPredicateTypes.Assembly:
                    pred.Assembly = (SpatialPredicateAssembly)value;
                    break;
                case SpatialPredicateTypes.Contacts:
                    pred.Contacts = (SpatialPredicateContacts)value;
                    break;
                case SpatialPredicateTypes.Comparability:
                    pred.Comparability = (SpatialPredicateComparability)value;
                    break;
                case SpatialPredicateTypes.Similarity:
                    pred.Similarity = (SpatialPredicateSimilarity)value;
                    break;
                case SpatialPredicateTypes.Visibility:
                    pred.Visibility = (SpatialPredicateVisibility)value;
                    break;
                case SpatialPredicateTypes.Geography:
                    pred.Geography = (SpatialPredicateGeography)value;
                    break;
                case SpatialPredicateTypes.Sectors:
                    pred.Sectors = (SpatialPredicateSectors)value;
                    break;
            }

            return pred;
        }

        public static Type GetSpatialPredicateTypeForValue(string value)
        {

            if (!_allPredicateNames.Contains(value)) { return null; }

            if (Enum.GetNames(typeof(SpatialPredicateProximity)).Contains(value))
                return typeof(SpatialPredicateProximity);
            if (Enum.GetNames(typeof(SpatialPredicateDirectionality)).Contains(value))
                return typeof(SpatialPredicateDirectionality);
            if (Enum.GetNames(typeof(SpatialPredicateAdjacency)).Contains(value))
                return typeof(SpatialPredicateAdjacency);
            if (Enum.GetNames(typeof(SpatialPredicateOrientations)).Contains(value))
                return typeof(SpatialPredicateOrientations);
            if (Enum.GetNames(typeof(SpatialPredicateAssembly)).Contains(value))
                return typeof(SpatialPredicateAssembly);
            if (Enum.GetNames(typeof(SpatialPredicateContacts)).Contains(value))
                return typeof(SpatialPredicateContacts);
            if (Enum.GetNames(typeof(SpatialPredicateComparability)).Contains(value))
                return typeof(SpatialPredicateComparability);
            if (Enum.GetNames(typeof(SpatialPredicateSimilarity)).Contains(value))
                return typeof(SpatialPredicateSimilarity);
            if (Enum.GetNames(typeof(SpatialPredicateVisibility)).Contains(value))
                return typeof(SpatialPredicateVisibility);
            if (Enum.GetNames(typeof(SpatialPredicateGeography)).Contains(value))
                return typeof(SpatialPredicateGeography);
            if (Enum.GetNames(typeof(SpatialPredicateSectors)).Contains(value))
                return typeof(SpatialPredicateSectors);

            return null;

        }

        public static SpatialPredicateTypes GetSpatialPredicateEnumTypeForValue(string value)
        {
            if (!_allPredicateNames.Contains(value)) { return SpatialPredicateTypes.None; }


            if (Enum.GetNames(typeof(SpatialPredicateProximity)).Contains(value))
                return SpatialPredicateTypes.Proximity;
            if (Enum.GetNames(typeof(SpatialPredicateDirectionality)).Contains(value))
                return SpatialPredicateTypes.Directionality;
            if (Enum.GetNames(typeof(SpatialPredicateAdjacency)).Contains(value))
                return SpatialPredicateTypes.Adjacency;
            if (Enum.GetNames(typeof(SpatialPredicateOrientations)).Contains(value))
                return SpatialPredicateTypes.Orientations;
            if (Enum.GetNames(typeof(SpatialPredicateAssembly)).Contains(value))
                return SpatialPredicateTypes.Assembly;
            if (Enum.GetNames(typeof(SpatialPredicateContacts)).Contains(value))
                return SpatialPredicateTypes.Contacts;
            if (Enum.GetNames(typeof(SpatialPredicateComparability)).Contains(value))
                return SpatialPredicateTypes.Comparability;
            if (Enum.GetNames(typeof(SpatialPredicateSimilarity)).Contains(value))
                return SpatialPredicateTypes.Similarity;
            if (Enum.GetNames(typeof(SpatialPredicateVisibility)).Contains(value))
                return SpatialPredicateTypes.Visibility;
            if (Enum.GetNames(typeof(SpatialPredicateGeography)).Contains(value))
                return SpatialPredicateTypes.Geography;
            if (Enum.GetNames(typeof(SpatialPredicateSectors)).Contains(value))
                return SpatialPredicateTypes.Sectors;

            return SpatialPredicateTypes.None;
        }

        public static SpatialPredicate CreateSpatialPredicate<T>(int value) where T:Enum
        {
            var pred = new SpatialPredicate();
            if(typeof(T) == typeof(SpatialPredicateProximity))
                pred.Proximity = (SpatialPredicateProximity)value;
            if (typeof(T) == typeof(SpatialPredicateDirectionality))
                pred.Directionality = (SpatialPredicateDirectionality)value;
            if (typeof(T) == typeof(SpatialPredicateAdjacency))
                pred.Adjacency = (SpatialPredicateAdjacency)value;
            if (typeof(T) == typeof(SpatialPredicateOrientations))
                pred.Orientations = (SpatialPredicateOrientations)value;
            if (typeof(T) == typeof(SpatialPredicateAssembly))
                pred.Assembly = (SpatialPredicateAssembly)value;
            if (typeof(T) == typeof(SpatialPredicateContacts))
                pred.Contacts = (SpatialPredicateContacts)value;
            if (typeof(T) == typeof(SpatialPredicateComparability))
                pred.Comparability = (SpatialPredicateComparability)value;
            if (typeof(T) == typeof(SpatialPredicateSimilarity))
                pred.Similarity = (SpatialPredicateSimilarity)value;
            if (typeof(T) == typeof(SpatialPredicateVisibility))
                pred.Visibility = (SpatialPredicateVisibility)value;
            if (typeof(T) == typeof(SpatialPredicateGeography))
                pred.Geography = (SpatialPredicateGeography)value;
            if (typeof(T) == typeof(SpatialPredicateSectors))
                pred.Sectors = (SpatialPredicateSectors)value;

            return pred;
        }


        [Flags]
        public enum SpatialPredicateTypes 
        { 
            None= 0, 
            Proximity=1<<0,
            Directionality =1<<1,
            Adjacency =1<<2,
            Orientations = 1<<3,
            Assembly = 1 <<4,
            Contacts = 1<<5,
            Comparability = 1<<6,
            Similarity = 1 << 7,
            Visibility = 1<<8,
            Geography = 1<<9,
            Sectors = 1<<10
        }

        public SpatialPredicateProximity Proximity { get; set; }
        public SpatialPredicateDirectionality Directionality { get; set; }
        public SpatialPredicateAdjacency Adjacency { get; set; }
        public SpatialPredicateOrientations Orientations { get; set; }
        public SpatialPredicateAssembly Assembly { get; set; }
        public SpatialPredicateContacts Contacts { get; set; }
        public SpatialPredicateComparability Comparability { get; set; }
        public SpatialPredicateSimilarity Similarity { get; set; }
        public SpatialPredicateVisibility Visibility { get; set; }
        public SpatialPredicateGeography Geography { get; set; }
        public SpatialPredicateSectors Sectors { get; set; }


        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateProximity
        {
            Undefined = 0,
            Near = 1 << 0,
            Far = 1 << 1
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateDirectionality
        {
            Undefined = 0,
            Left = 1 << 0,
            Right = 1 << 1,
            Above = 1 << 2,
            Below = 1 << 3,
            Ahead = 1 << 4,
            Behind = 1 << 5
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateAdjacency
        {
            Undefined = 0,
            LeftSide = 1 << 0,
            RightSide = 1 << 1,
            OnTop = 1 << 2,
            Beneath = 1 << 3,
            UpperSide = 1 << 4,
            LowerSide = 1 << 5,
            FrontSide = 1 << 6,
            BackSide = 1 << 7
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateOrientations
        {
            Undefined = 0,
            Orthogonal = 1 << 0,
            Opposite = 1 << 1,
            Aligned = 1 << 2,
            FrontAligned = 1 << 3,
            BackAligned = 1 << 4,
            RightAligned = 1 << 5,
            LeftAligned = 1 << 6
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateAssembly
        {
            Undefined = 0,
            Disjoint = 1 << 0,
            Inside = 1 << 1,
            Containing = 1 << 2,
            Overlapping = 1 << 3,
            Crossing = 1 << 4,
            Touching = 1 << 5,
            Meeting = 1 << 6,
            Beside = 1 << 7
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateContacts //== Connectivity
        {
            Undefined = 0,
            On = 1 << 0,
            At = 1 << 1,
            By = 1 << 2,
            In = 1 << 3
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateComparability
        {
            Undefined = 0,
            Smaller = 1 << 0,
            Bigger = 1 << 1,
            Shorter = 1 << 2,
            Longer = 1 << 3,
            Taller = 1 << 4,
            Thinner = 1 << 5,
            Wider = 1 << 6,
            Fitting = 1 << 7,
            Exceeding = 1 << 8
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateSimilarity
        {
            Undefined = 0,
            SameHeight = 1 << 0,
            SameWidth = 1 << 1,
            SameDepth = 1 << 2,
            SameLength = 1 << 3,
            SameFront = 1 << 4,
            SameSide = 1 << 5,
            SameFootprint = 1 << 6,
            SameVolume = 1 << 7,
            SameCenter = 1 << 8,
            SamePosition = 1 << 9,
            SameCuboid = 1 << 10,
            Congruent = 1 << 11,
            SameShape = 1 << 12,
            SamePerimeter = 1 << 13,
            SameSurface = 1 << 14
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateVisibility
        {
            Undefined = 0,
            SeenLeft = 1 << 0,
            SeenRight = 1 << 1,
            InFront = 1 << 2,
            AtRear = 1 << 3,
            Tangible = 1 << 4,
            OneOClock = 1 << 5,
            TwoOClock = 1 << 6,
            ThreeOClock = 1 << 7,
            FourOClock = 1 << 8,
            FiveOClock = 1 << 9,
            SixOClock = 1 << 10,
            SevenOClock = 1 << 11,
            EightOClock = 1 << 12,
            NineOClock = 1 << 13,
            TenOClock = 1 << 14,
            ElevenOClock = 1 << 15,
            TwelveOClock = 1 << 16
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateGeography
        {
            Undefined = 0,
            North = 1 << 0,
            South = 1 << 1,
            East = 1 << 2,
            West = 1 << 3,
            Northwest = 1 << 4,
            Northeast = 1 << 5,
            Southwest = 1 << 6,
            Southeast = 1 << 7
        }

        [Flags]
        [SpatialPredicateEnumAttribute]
        public enum SpatialPredicateSectors
        {
            None = 0,
            I = 1 << 0,
            A = 1 << 1,
            B = 1 << 2,
            O = 1 << 3,
            U = 1 << 4,
            L = 1 << 5,
            R = 1 << 6,
            AL = 1 << 7,
            AR = 1 << 8,
            BL = 1 << 9,
            BR = 1 << 10,
            AO = 1 << 11,
            AU = 1 << 12,
            BO = 1 << 13,
            BU = 1 << 14,
            LO = 1 << 15,
            LU = 1 << 16,
            RO = 1 << 17,
            RU = 1 << 18,
            ALO = 1 << 19,
            ARO = 1 << 20,
            BLO = 1 << 21,
            BRO = 1 << 22,
            ALU = 1 << 23,
            ARU = 1 << 24,
            BLU = 1 << 25,
            BRU = 1 << 26
        }

        //public override bool Equals(object obj)
        //{
        //    return ((SpatialPredicate)obj).Equals(this);
        //}

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }
    }

    //public struct SpatialPredicateTopology
    //{
    //    public SpatialPredicateProximity Proximity { get; set; }
    //    public SpatialPredicateDirectionality Directionality { get; set; }
    //    public SpatialPredicateAdjacency Adjacency { get; }
    //    public SpatialPredicateOrientations Orientations { get; }
    //    public SpatialPredicateAssembly Assembly { get; }
    //}
}
    