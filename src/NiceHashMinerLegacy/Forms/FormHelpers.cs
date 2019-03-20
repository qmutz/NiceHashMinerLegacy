using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public static class FormHelpers
    {

        public static void TranslateFormControls(Control c)
        {
            try
            {
                c.Text = Translations.Tr(c.Text);
            }
            catch(Exception)
            {
            }
            
            // call on all controls
            foreach (Control childC in c.Controls)
            {
                TranslateFormControls(childC);
            }
        }

        // TODO maybe not the best name
        static public void SafeInvoke(Control c, MethodInvoker f)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(f);
            }
            else
            {
                f();
            }
        }

        public static void SafeUpdateTextbox(TextBox t, string text)
        {
            if (t.InvokeRequired)
            {
                t.Invoke(new Action(() =>
                {
                    SafeUpdateTextbox(t, text);
                }));
            }
            else
            {
                t.Text = text;
            }
        }
    }
}
