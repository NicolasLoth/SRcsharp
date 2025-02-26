

namespace SRcsharp.Library
{
    using System;
    using System.Collections.Generic;
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
            //throw new NotImplementedException();

            var adds = new List<int>();
            //var subs = condition.Split(FilterOperationLiterals, StringSplitOptions.RemoveEmptyEntries);
            //var propertyString = subs[0];
            //var valueString = subs[1];
            //var op = FilterOperations.None;
            //for (int i = 0; i < FilterOperationLiterals.Length; i++)
            //{
            //    if (condition.Contains(FilterOperationLiterals[i]))
            //        op = (FilterOperations)i + 1;
            //}
            //var boolOp = BooleanOperations.None;
            //for (int i = 0; i < BooleanOperationLiterals.Length; i++)
            //{
            //    if (condition.Contains(BooleanOperationLiterals[i]))
            //        boolOp = (BooleanOperations)i + 1;
            //}

            var result = false;

            foreach (var i in indices)
            {
                var so = Fact.BaseObjects[i];
                var cond = ReplaceVariables(condition, so);
                cond = cond.Replace("==", "=");
                result = Evaluator.CompEval(cond);
                if (result)
                {
                    adds.Add(i);
                }
            }

            //foreach (var i in indices)
            //{
            //    var so = Fact.BaseObjects[i];
            //    if (op != FilterOperations.None)
            //    {
            //        switch (op)
            //        {
            //            case FilterOperations.Equals:
            //            case FilterOperations.DoubleEquals:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) == 0);
            //                break;
            //            case FilterOperations.Smaller:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) < 0);
            //                break;
            //            case FilterOperations.Greater:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) > 0);
            //                break;
            //            case FilterOperations.SmallerEquals:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) <= 0);
            //                break;
            //            case FilterOperations.GreaterEquals:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) >= 0);
            //                break;
            //            case FilterOperations.NotEquals:
            //            case FilterOperations.NotEquals2:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) != 0);
            //                break;
            //        }
            //    }
            //    if (boolOp != BooleanOperations.None)
            //    {
            //        switch (boolOp)
            //        {
            //            case BooleanOperations.AND:
            //                result = so.Any(kvp => kvp.Key == propertyString && ((IComparable)kvp.Value).CompareTo((IComparable)valueString) == 0);
            //                break;
            //        }
            //    }
            //    if (result)
            //    {
            //        adds.Add(i);
            //    }
            //}

