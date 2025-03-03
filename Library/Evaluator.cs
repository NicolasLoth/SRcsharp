using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public static class Evaluator
    {
        static Regex wordRE = new Regex(@"[A-Z]+", RegexOptions.Compiled);
        static HashSet<string> Operators = new[] { "AND", "OR", "NOT" }.ToHashSet(StringComparer.OrdinalIgnoreCase);
        public static bool Evaluate(this List<string> options, string op)
        {
            var opListOfOptions = wordRE.Matches(op).Select(m => m.Value).Where(w => !Operators.Contains(w));

            foreach (var option in opListOfOptions)
            {
                var value = options.Contains(option).ToString();
                op = op.Replace(option, value);
            }

            //return DTEval(op) == 1;
            return CompEval(op);
            //return XEval(op);
        }

        static double DTEval(string expression)
        {
            var dt = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(double), expression);
            dt.Columns.Add(loDataColumn);
            dt.Rows.Add(0);
            return (double)(dt.Rows[0]["Eval"]);
        }

        public static bool Eval(string expression, bool throwException=true)
        {
            bool? res = null;
            try
            {
                res = CompEval(expression);
            }
            catch(Exception ex) 
            {
                res = XEval(expression);
            }

            if (!res.HasValue)
            {
                throw new Exception("Evaluation of expression failed: " + expression);
            }

            return res.Value;
        }

        static DataTable cDT = new DataTable();
        public static bool CompEval(string expression)
        {
            return (bool)cDT.Compute(expression, "");
        }

        public static bool XEval(string expression, bool throwException = true)
        {
            expression = new System.Text.RegularExpressions.Regex(@"not +(true|false)").Replace(expression.ToLower(), " not(${1}) ");
            expression = new System.Text.RegularExpressions.Regex(@"(true|false)").Replace(expression, " ${1}() ");

            var res = false;
            try
            {
                res = (bool)new System.Xml.XPath.XPathDocument(new System.IO.StringReader("<r/>")).CreateNavigator()
                    .Evaluate(String.Format("boolean({0})", expression));
            }
            catch (Exception ex)
            {
                if (throwException)
                    throw ex;
                return false;
            }

            return res;
        }
    }
}
