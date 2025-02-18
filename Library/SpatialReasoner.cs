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

        #endregion


    }
}
