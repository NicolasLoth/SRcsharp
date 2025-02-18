using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public struct PredicateTerm
    {
		private SpatialPredicate _code;

		public SpatialPredicate Code
		{
			get { return _code; }
			set { _code = value; }
		}

		private string _predicate;

		public string Predicate
		{
			get { return _predicate; }
			set { _predicate = value; }
		}

		private string _preposition;

		public string Preposition
		{
			get { return _preposition; }
			set { _preposition = value; }
		}

		private string _synonym = string.Empty;

		public string Synonym
		{
			get { return _synonym; }
			set { _synonym = value; }
		}

		private string _reverse = string.Empty;

		public string Reverse
		{
			get { return _reverse; }
			set { _reverse = value; }
		}

		private string _antonym = string.Empty;

		public string Antonym
		{
			get { return _antonym; }
			set { _antonym = value; }
		}

		private string _verb = "is";

        public string Verb
		{
			get { return _verb; }
			set { _verb = value; }
		}

        public PredicateTerm(SpatialPredicate code, string pred, string preposition, string synonym, string reverse, string antonym, string verb)
        {
			_code = code;		
			_predicate = pred;
			_preposition = preposition;
			_synonym = synonym;	
			_reverse = reverse;	
			_antonym = antonym;	
			_verb = verb;
        }


    }
}
