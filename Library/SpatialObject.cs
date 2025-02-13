using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRcsharp.Library
{
    public class SpatialObject
    {

		private string _id;

		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private string _label;

		public string Label
		{
			get { return _label; }
			set { _label = value; }
		}

		private string _type;

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}




	}
}
