using System;
using System.Collections;
using System.Linq;
using System.IO;
using Newtonsoft.Json;


namespace SRcsharp.Library
{
    public class SpatialTerms
    {

        private static SpatialTerms _instance = new SpatialTerms();

        public static SpatialTerms Instance
        {
            get {  return _instance; }
            set { _instance = value; }
        }


        private List<PredicateTerm> _terms; 

        public List<PredicateTerm> Terms
		{
			get { return _terms; }
			set { _terms = value; }
		}

        public SpatialTerms()
        {
            _terms = new List<PredicateTerm>();
            _terms = LoadPredicateTerms();
        }



        public SpatialPredicate? GetPredicate(string name)
        {
            var pred = SpatialPredicate.CreateSpatialPredicateByName(name);
            if(SpatialPredicate.IsDefined(pred)) { return pred; }

            pred = _terms.Where(t => t.Predicate == name).Select(t => t.Code).FirstOrDefault();
            if (SpatialPredicate.IsDefined(pred)) { return pred; }

            pred = _terms.Where(t => t.Synonym == name).Select(t => t.Code).FirstOrDefault();
            if (SpatialPredicate.IsDefined(pred)) { return pred; }

            return null;
        }

        public string GetTerm(SpatialPredicate code)
        {

            var pred = _terms.Where(t => t.Code == code).Select(t => t.Predicate).FirstOrDefault();
            if (pred != null) { return pred; }

            return "undefined";
        }

        public string GetTermWithPreposition(SpatialPredicate code)
        {

            PredicateTerm pred;
            try
            {
                pred = _terms.Where(t => t.Code == code).First();
            }
            catch(Exception ex)
            {
                return "undefined";
            }

            if(string.IsNullOrWhiteSpace(pred.Preposition))
            {
                return pred.Predicate;
            }
            else
            {
                return pred.Predicate + " " + pred.Predicate;
            }
        }

        public string GetTermWithVerbAndPreposition(SpatialPredicate code)
        {

            PredicateTerm pred;
            try
            {
                pred = _terms.Where(t => t.Code == code).First();
            }
            catch (Exception ex)
            {
                return "undefined";
            }

            if (string.IsNullOrWhiteSpace(pred.Preposition))
            {
                return pred.Verb + " " + pred.Predicate;
            }
            else
            {
                return pred.Verb + " " + pred.Predicate + " " + pred.Predicate;
            }
        }

        public bool IsSymmetric(SpatialPredicate code)
        {
            return _terms.Any(term => term.Code == code && term.Predicate == term.Reverse);
        }

        public bool IsInverse(string pred)
        {
            throw new NotImplementedException();
        }

        public bool IsNegation(string pred)
        {
            throw new NotImplementedException();
        }



        public List<PredicateTerm> LoadPredicateTerms(string jsonFilePath = "spatialterms.json")
        {
            var json = File.ReadAllText(jsonFilePath);
            _terms = JsonConvert.DeserializeObject<List<PredicateTerm>>(json, new SpatialPredicateTypeConverter());
            if(_terms == null)
            {
                throw new NotImplementedException("Error reading predicate terms config file");
            }
            return _terms;
            
        }

        public class SpatialPredicateTypeConverter : Newtonsoft.Json.JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException("We only load the configuration. The type will skip the converter.");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.Value == null) { return new SpatialPredicate(); }

                string val = (string)reader.Value;
                var pred = SpatialPredicate.CreateSpatialPredicateByName(val);

                return pred;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(SpatialPredicate);
            }
        }



    }
}
