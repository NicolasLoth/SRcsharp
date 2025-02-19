using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SRcsharp.Library.SREnums;

namespace SRcsharp.Library
{
    public class SpatialReasoner
    {

        #region Settings
        private SpatialAdjustment _adjustment;

		public SpatialAdjustment Adjustment
		{
			get { return _adjustment; }
			set { _adjustment = value; }
		}

		private SpatialPredicatedCategories _deduce;

		public SpatialPredicatedCategories Deduce
		{
			get { return _deduce; }
			set { _deduce = value; }
		}


		private CGVector _north = new CGVector(0.0f, -1.0f);

		public CGVector North
		{
			get { return _north; }
			set { _north = value; }
		}

        #endregion


        #region Data
        private SpatialObject[] _objects;

		public SpatialObject[] Objects
		{
			get { return _objects; }
			set { _objects = value; }
		}

		private SpatialObject? _observer;

		public SpatialObject? Observer
		{
			get { return _observer; }
			set { _observer = value; }
		}

		private Dictionary<int,SpatialRelation> _relMap;

		public Dictionary<int,SpatialRelation> RelMap
		{
			get { return _relMap; }
			set { _relMap = value; }
		}

		private SpatialInference[] _chain;

		public SpatialInference[] Chain
		{
			get { return _chain; }
			set { _chain = value; }
		}

		private Dictionary<string,object> _base;

		public Dictionary<string,object> Base
		{
			get { return _base; }
			set { _base = value; }
		}

		private DateTime _snapTime = DateTime.Now;

		public DateTime SnapTime
		{
			get { return _snapTime; }
			set { _snapTime = value; }
		}

		#endregion


	}
}
