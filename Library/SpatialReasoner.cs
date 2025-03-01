using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using static SRcsharp.Library.SREnums;

namespace SRcsharp.Library
{
    public class SpatialReasoner : IEnumerable<SpatialObject>
    {

        public enum SpatialReasonerBaseContentType { Undefined, Objects, Snaptime, Data, Chain }

        #region Settings
        private SpatialAdjustment _adjustment;

        public SpatialAdjustment Adjustment
        {
            get { return _adjustment; }
            set { _adjustment = value; }
        }

        private SpatialPredicatedCategories _deduceCategories;

        public SpatialPredicatedCategories DeduceCategories
        {
            get { return _deduceCategories; }
            set { _deduceCategories = value; }
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

        private SpatialObject _observer;

        public SpatialObject Observer
        {
            get { return _observer; }
            set { _observer = value; }
        }

        private Dictionary<int, List<SpatialRelation>> _relMap;

        public Dictionary<int, List<SpatialRelation>> RelMap
        {
            get { return _relMap; }
            set { _relMap = value; }
        }

        private List<SpatialInference> _chain;

        public List<SpatialInference> Chain
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

        private string? _logFolder;

        public string? LogFolder
        {
            get { return _logFolder; }
            set { _logFolder = value; }
        }

        private string _logBaseFileName = "logBase.json";

        public string LogBaseFileName
        {
            get { return _logBaseFileName; }
            set { _logBaseFileName = value; }
        }



        #endregion

        public List<SpatialObject> Result { get { return GetResult(); } }

        //public List<Dictionary<string, object>> BaseObjects { get { return (List<Dictionary<string, object>>)_base[SpatialReasonerBaseContentType.Objects]; } }

        public List<Dictionary<string, object>> BaseData { get { return (List<Dictionary<string, object>>)_base[SpatialReasonerBaseContentType.Data]; } }


        public static SpatialReasoner Create()
        {
            return new SpatialReasoner();
        }


        public SpatialReasoner()
        {
            _adjustment = new SpatialAdjustment();
            _chain = new List<SpatialInference>();
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


        public SpatialReasoner Load(SpatialObject obj)
        {
            return Load(new List<SpatialObject> { obj });
        }

        public SpatialReasoner Load(List<SpatialObject> spatialObjs = null)
        {
            if (spatialObjs != null)
            {
                _objects = spatialObjs;
            }
            _observer = null;
            _relMap = new Dictionary<int, List<SpatialRelation>>();

            InitBase(true);

            if (_objects.Count > 0)
            {
                List<int> indices = new List<int>();
                for (int i = 0; i < _objects.Count; i++)
                {
                    indices.Add(i);
                }

                //List<Dictionary<string, object>> objList = new List<Dictionary<string, object>>();
                foreach (var idx in indices)
                {
                    _objects[idx].Context = this;
                    //var dict = _objects[idx].AsDict();
                    //objList.Add(dict);
                    if (_objects[idx].Observing)
                    {
                        _observer = _objects[idx];
                    }
                }
                //_base[SpatialReasonerBaseContentType.Objects] = objList;
            }

            var snapTime = DateTime.Now;
            _base[SpatialReasonerBaseContentType.Snaptime] = snapTime;

            return this;
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
            _relMap = new Dictionary<int, List<SpatialRelation>>();

            var objectList = (List<Dictionary<string, object>>)_base[SpatialReasonerBaseContentType.Objects];
            foreach (var objDict in objectList)
            {
                var obj = new SpatialObject(objDict["Id"].ToString());
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
            _chain.Add(inference);
            if (_base[SpatialReasonerBaseContentType.Chain] == null)
                _base[SpatialReasonerBaseContentType.Chain] = new List<Dictionary<string, object>>();

            ((List<Dictionary<string, object>>)_base[SpatialReasonerBaseContentType.Chain]).Add(inference.AsDict());
        }

        public List<int> Backtrace(int steps)
        {
            var cnt = 0;
            foreach (var inf in _chain.ToArray().Reverse())
            {
                cnt++;
                if (inf.IsManipulating())
                    if(cnt == steps)
                        return inf.Input; 
            }
            return null;
        }


        public bool Run(string pipeline, bool doLogging = true)
        {
            _pipeline = pipeline;
            _logCount = 0;
            _chain = new List<SpatialInference>();
            _base[SpatialReasonerBaseContentType.Chain] = new List<Dictionary<string, object>>();

            var list = pipeline.Split(new string[] { "|", "->" }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(op => op.Trim())
                               .ToList();

            var indices = Enumerable.Range(0, _objects.Count).ToList();

            foreach (var op in list)
            {
                if (op.ToLower().StartsWith("log("))
                {
                    var startIdx = op.IndexOf('(') + 1;
                    var endIdx = op.LastIndexOf(')');
                    if(doLogging)
                        Log(op.Substring(startIdx, endIdx - startIdx));
                }
                else if (op.ToLower().StartsWith("adjust("))
                {
                    var startIdx = op.IndexOf('(') + 1;
                    var endIdx = op.LastIndexOf(')');
                    var ok = Adjust(op.Substring(startIdx, endIdx - startIdx));
                    if (!ok)
                    {
                        LogError();
                        break;
                    }
                }
                else if (op.ToLower().StartsWith("deduce("))
                {
                    var startIdx = op.IndexOf('(') + 1;
                    var endIdx = op.LastIndexOf(')');
                    Deduce(op.Substring(startIdx, endIdx - startIdx));
                }
                else
                {
                    var inference = new SpatialInference((_chain.Any() ? _chain.Last().Output : indices), op, this);
                    Record(inference);
                    if (inference.HasFailed())
                    {
                        LogError();
                        break;
                    }
                }
            }
            //SyncToObjects();

            if (_chain.Any())
            {
                return _chain.Last().Succeeded;
            }
            else if (pipeline.Contains("log("))
            {
                return true;
            }

            return false;
        }

        public SpatialReasoner Filter(string condition)
        {
            var inference = CreateInference();
            inference.Filter(condition);
            ChainedOperationCompleted(inference);
            return this;
        }

        //public SpatialReasoner Filter(Func<Dictionary<string,object>,bool> filter)
        //{
        //    filter.Invoke()
        //}

        public SpatialReasoner Pick(string relations)
        {
            var inference = CreateInference();
            inference.Pick(relations);
            ChainedOperationCompleted(inference);
            return this;
        }

        public SpatialReasoner Select(string terms)
        {
            var inference = CreateInference();
            inference.Select(terms);
            ChainedOperationCompleted(inference);
            return this;
        }

        public SpatialReasoner Sort(string attribute)
        {
            var inference = CreateInference();
            inference.Sort(attribute);
            ChainedOperationCompleted(inference);
            return this;
        }

        public SpatialReasoner Slice(string range)
        {
            var inference = CreateInference();
            inference.Slice(range);
            ChainedOperationCompleted(inference);
            return this;
        }

        public SpatialReasoner Produce(string terms)
        {
            var inference = CreateInference();
            inference.Produce(terms);
            ChainedOperationCompleted(inference);
            return this;
        }

        public SpatialReasoner Calc(string assignments)
        {
            var inference = CreateInference();
            inference.Calc(assignments);
            ChainedOperationCompleted(inference);
            return this;
        }

        public SpatialReasoner CLog()
        {
            Console.WriteLine(string.Join('\n', Result));
            return this;
        }

        private SpatialInference CreateInference()
        {
            return SpatialInference.Create((_chain.Any() ? _chain.Last().Output : Enumerable.Range(0, _objects.Count).ToList()), this);
        }

        private bool ChainedOperationCompleted(SpatialInference inference)
        {
            Record(inference);
            if (inference.HasFailed())
            {
                LogError();
                return false;
            }
            return true;
        }

        public List<SpatialObject> GetResult()
        {
            var list = new List<SpatialObject>();
            if (_chain.Any())
            {
                list = _chain.Last().Output.Select(idx => _objects[idx]).ToList();
            }
            return list;
        }

        public void LogError()
        {
            Console.WriteLine(_chain.Last()?.Error);
        }

        public static void PrintRelation(List<SpatialRelation> relations)
        {
            relations.ToList().ForEach(rel => Console.WriteLine(rel.ToString()));
        }

        public List<SpatialRelation> RelationsOf(int idx)
        {
            if (_relMap.ContainsKey(idx))
                return _relMap[idx];

            var relations = new List<SpatialRelation>();
            _objects.Where(sub => sub != _objects[idx]).ToList().ForEach(sub => relations.AddRange(_objects[idx].Relate(sub, true)));

            _relMap.Add(idx, relations);

            return relations;
        }

        public List<SpatialRelation> RelationsWith(int objIdx, string predicate)
        {
            var rels = new List<SpatialRelation>();
            if (objIdx >= 0)
                rels = RelationsOf(objIdx).Where(rel => rel.Predicate.IsDefined() && rel.Predicate.RawValue.ToLower() == predicate.ToLower()).ToList();
            return rels;
        }

        public bool DoesSubjectHasRelationOfPredWithObject(SpatialObject subject, string predicate, int objIdx)
        {
            var relations = RelationsOf(objIdx);

            return relations.Any(rel => rel.Subject == subject && rel.Predicate.IsDefined() && rel.Predicate.RawValue.ToLower() == predicate.ToLower());
        }

        public bool Adjust(string settings)
        {
            string error = "";
            var list = settings.Split(';')
                               .Select(s => s.Trim())
                               .ToList();

            foreach (var setting in list)
            {
                var parts = setting.Split(' ');
                string first = parts.Length > 0 ? parts[0] : "";
                string second = parts.Length > 1 ? parts[1] : "";
                string number = parts.Length > 2 ? parts[2] : "";

                switch (first)
                {
                    case "max":
                        switch (second)
                        {
                            case "gap":
                                if (!string.IsNullOrEmpty(number))
                                {
                                    if (float.TryParse(number, out float val))
                                    {
                                        Adjustment.MaxGap = val;
                                    }
                                    else
                                    {
                                        error = $"Invalid max gap value: {number}";
                                    }
                                }
                                break;

                            case "angle":
                            case "delta":
                                if (!string.IsNullOrEmpty(number))
                                {
                                    if (float.TryParse(number, out float val))
                                    {
                                        Adjustment.MaxAngleDelta = val;
                                    }
                                    else
                                    {
                                        error = $"Invalid max angle value: {number}";
                                    }
                                }
                                break;

                            default:
                                error = $"Unknown max setting: {second}";
                                break;
                        }
                        break;

                    case "sector":
                        bool setFactor = true;
                        switch (second)
                        {
                            case "fixed":
                                Adjustment.SectorSchema = SectorSchema.Fixed;
                                break;

                            case "dimension":
                                Adjustment.SectorSchema = SectorSchema.Dimension;
                                break;

                            case "perimeter":
                                Adjustment.SectorSchema = SectorSchema.Perimeter;
                                break;

                            case "area":
                                Adjustment.SectorSchema = SectorSchema.Area;
                                break;

                            case "nearby":
                                Adjustment.SectorSchema = SectorSchema.Nearby;
                                break;

                            case "factor":
                                setFactor = true;
                                break;

                            case "limit":
                                setFactor = false;
                                if (!string.IsNullOrEmpty(number))
                                {
                                    if (float.TryParse(number, out float val))
                                    {
                                        Adjustment.SectorLimit = val;
                                    }
                                    else
                                    {
                                        error = $"Invalid sector limit value: {number}";
                                    }
                                }
                                break;

                            default:
                                error = $"Unknown sector setting: {second}";
                                break;
                        }

                        if (setFactor && !string.IsNullOrEmpty(number))
                        {
                            if (float.TryParse(number, out float val))
                            {
                                Adjustment.SectorLimit = val;
                            }
                            else
                            {
                                error = $"Invalid sector limit value: {number}";
                            }
                        }
                        break;

                    case "nearby":
                        setFactor = true;
                        switch (second)
                        {
                            case "fixed":
                                Adjustment.NearbySchema = NearbySchema.Fixed;
                                break;

                            case "circle":
                                Adjustment.NearbySchema = NearbySchema.Circle;
                                break;

                            case "sphere":
                                Adjustment.NearbySchema = NearbySchema.Sphere;
                                break;

                            case "perimeter":
                                Adjustment.NearbySchema = NearbySchema.Perimeter;
                                break;

                            case "area":
                                Adjustment.NearbySchema = NearbySchema.Area;
                                break;

                            case "factor":
                                setFactor = true;
                                break;

                            case "limit":
                                setFactor = false;
                                if (!string.IsNullOrEmpty(number))
                                {
                                    if (float.TryParse(number, out float val))
                                    {
                                        Adjustment.NearbyLimit = val;
                                    }
                                    else
                                    {
                                        error = $"Invalid nearby limit value: {number}";
                                    }
                                }
                                break;

                            default:
                                error = $"Unknown nearby setting: {second}";
                                break;
                        }

                        if (setFactor && !string.IsNullOrEmpty(number))
                        {
                            if (float.TryParse(number, out float val))
                            {
                                Adjustment.NearbyFactor = val;
                            }
                            else
                            {
                                error = $"Invalid nearby factor value: {number}";
                            }
                        }
                        break;

                    case "long":
                        if (second == "ratio" && !string.IsNullOrEmpty(number))
                        {
                            if (float.TryParse(number, out float val))
                            {
                                Adjustment.LongRatio = val;
                            }
                            else
                            {
                                error = $"Invalid long ratio value: {number}";
                            }
                        }
                        break;

                    case "thin":
                        if (second == "ratio" && !string.IsNullOrEmpty(number))
                        {
                            if (float.TryParse(number, out float val))
                            {
                                Adjustment.ThinRatio = val;
                            }
                            else
                            {
                                error = $"Invalid thin ratio value: {number}";
                            }
                        }
                        break;

                    default:
                        error = $"Unknown adjust setting: {first}";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error: {error}");
                var errorState = new SpatialInference(new List<int>(), $"adjust({settings})", this)
                {
                    Error = error
                };
                return false;
            }

            return true;
        }

        public void Deduce(string categories)
        {
            if (categories.Contains("topo"))
                _deduceCategories |= SpatialPredicatedCategories.Topology;
            if (categories.Contains("connect"))
                _deduceCategories |= SpatialPredicatedCategories.Connectivity;
            if (categories.Contains("compar"))
                _deduceCategories |= SpatialPredicatedCategories.Comparability;
            if (categories.Contains("simil"))
                _deduceCategories |= SpatialPredicatedCategories.Similarity;
            if (categories.Contains("sector"))
                _deduceCategories |= SpatialPredicatedCategories.Sectoriality;
            if (categories.Contains("visib"))
                _deduceCategories |= SpatialPredicatedCategories.Visbility;
            if (categories.Contains("geo"))
                _deduceCategories |= SpatialPredicatedCategories.Geography;
        }


        public void Log(string predicates)
        {
            if (_logFolder == null)
            {
                var urls = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", SearchOption.TopDirectoryOnly);
                _logFolder = urls.Length > 0 ? urls[0] : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            _logCount += 1;
            var allIndices = Enumerable.Range(0, _objects.Count).ToList();
            var indices = _chain.Any() ? _chain.Last().Output : allIndices;
            var list = predicates.Split(' ')
                                 .Select(p => p.Trim())
                                 .ToList();

            if (list.Contains("base"))
            {
                list.RemoveAll(p => p == "base");
                LogBase();
            }

            if (list.Contains("3D"))
            {
                list.RemoveAll(p => p == "3D");
                Log3D();
            }

            string md = "# ";
            string str = !string.IsNullOrEmpty(_name) ? _name : "Spatial Reasoning Log";
            string mmdObjs = "";
            string mmdRels = "";
            string mmdContacts = "";
            string rels = "";

            md += str + "\n";
            str = !string.IsNullOrEmpty(_description) ? _description : "";
            md += str + "\n## Inference Pipeline\n\n```\n" + _pipeline + "\n```\n\n## Inference Chain\n\n```\n";

            foreach (var chainItem in _chain)
            {
                md += "| " + chainItem.Operation + "  ->  " + string.Join(",", chainItem.Output) + "\n";
            }

            md += "```\n\n## Spatial Objects\n\n### Fact Base\n\n";

            foreach (var i in allIndices)
            {
                str = _objects[i].Id;
                md += $"{i}.  __{str}__: {_objects[i].Describe()}\n";
            }

            md += "\n\n### Resulting Objects (Output)\n\n";

            foreach (var i in indices)
            {
                str = _objects[i].Id;
                mmdObjs += $"    {str}\n";
                md += $"{i}.  __{str}__: {_objects[i].Describe()}\n";

                foreach (var relation in RelationsOf(i))
                {
                    bool doAdd = list.Count > 0 ? list.Contains(relation.Predicate.ToString()) : true;

                    if (doAdd)
                    {
                        string leftLink = " -- ";
                        if (SpatialTerms.Instance.IsSymmetric(relation.Predicate))
                        {
                            leftLink = " <-- ";
                            var searchBy = $"{relation.Object.Id}{leftLink}{relation.Predicate} --> {relation.Subject.Id}";
                            if (mmdRels.Contains(searchBy))
                            {
                                doAdd = false;
                            }
                        }

                        if (doAdd)
                        {
                            mmdRels += $"    {relation.Subject.Id}{leftLink}{relation.Predicate} --> {relation.Object.Id}\n";
                        }
                    }

                    if (relation.Predicate.IsDefined(SpatialPredicate.SpatialPredicateTypes.Contacts))
                    {
                        bool doAddContact = true;
                        string leftLink = " -- ";

                        if (relation.Predicate.IsDefined(SpatialPredicate.SpatialPredicateTypes.Contacts) && relation.Predicate.GetDefinedPredicateValueName() == "By")
                        {
                            leftLink = " <-- ";
                            var searchBy = $"{relation.Object.Id}{leftLink}{relation.Predicate} --> {relation.Subject.Id}";
                            if (mmdContacts.Contains(searchBy))
                            {
                                doAddContact = false;
                            }
                        }

                        if (doAddContact)
                        {
                            mmdContacts += $"    {relation.Subject.Id}{leftLink}{relation.Predicate} --> {relation.Object.Id}\n";
                        }
                    }
                    rels += $"* {relation.Description}\n";
                }
            }

            if (!string.IsNullOrEmpty(mmdRels))
            {
                md += "\n## Spatial Relations Graph\n\n";
                md += "```mermaid\ngraph LR;\n" + mmdObjs + mmdRels + "```\n";
            }

            if (!string.IsNullOrEmpty(mmdContacts))
            {
                md += "\n## Connectivity Graph\n\n";
                md += "```mermaid\ngraph TD;\n" + mmdContacts + "```\n";
            }

            md += "\n## Spatial Relations\n\n" + rels + "\n";

            bool multipleLogs = _pipeline.Split(new string[] { "log(" }, StringSplitOptions.None).Length > 2;
            try
            {
                string counterStr = multipleLogs ? _logCount.ToString() : "";
                var fileURL = Path.Combine(_logFolder, "log" + counterStr + ".md");
                File.WriteAllText(fileURL, md);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void LogBase()
        {
            var fileUrl = Path.Combine(_logFolder, _logBaseFileName);
            var data = JsonConvert.SerializeObject(_base);
            File.WriteAllText(fileUrl, data);
        }

        public void Log3D()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SpatialObject> GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

    }

    //public static class SpatialReasonerExtensions
    //{ 

    //    public static IEnumerable<SpatialObject> Pick(this IEnumerable<SpatialObject> so)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}



}
