using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JeopardyCore
{
    class JeoButton : Button
    {
        public JeoQuestion Question;
        public bool Answered { get; set; }

        public JeoButton (JeoQuestion q)
        {
            Question = q;
        }
    }
}
