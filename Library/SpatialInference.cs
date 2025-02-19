using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SpatialInference
    {
        private int[] _input;
        private int[] _output;
        private string _operation;
        private bool _succeeded;
        private string _error;
        private SpatialReasoner _fact;

        public int[] Input { get => _input; set => _input = value; }
        public int[] Output { get => _output; set => _output = value; }
        public string Operation { get => _operation; set => _operation = value; }
        public bool Succeeded { get => _succeeded; set => _succeeded = value; }
        public string Error { get => _error; set => _error = value; }
        public SpatialReasoner Fact { get => _fact; set => _fact = value; }

        public SpatialInference(int[] input, string operation, SpatialReasoner fact)
        {
            _input = input;
            _operation = operation;
            _fact = fact;
            int endIdx = operation.Length - 1;

            if (operation.StartsWith("filter("))
            {
                string condition = operation.Substring(0, endIdx - 7);
                Filter(condition);
            }
            else if (operation.StartsWith("pick("))
            {
                string relations = operation.Substring(0, endIdx - 5);
                Pick(relations);
            }
            else if (operation.StartsWith("select("))
            {
                string terms = operation.Substring(0, endIdx - 7);
                Select(terms);
            }
            else if (operation.StartsWith("sort("))
            {
                string attribute = operation.Substring(0, endIdx - 5);
                Sort(attribute);
            }
            else if (operation.StartsWith("slice("))
            {
                string range = operation.Substring(0, endIdx - 6);
                Slice(range);
            }
            else if (operation.StartsWith("produce("))
            {
                string terms = operation.Substring(0, endIdx - 8);
                Produce(terms);
            }
            else if (operation.StartsWith("calc("))
            {
                string assignments = operation.Substring(0, endIdx - 5);
                Calc(assignments);
            }
            else if (operation.StartsWith("map("))
            {
                string assignments = operation.Substring(0, endIdx - 4);
                Map(assignments);
            }
        }

        private void Add(int index)
        {
            if (!Output.Contains(index))
            {
                Output.Append(index);
            }
        }

        public void Filter(string condition)
        {
            var predicate = AttributePredicate(condition);
            var baseObjects = Fact.Base["objects"] as object[];

            foreach (var i in Input)
            {
                bool result = predicate.Invoke(baseObjects[i]);
                if (result)
                {
                    Add(i);
                }
            }
            Succeeded = true;
        }

        public void Pick(string relations)
        {
            var predicates = relations.Keywords();

            foreach (var i in Input)
            {
                for (int j = 0; j < Fact.Objects.Count; j++)
                {
                    string cond = relations;
                    if (i != j)
                    {
                        foreach (var predicate in predicates)
                        {
                            if (Fact.Does(Fact.Objects[j], predicate, i))
                            {
                                cond = cond.Replace(predicate, "TRUEPREDICATE");
                            }
                            else
                            {
                                cond = cond.Replace(predicate, "FALSEPREDICATE");
                            }
                        }

                        bool result = NSPredicate.Format(cond).EvaluateWith(null);
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
            var baseObjects = Fact.Base["objects"] as List<object>;

            foreach (var i in Input)
            {
                for (int j = 0; j < Fact.Objects.Count; j++)
                {
                    string cond = relations;
                    if (i != j)
                    {
                        foreach (var predicate in predicates)
                        {
                            if (Fact.Does(Fact.Objects[j], predicate, i))
                            {
                                cond = cond.Replace(predicate, "TRUEPREDICATE");
                            }
                            else
                            {
                                cond = cond.Replace(predicate, "FALSEPREDICATE");
                            }
                        }

                        bool result = NSPredicate.Format(cond).EvaluateWith(null);
                        if (result)
                        {
                            var attrPredicate = SpatialInference.AttributePredicate(conditions);
                            bool result2 = attrPredicate.EvaluateWith(baseObjects[j]);
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
            var list = assignments.Split(';').Select(a => a.Trim()).ToArray();
            var baseObjects = Fact.Base["objects"] as List<object>;

            foreach (var i in Input)
            {
                var dict = new Dictionary<string, object>();

                if (Fact.Base.ContainsKey("data"))
                {
                    foreach (var pair in Fact.Base["data"] as Dictionary<string, object>)
                    {
                        dict[pair.Key] = pair.Value;
                    }
                }

                foreach (var assignment in list)
                {
                    var kv = assignment.Split('=');
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim();
                        var expr = kv[1].Trim();
                        var expression = new NSExpression(expr);
                        var value = expression.ExpressionValueWith(baseObjects[i], null);

                        if (value != null)
                        {
                            dict[key] = value;
                        }
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
            var list = assignments.Split(';').Select(a => a.Trim()).ToArray();

            foreach (var assignment in list)
            {
                var kv = assignment.Split('=');
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim();
                    var expr = kv[1].Trim();
                    var expression = new NSExpression(expr);
                    var value = expression.ExpressionValueWith(Fact.Base, null);

                    if (value != null)
                    {
                        Fact.SetData(key, value);
                    }
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

            var idxRange = new Range<int>(lower, upper);
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
                    case "width":
                        sortedObjects = inputObjects.OrderBy(o => o.Width).ToList();
                        break;
                    case "height":
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
                    case "width":
                        sortedObjects = inputObjects.OrderByDescending(o => o.Width).ToList();
                        break;
                    case "height":
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
                sortedObjects = inputObjects.OrderBy(o => o.RelationValue(attribute, preIndices)).ToList();
            }
            else
            {
                sortedObjects = inputObjects.OrderByDescending(o => o.RelationValue(attribute, preIndices)).ToList();
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
            return !string.IsNullOrEmpty(error);
        }

        public bool IsManipulating()
        {
            return operation.StartsWith("filter") ||
                   operation.StartsWith("pick") ||
                   operation.StartsWith("select") ||
                   operation.StartsWith("produce") ||
                   operation.StartsWith("slice");
        }

        public Dictionary<string, object> AsDict()
        {
            return new Dictionary<string, object>
                {
                    { "operation", operation },
                    { "input", input },
                    { "output", output },
                    { "error", error },
                    { "succeeded", succeeded }
                };
        }

        public static Predicate<string> AttributePredicate(string condition)
        {
            string cond = condition.Trim();

            // Iterate through boolean attributes and modify condition
            foreach (var word in SpatialObject.BooleanAttributes)
            {
                int index = 0;
                while ((index = cond.IndexOf(word, index)) != -1)
                {
                    if (index + word.Length < cond.Length)
                    {
                        var ahead = cond.Substring(index + word.Length, Math.Min(5, cond.Length - index - word.Length));
                        if (!ahead.Contains("=") && !ahead.Contains("<") && !ahead.Contains(">"))
                        {
                            cond = cond.Substring(0, index) + word + " == TRUE" + cond.Substring(index + word.Length);
                        }
                    }
                    else
                    {
                        cond = cond.Substring(0, index) + word + " == TRUE";
                    }
                    index += word.Length;
                }
            }

            return new Predicate<string>((s) => s == cond);
        }



        public static class StringExtensions
        {
            public static List<string> Keywords(this string str)
            {
                var scanner = new Scanner(str);
                var keywords = new List<string>();
                while (!scanner.IsAtEnd)
                {
                    var result = scanner.ScanCharactersFrom(new CharacterSet("abcdefghijklmnopqrstuvwxyz"));
                    if (result != null)
                    {
                        if (!keywords.Contains(result))
                        {
                            keywords.Add(result);
                        }
                    }
                    scanner.ScanUpToCharactersFrom(new CharacterSet("abcdefghijklmnopqrstuvwxyz"));
                }
                return keywords;
            }
        }

    }
}
