using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunePlugin
{
	public class SWMessage
	{
		public string command;

		protected string session_key;

		public int proto_ver;

		public string infocsv;

		public int channel_uid;

		public int ts_val;

		public int wizard_id;
	}
}
