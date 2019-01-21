using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    static class FormHelpers
    {
        // TODO maybe not the best name
        static public void SafeInvoke(Control c, Action f, bool beginInvoke = false)
        {
            if (c.InvokeRequired)
            {
                if (beginInvoke)
                {
                    c.BeginInvoke(f);
                } else
                {
                    c.Invoke(f);
                }
            }
            else
            {
                f();
            }
        }
    }
}
