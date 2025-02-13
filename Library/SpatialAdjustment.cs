using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SRcsharp.Library.SREnums;

namespace SRcsharp.Library
{
    public struct SpatialAdjustment
    {

		private float _maxGap = 0.02f;

		public float MaxGap
		{
			get { return _maxGap; }
			set { _maxGap = value; }
		}

		private double _maxAngleDelta = 0.05 * Math.PI;

		public double MaxAngleDelta
		{
			get { return _maxAngleDelta; }
			set { _maxAngleDelta = value; }
		}

		private SectorSchema _sectorSchema = SectorSchema.Nearby;

		public SectorSchema SectorSchema
		{
			get { return _sectorSchema; }
			set { _sectorSchema = value; }
		}

		private float _sectorFactor;

		public float SectorFactor
		{
			get { return _sectorFactor; }
			set { _sectorFactor = value; }
		}

		private float _sectorLimit;

		public float SectorLimit
		{
			get { return _sectorLimit; }
			set { _sectorLimit = value; }
		}

		private NearbySchema _nearbySchema;

		public NearbySchema NearbySchema
		{
			get { return _nearbySchema; }
			set { _nearbySchema = value; }
		}

		private float _nearbyFactor;

		public float NearbyFactor
		{
			get { return _nearbyFactor; }
			set { _nearbyFactor = value; }
		}

		private float _nearbyLimit;

		public float NearbyLimit
		{
			get { return _nearbyLimit; }
			set { _nearbyLimit = value; }
		}

		private float _longRatio=4.0f;
		public float LongRatio
		{
			get { return _longRatio; }
			set { _longRatio = value; }
		}

		private float _thinRation = 10.0f;

		public float ThinRation
		{
			get { return _thinRation; }
			set { _thinRation = value; }
		}

		public double Yaw { 
			get { return _maxAngleDelta * 180.0 / Math.PI; } 
			set { _maxAngleDelta = value * Math.PI / 180.0f; } 
		}

		public SpatialAdjustment(float gap = 0.02f, double angle = 0.05*Math.PI, SectorSchema sectorSchema = SectorSchema.Nearby, float sectorFactor = 1.0f, float sectorLimit = 2.5f, NearbySchema nearbySchema = NearbySchema.Circle, float nearbyFactor = 2.0f, float nearbyLimit = 2.5f )
		{
			_maxGap = gap;
			_maxAngleDelta = angle;
			_sectorSchema = sectorSchema;
			_sectorFactor = sectorFactor;
			_sectorLimit = sectorLimit;
			_nearbySchema = nearbySchema;
			_nearbyFactor = nearbyFactor;
			_nearbyLimit = nearbyLimit;
        }

		public static SpatialAdjustment DefaultAdjustment = new SpatialAdjustment();
		public static SpatialAdjustment TightAdjustment = new SpatialAdjustment(gap: 0.002f, angle: 0.01 * Math.PI, sectorFactor: 0.5f);



	}
}
