

namespace SRcsharp.Library
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using static SRcsharp.Library.SpatialInference;

    public class SpatialInference
    {
        public enum FilterOperations { None, Equals, DoubleEquals, Smaller, Greater, SmallerEquals, GreaterEquals, NotEquals, NotEquals2 }; //Ternary operations
        public readonly string[] FilterOperationLiterals = new string[] { "=", "==", "<", ">", "<=", ">=", "!=", "<>" };
        public enum BooleanOperations { None, AND, OR, XOR, NOT};
        public readonly string[] BooleanOperationLiterals = new string[] { "AND", "OR", "XOR", "NOT", "!"};

        private List<int> _input;
        private List<int> _output;
        private string _operation;
        private bool _succeeded;
        private string _error;
        private SpatialReasoner _fact;

        public List<int> Input { get => _input; set => _input = value; }
        public List<int> Output { get => _output; set => _output = value; }
        public string Operation { get => _operation; set => _operation = value; }
        public bool Succeeded { get => _succeeded; set => _succeeded = value; }
        public string Error { get => _error; set => _error = value; }
        public SpatialReasoner Fact { get => _fact; set => _fact = value; }

        public static SpatialInference Create(List<int> input, string operation, SpatialReasoner fact)
        {
            return new SpatialInference(input, operation, fact);
        }

        public static SpatialInference Create(List<int> input, SpatialReasoner fact)
        {
            return new SpatialInference(input, fact);
        }

        public SpatialInference(List<int> input, SpatialReasoner fact)
        {
            _input = input;
            _fact = fact;
            _output = new List<int>();
        }

        public SpatialInference(List<int> input, string operation, SpatialReasoner fact, bool run = true)
        {
            _input = input;
            _operation = operation;
            _fact = fact;
            _output = new List<int>();
            
            int endIdx = operation.Length - 1;

            if (operation.ToLower().StartsWith("filter("))
            {
                string condition = operation.Substring(7, endIdx - 7);
                if(run)
                    Filter(condition);
            }
            else if (operation.ToLower().StartsWith("pick("))
            {
                string relations = operation.Substring(5, endIdx - 5);
                if (run)
                    Pick(relations);
            }
            else if (operation.ToLower().StartsWith("select("))
            {
                string terms = operation.Substring(7, endIdx - 7);
                if (run)
                    Select(terms);
            }
            else if (operation.ToLower().StartsWith("sort("))
            {
                string attribute = operation.Substring(5, endIdx - 5);
                if (run)
                    Sort(attribute);
            }
            else if (operation.ToLower().StartsWith("slice("))
            {
                string range = operation.Substring(6, endIdx - 6);
                if (run)
                    Slice(range);
            }
            else if (operation.ToLower().StartsWith("produce("))
            {
                string terms = operation.Substring(8, endIdx - 8);
                if (run)
                    Produce(terms);
            }
            else if (operation.ToLower().StartsWith("calc("))
            {
                string assignments = operation.Substring(5, endIdx - 5);
                if (run)
                    Calc(assignments);
            }
            else if (operation.ToLower().StartsWith("map("))
            {
                string assignments = operation.Substring(4, endIdx - 4);
                if (run)
                    Map(assignments);
            }
            else if (operation.ToLower().StartsWith("reload("))
            {
                Reload();
            }
            else  {
                _error = "Unknown inference operation: "+operation;
            }
        }

        private void Add(int index)
        {
            if (!Output.Contains(index))
            {
                Output.Add(index);
            }
        }

        public void Filter(string condition)
        {
            FilterInternal(Input,condition).ForEach(i => Add(i));
        }

        private List<int> FilterInternal(List<int> indices, string condition)
        {

            var adds = new List<int>();

            var result = false;

            foreach (var i in indices)
            {
                var so = Fact.Objects[i];
                var cond = ReplaceVariables(condition, so);
                cond = cond.Replace("==", "=");
                result = Evaluator.Eval(cond);
                if (result)
                {
                    adds.Add(i);
                }
            }


            Succeeded = true;
            return adds;
        }

        private string ReplaceVariables(string condition, List<SpatialObject> so)
        {
            foreach (var dict in so)
            {
                condition = ReplaceVariables(condition, dict);
            }
            return condition;
        }

        private string ReplaceVariables(string condition, SpatialObject so)
        {
            foreach (var kvp in SpatialObject.AllAttributes)
            {
                if (condition.Contains(kvp.Key.ToLower()))
                { 

                    var obj = so.GetPropertyValue(kvp.Key);

                    if (SpatialObject.StringAttributes.ContainsKey(kvp.Key))
                        condition = condition.Replace(kvp.Key, "'" + obj.ToString() + "'", StringComparison.OrdinalIgnoreCase);
                    else
                        condition = condition.Replace(kvp.Key, obj.ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }
            return condition;
        }


        public void Pick(string relations)
        {
            var predicates = relations.Keywords().Where(k => !BooleanOperationLiterals.Contains(k.ToUpper()));

            foreach (var i in Input)
            {
                for (int j = 0; j < Fact.Objects.Count; j++)
                {
                    string cond = relations;
                    if (i != j)
                    {
                        foreach (var predicate in predicates)
                        {
                            if (Fact.DoesSubjectHasRelationOfPredWithObject(Fact.Objects[j], predicate, i))
                            {
                                cond = cond.Replace(predicate, "true");
                            }
                            else
                            {
                                cond = cond.Replace(predicate, "false");
                            }
                        }
                        bool result = Evaluator.Eval(cond);
                        if (result)
                        {
                            Add(j);
                        }
                    }
                }
            }
            Succeeded = Output.Count > 0;
        }

        public void Select(string terms)
        {
            var list = terms.Split('?').Select(term => term.Trim()).ToArray();
            if (list.Length != 2)
            {
                Error = "Invalid select query";
                return;
            }
            string conditions = list[1];
            string relations = list[0];
            var predicates = relations.Keywords();
            //var baseObjects = Fact.BaseObjects;

            foreach (var i in Input)
            {
                for (int j = 0; j < Fact.Objects.Count; j++)
                {
                    string cond = relations;
                    if (i != j)
                    {
                        foreach (var predicate in predicates)
                        {
                            if (Fact.DoesSubjectHasRelationOfPredWithObject(Fact.Objects[j], predicate, i))
                            {
                                cond = cond.Replace(predicate, "true");
                            }
                            else
                            {
                                cond = cond.Replace(predicate, "false");
                            }
                        }

                        bool result = Evaluator.Eval(cond);
                        if (result)
                        {
                            var result2 = FilterInternal(new List<int>() { j }, conditions).Any();
                            if (result2)
                            {
                                Add(i);
                            }
                        }
                    }
                }
            }
            Succeeded = Output.Count > 0;
        }

        public void Produce(string terms)
        {
            Console.WriteLine(terms);
            var list = terms.Split(':').Select(x => x.Trim()).ToList();
            var assignments = "";
            var rule = list[0];

            if (list.Count > 1)
            {
                assignments = list[1];
            }

            var indices = new List<int>(); // new produced object indices
            var newObjects = new List<SpatialObject>();

            switch (rule)
            {
                case "group":
                case "aggregate":
                    if (Input.Count > 0)
                    {
                        
                        var inputObjects = new List<SpatialObject>(Fact.Objects);

                        var sortedObjects = inputObjects.OrderByDescending(o => o.Volume).ToList();
                        var largestObject = sortedObjects.FirstOrDefault();
                        float minY = 0f;
                        float maxY = largestObject.Height;
                        float minX = -largestObject.Width / 2f;
                        float maxX = largestObject.Width / 2f;
                        float minZ = -largestObject.Depth / 2f;
                        float maxZ = largestObject.Depth / 2f;
                        var groupId = "group:" + largestObject.Id;

                        for (int j = 1; j < sortedObjects.Count; j++)
                        {
                            var localPts = largestObject.ConvertIntoLocal(sortedObjects[j].CalcPoints());
                            foreach (var pt in localPts)
                            {
                                minX = Math.Min(minX, pt.X);
                                maxX = Math.Max(maxX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxY = Math.Max(maxY, pt.Y);
                                minZ = Math.Min(minZ, pt.Z);
                                maxZ = Math.Max(maxZ, pt.Z);
                            }
                            groupId += "+" + sortedObjects[j].Id;
                        }

                        var w = maxX - minX;
                        var h = maxY - minY;
                        var d = maxZ - minZ;
                        var dx = minX + w / 2f;
                        var dy = minY / 2f;
                        var dz = minZ + d / 2f;
                        var objIdx = (int)Fact.IndexOf(groupId);
                        var group = objIdx < 0 ? new SpatialObject(groupId) : Fact.Objects[objIdx];

                        group.Position = largestObject.Position;
                        group.RotShift(-largestObject.Angle, dx, dy, dz);
                        group.Angle = largestObject.Angle;
                        group.Width = w;
                        group.Height = h;
                        group.Depth = d;
                        group.Cause = SREnums.ObjectCause.RuleProduced;

                        if (objIdx < 0)
                        {
                            newObjects.Add(group);
                            indices.Add(Fact.Objects.Count);
                            Fact.Objects.Add(group);
                        }
                    }
                    break;

                case "copy":
                case "duplicate":
                    foreach (var i in Input)
                    {
                        var copyId = "copy:" + Fact.Objects[i].Id;
                        var idx = (int)Fact.IndexOf(copyId);

                        if (idx == -1)
                        {
                            idx = Fact.Objects.Count;
                            var objIdx = (int)Fact.IndexOf(copyId);
                            var template = objIdx < 0 ? new SpatialObject(Fact.Objects[i].Id) : Fact.Objects[objIdx];
                            var copy = SpatialObject.Clone(template, copyId);
                            //copy.FromAny(Fact.Objects[i].ToAny());
                            //copy.Id = copyId;
                            copy.Cause = SREnums.ObjectCause.RuleProduced;
                            copy.Position = Fact.Objects[i].Position;
                            copy.Angle = Fact.Objects[i].Angle;

                            if (objIdx < 0)
                            {
                                newObjects.Add(copy);
                                Fact.Objects.Add(copy);
                                indices.Add(idx);
                            }
                        }
                        else
                        {
                            indices.Add(idx);
                        }
                    }
                    break;

                case "by":
                    var processedBys = new HashSet<string>();

                    foreach (var i in Input)
                    {
                        var rels = Fact.RelationsWith(i, "by");

                        foreach (var rel in rels)
                        {
                            var idx = (int)Fact.IndexOf(rel.Subject.Id);
                            if (Input.Contains(idx) && !processedBys.Contains(rel.Subject.Id + "-" + Fact.Objects[i].Id))
                            {
                                var nearest = Fact.Objects[i].Position.Nearest(rel.Subject.CalcPoints());
                                var byId = "by:" + Fact.Objects[i].Id + "-" + rel.Subject.Id;
                                var objIdx = (int)Fact.IndexOf(byId);
                                var obj = objIdx < 0 ? new SpatialObject(byId) : Fact.Objects[objIdx];

                                obj.Cause = SREnums.ObjectCause.RuleProduced;
                                obj.Position = nearest.FirstOrDefault();
                                obj.Angle = Fact.Objects[i].Angle;

                                var w = Math.Max(rel.Delta, Fact.Objects[i].Adjustment.MaxGap);
                                obj.Width = w;
                                obj.Depth = w;

                                var h = rel.Subject.Height;
                                if (nearest[0].X == nearest[1].X && nearest[0].Z == nearest[1].Z)
                                {
                                    h = nearest[1].Y - nearest[0].Y;
                                }

                                obj.Height = h;

                                if (objIdx < 0)
                                {
                                    newObjects.Add(obj);
                                    indices.Add(Fact.Objects.Count);
                                    Fact.Objects.Add(obj);
                                }

                                processedBys.Add(Fact.Objects[i].Id + "-" + rel.Subject.Id);
                            }
                        }
                    }
                    break;

                default:
                    Error += ($"Unknown {rule} rule in produce()");
                    return;
            }

            if (indices.Any())
            {
                Fact.Objects.AddRange(newObjects);
                if (!string.IsNullOrEmpty(assignments))
                {
                    Assign(assignments, indices);
                }

                _output = _input;
                foreach (var i in indices)
                {
                    if (!_output.Contains(i))
                    {
                        _output.Add(i);
                    }
                }
            }
            else
            {
                _output = _input;
            }

            Fact.Load();
            Succeeded = string.IsNullOrEmpty(Error);
        }

        public void Map(string assignments)
        {
            Assign(assignments, _input);
            Fact.Load();
            _output = _input;
            Succeeded = _output.Count > 0;
        }

        public void Assign(string assignments, List<int> indices)
        {
            //throw new NotImplementedException();

            var list = assignments.Split(';').Select(a => a.Trim()).ToArray();

            foreach (var i in indices)
            {
                var dict = new Dictionary<string, object>();
                foreach (var assignment in list)
                {
                    var kv = assignment.Split('=');
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim();
                        var expr = kv[1].Trim();

                        //var expression = new NSExpression(expr);
                        //var value = expression.ExpressionValueWith(baseObjects[i], null);
                        //var value = expr; //TODO: evaluate boolean and mathematical statements
                        var value = ReplaceVariables(expr, Fact.Objects[i]);
                        var res = new DataTable().Compute(value, ""); 
                        Fact.Objects[i].SetPropertyByName(key, res);

                        //if (value != null)
                        //{
                        //    dict[key] = value;
                        //}
                    }
                }

                //Fact.Objects[i].FromAny(dict);
            }
            //Fact.Load();
            _output = _input;
            Succeeded = _output.Count > 0;
        }

        public void Calc(string assignments)
        {
            //throw new NotImplementedException();
            var list = assignments.Split(';').Select(a => a.Trim()).ToArray();

            foreach (var assignment in list)
            {
                var kv = assignment.Split('=');
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim();
                    var expr = kv[1].Trim();

                    var value = ReplaceVariables(expr, Fact.Objects);
                    var res = new DataTable().Compute(value, "");
 

                    //var expression = new NSExpression(expr);
                    //var value = expression.ExpressionValueWith(Fact.Base, null);

                    if (res != null)
                    {
                        Fact.SetData(key, res);
                    }
                }
            }
            _output = _input;
            Succeeded = _output.Count > 0;
        }

        public void Slice(string range)
        {
            if(Input.Count <= 0) { return; }

            Console.WriteLine("slice " + range);

            var str = range.Replace("..", ".");
            var list = str.Split('.').Select(s => s.Trim()).ToArray();

            int lower = 0;
            int upper = 0;

            if (list.Length > 0)
            {
                lower = int.TryParse(list[0], out int result) ? result : 1;
                if(lower >= Input.Count)
                    lower = Input.Count;
                if (lower < 0)
                    lower += Input.Count;
                else
                    lower -= 1;
            }

            if (list.Length > 1)
            {
                upper = int.TryParse(list[1], out int result2) ? result2 : 1;
                if (upper >= Input.Count)
                    upper = Input.Count;
                if (upper < 0)
                    upper += Input.Count;
                else
                    upper -= 1;
            }
            else
            {
                upper = lower;
            }

            if (lower > upper)
            {
                (lower, upper) = (upper, lower);
            }

            //var idxRange = new Range(lower, upper);
            //Console.WriteLine(idxRange);

            Output = Input.GetRange(lower, upper-lower+1);
            Succeeded = Output.Count > 0;
        }

        public void Sort(string attribute)
        {
            if (attribute.Contains("."))
            {
                SortByRelation(attribute);
                return;
            }

            bool ascending = false;
            var inputObjects = new List<SpatialObject>();

            foreach (var i in _input)
            {
                inputObjects.Add(Fact.Objects[i]);
            }

            var list = attribute.Split(' ').Select(a => a.Trim()).ToArray();
            var prop = list[0];
            if (list.Length > 1)
            {
                if (list[1] == "<")
                {
                    ascending = true;
                }
            }

            List<SpatialObject> sortedObjects;

            if (ascending)
            {
                sortedObjects = inputObjects.OrderBy(o => o.GetPropertyValue(prop)).ToList();
            }
            else
            {
                sortedObjects = inputObjects.OrderByDescending(o => o.GetPropertyValue(prop)).ToList();
            }

            _output.Clear();
            foreach (var obj in sortedObjects)
            {
                var idx = Fact.Objects.IndexOf(obj);
                if (idx >= 0)
                {
                    Add(idx);
                }
            }
            Succeeded = _output.Count > 0;
        }

        public void SortByRelation(string param, int backtraceSteps = 1)
        {
            bool ascending = false;
            var inputObjects = new List<SpatialObject>(Fact.Objects);
            var steps = backtraceSteps;
            

            var list = param.Split(' ').Select(a => a.Trim()).ToArray();
            var attribute = list[0];
            if (list.Length > 1)
            {
                foreach(var sub in list)
                {
                    if (sub == "<")
                    {
                        ascending = true;
                    }
                    else
                    {
                        int nsteps = -1;
                        if(int.TryParse(sub, out nsteps))
                        {
                            steps = nsteps;
                        }
                    }
                }
            }
            var preIndices = Fact.Backtrace(steps);
            list = attribute.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var pred = list[0];
            var prop = list[1];

            List<SpatialObject> sortedObjects;

            if (ascending)
            {
                sortedObjects = inputObjects.OrderBy(o => o.CalcRelationValue(pred,prop, preIndices)).ToList();
            }
            else
            {
                sortedObjects = inputObjects.OrderByDescending(o => o.CalcRelationValue(pred,prop, preIndices)).ToList();
            }


            _output.Clear();
            foreach (var obj in sortedObjects)
            {
                var idx = Fact.Objects.IndexOf(obj);
                if (idx >= 0)
                {
                    Add(idx);
                }
            }

            Succeeded = _output.Count > 0;
        }

        public void Reload()
        {
            Fact.SyncToObjects();
            Fact.Load();
            Succeeded = _output.Count > 0;
        }


        public bool HasFailed()
        {
            return !string.IsNullOrEmpty(_error);
        }

        public bool IsManipulating()
        {
            return _operation.StartsWith("filter") ||
                   _operation.StartsWith("pick") ||
                   _operation.StartsWith("select") ||
                   _operation.StartsWith("produce") ||
                   _operation.StartsWith("slice");
        }

        public Dictionary<string, object> AsDict()
        {
            return new Dictionary<string, object>
                {
                    { "operation", _operation },
                    { "input", _input },
                    { "output", _output },
                    { "error", _error },
                    { "succeeded", _succeeded }
                };
        }




    }
}
