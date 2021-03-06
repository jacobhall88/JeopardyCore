﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JeopardyCore
{

    class JeoGame
    {
        public List<JeoCategory<JeoQuestion>> FirstRound { get; set; }
        public List<JeoCategory<JeoQuestion>> SecondRound { get; set; }
        public List<JeoButton> FRQuestions { get; set; } = new List<JeoButton>();
        public List<JeoButton> SRQuestions { get; set; } = new List<JeoButton>();
        public JeoQuestion FinalQuestion { get; set; }
        public int Cash { get; set; } = 0;

        public JeoGame(List<JeoCategory<JeoQuestion>> first, List<JeoCategory<JeoQuestion>> second, JeoQuestion final)
        {
            FirstRound = first; SecondRound = second; FinalQuestion = final;

        }
        override public string ToString()
        {
            string retString = "";
            retString += FirstRound.ToString();
            retString += SecondRound.ToString();
            retString += FinalQuestion.ToString();

            return retString;
        }
    }
}
