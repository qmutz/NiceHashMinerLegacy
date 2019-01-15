using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.Equihash
{
    public class Ewbf144 : Ewbf
    {
        public Ewbf144()
            : base("ewbf144")
        { }

        protected override string GetStartCommand(string url, string username)
        {
            var command = base.GetStartCommand(url, username);

            return command + " --algo 144_5 --pers auto";
        }
    }
}