            Succeeded = true;
            return adds;
        }

        private string ReplaceVariables(string condition, Dictionary<string, object> so)
        {
            foreach(var kvp in so)
            {
                if (condition.Contains(kvp.Key.ToLower()))
                {
                    if(kvp.Value.GetType() == typeof(string))
                        condition = condition.Replace(kvp.Key, "'"+kvp.Value.ToString()+"'", StringComparison.OrdinalIgnoreCase);
                    else
                        condition = condition.Replace(kvp.Key, kvp.Value.ToString(), StringComparison.OrdinalIgnoreCase);
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
                        bool result = Evaluator.CompEval(cond);
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
            var baseObjects = Fact.BaseObjects;

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

                        bool result = Evaluator.CompEval(cond);
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
        }

        public void Map(string assignments)
        {
            throw new NotImplementedException();

            var list = assignments.Split(';').Select(a => a.Trim()).ToArray();
            var baseObjects = Fact.BaseObjects;

            foreach (var i in Input)
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

                        //if (value != null)
                        //{
                        //    dict[key] = value;
                        //}
                    }
                }

                Fact.Objects[i].FromAny(dict);
            }
            Fact.Load();
            Output = Input;
            Succeeded = Output.Count > 0;
        }

        public void Calc(string assignments)
        {
            throw new NotImplementedException();

            var list = assignments.Split(';').Select(a => a.Trim()).ToArray();

            foreach (var assignment in list)
            {
                var kv = assignment.Split('=');
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim();
                    var expr = kv[1].Trim();
                    //var expression = new NSExpression(expr);
                    //var value = expression.ExpressionValueWith(Fact.Base, null);

                    //if (value != null)
                    //{
                    //    Fact.SetData(key, value);
                    //}
                }
            }
            Output = Input;
            Succeeded = Output.Count > 0;
        }

        public void Slice(string range)
        {
            Console.WriteLine("slice " + range);

            var str = range.Replace("..", ".");
            var list = str.Split('.').Select(s => s.Trim()).ToArray();

            int lower = 0;
            int upper = 0;

            if (list.Length > 0)
            {
                lower = int.TryParse(list[0], out int result) ? result : 1;
                lower = Math.Clamp(lower, 0, Input.Count);
                lower = lower < 0 ? Input.Count + lower : lower - 1;
            }

            if (list.Length > 1)
            {
                upper = int.TryParse(list[1], out int result2) ? result2 : 1;
                upper = Math.Clamp(upper, 0, Input.Count);
                upper = upper < 0 ? Input.Count + upper : upper - 1;
            }
            else
            {
                upper = lower;
            }

            if (lower > upper)
            {
                (lower, upper) = (upper, lower);
            }

            var idxRange = new Range(lower, upper);
            Console.WriteLine(idxRange);

            Output = Input.GetRange(lower, upper - lower + 1);
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

            foreach (var i in Input)
            {
                inputObjects.Add(Fact.Objects[i]);
            }

            var list = attribute.Split(' ').Select(a => a.Trim()).ToArray();
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
                switch (list[0])
                {
                    case "Width":
                        sortedObjects = inputObjects.OrderBy(o => o.Width).ToList();
                        break;
                    case "Height":
                        sortedObjects = inputObjects.OrderBy(o => o.Height).ToList();
                        break;
                    // Add other cases for attributes here
                    default:
                        sortedObjects = inputObjects.OrderBy(o => o.DataValue(list[0])).ToList();
                        break;
                }
            }
            else
            {
                switch (list[0])
                {
                    case "Width":
                        sortedObjects = inputObjects.OrderByDescending(o => o.Width).ToList();
                        break;
                    case "Height":
                        sortedObjects = inputObjects.OrderByDescending(o => o.Height).ToList();
                        break;
                    // Add other cases for attributes here
                    default:
                        sortedObjects = inputObjects.OrderByDescending(o => o.DataValue(list[0])).ToList();
                        break;
                }
            }

            foreach (var obj in sortedObjects)
            {
                var idx = Fact.Objects.IndexOf(obj);
                if (idx >= 0)
                {
                    Add(idx);
                }
            }
            Succeeded = Output.Count > 0;
        }

        public void SortByRelation(string attribute)
        {
            bool ascending = false;
            var inputObjects = new List<SpatialObject>();
            var preIndices = Fact.Backtrace();

            foreach (var i in Input)
            {
                inputObjects.Add(Fact.Objects[i]);
            }

            var list = attribute.Split(' ').Select(a => a.Trim()).ToArray();
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
                sortedObjects = inputObjects.OrderBy(o => o.CalcRelationValue(attribute, preIndices)).ToList();
            }
            else
            {
                sortedObjects = inputObjects.OrderByDescending(o => o.CalcRelationValue(attribute, preIndices)).ToList();
            }

            foreach (var obj in sortedObjects)
            {
                var idx = Fact.Objects.IndexOf(obj);
                if (idx >= 0)
                {
                    Add(idx);
                }
            }

            Succeeded = Output.Count > 0;
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

        //public static string AttributePredicate(string condition)
        //{
        //    string cond = condition.Trim();

        //    // Iterate through boolean attributes and modify condition
        //    foreach (var word in SpatialObject.BooleanAttributes.Keys)
        //    {
        //        int index = 0;
        //        while ((index = cond.IndexOf(word, index)) != -1)
        //        {
        //            if (index + word.Length < cond.Length)
        //            {
        //                var ahead = cond.Substring(index + word.Length, Math.Min(5, cond.Length - index - word.Length));
        //                if (!ahead.Contains("=") && !ahead.Contains("<") && !ahead.Contains(">"))
        //                {
        //                    cond = cond.Substring(0, index) + word + " == TRUE" + cond.Substring(index + word.Length);
        //                }
        //            }
        //            else
        //            {
        //                cond = cond.Substring(0, index) + word + " == TRUE";
        //            }
        //            index += word.Length;
        //        }
        //    }

        //    return cond;
        //}



    }
}
