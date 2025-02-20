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

		public enum SpatialReasonerBaseContentType { Undefined, Objects, Snaptime, Data, Chain }

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
		private List<SpatialObject> _objects;

		public List<SpatialObject> Objects
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

		private Dictionary<int, SpatialRelation> _relMap;

		public Dictionary<int, SpatialRelation> RelMap
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

		private Dictionary<SpatialReasonerBaseContentType, object> _base;

		public Dictionary<SpatialReasonerBaseContentType, object> Base
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

		#region Logging

		private string _pipeline;

		public string Pipeline
		{
			get { return _pipeline; }
			set { _pipeline = value; }
		}

		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private string _description;

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		private int _logCount;

		public int LogCount
		{
			get { return _logCount; }
			set { _logCount = value; }
		}

		private Uri? _logFolder;

		public Uri? LogFolder
		{
			get { return _logFolder; }
			set { _logFolder = value; }
		}


		#endregion

		public SpatialReasoner()
		{
			InitBase();
		}

		private void InitBase(bool reset = false)
		{
			if (_base == null || reset)
				_base = new Dictionary<SpatialReasonerBaseContentType, object>();
			if (!_base.ContainsKey(SpatialReasonerBaseContentType.Objects) || reset)
				_base.Add(SpatialReasonerBaseContentType.Objects, null);
			if (!_base.ContainsKey(SpatialReasonerBaseContentType.Snaptime) || reset)
				_base.Add(SpatialReasonerBaseContentType.Snaptime, null);
			if (!_base.ContainsKey(SpatialReasonerBaseContentType.Data) || reset)
				_base.Add(SpatialReasonerBaseContentType.Data, null);
			if (!_base.ContainsKey(SpatialReasonerBaseContentType.Chain) || reset)
				_base.Add(SpatialReasonerBaseContentType.Chain, null);
		}

		public void Load(List<SpatialObject> spatialObjs)
		{
			if (spatialObjs != null)
			{
				_objects = spatialObjs;
			}
			_observer = null;
			_relMap = new Dictionary<int, SpatialRelation>();

			InitBase(true);

			if (_objects.Count > 0)
			{
				List<int> indices = new List<int>();
				for (int i = 0; i < _objects.Count; i++)
				{
					indices.Add(i);
				}

				List<object> objList = new List<object>();
				foreach (var idx in indices)
				{
					_objects[idx].Context = this;
					objList.Add(_objects[idx].AsDict());
					if (_objects[idx].Observing)
					{
						_observer = _objects[idx];
					}
				}
				_base[SpatialReasonerBaseContentType.Objects] = objList;
			}

			var snapTime = DateTime.Now;
			_base[SpatialReasonerBaseContentType.Snaptime] = snapTime;
		}

		public void Load(Dictionary<string, object> objs)
		{
			_base[SpatialReasonerBaseContentType.Objects] = objs;
			SyncToObjects();
			_base[SpatialReasonerBaseContentType.Snaptime] = _snapTime.ToString();
			_snapTime = DateTime.Now;
		}

		public void Load(string json)
		{
			throw new NotImplementedException();
		}

		public SpatialObject? GetObjectById(string id)
		{
			var found = _objects.Where(obj => obj.Id == id);
			if (found.Count() > 1)
				throw new Exception("More than one Spatial Object found with Id: " + id);
			return found.FirstOrDefault();
		}

		public int? IndexOf(string id)
		{
			return _objects.ToList().FindIndex(obj => obj.Id == id);
		}

		public void SetData(string key, object value)
		{
			if (_base[SpatialReasonerBaseContentType.Data] == null)
				_base[SpatialReasonerBaseContentType.Data] = new Dictionary<string, object>();
			var data = ((Dictionary<string, object>)_base[SpatialReasonerBaseContentType.Data]);
			if (!data.ContainsKey(key))
				data[key] = value;
			else
				data.Add(key, value);
		}

		public void SyncToObjects()
		{
			_objects = new List<SpatialObject>();
			_observer = null;
			_relMap = new Dictionary<int, SpatialRelation>();

			var objectList = (List<Dictionary<string, object>>)_base[SpatialReasonerBaseContentType.Objects];
			foreach (var objDict in objectList)
			{
				var obj = new SpatialObject(objDict["id"].ToString());
				obj.FromAny(objDict);
				_objects.Add(obj);

				if (obj.Observing)
				{
					_observer = obj;
				}
			}
		}

		public Dictionary<SpatialReasonerBaseContentType, object> TakeSnapshot()
		{
			return _base;
		}

		public void LoadSnapshot(Dictionary<SpatialReasonerBaseContentType, object> snapshot)
		{
			_base = snapshot;
			SyncToObjects();
		}

		public void Record(SpatialInference inference)
		{
			_chain.Append(inference);
			if (_base[SpatialReasonerBaseContentType.Chain] == null)
				_base[SpatialReasonerBaseContentType.Chain] = new Dictionary<string, object>();

			((Dictionary<string, object>)_base[SpatialReasonerBaseContentType.Chain]).Concat(inference.AsDict());
		}

		public int[] Backtrace()
		{
			foreach(var inf in _chain.Reverse())
			{
				if (inf.IsManipulating())
					return inf.Input;
			}
			return new int[0];
		}

		public List<SpatialRelation> RelationsWith(int objIdx, string predicate)
		{
			throw new NotImplementedException();
		}


	}
}
