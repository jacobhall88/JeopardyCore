using System;
using System.Collections.Generic;
using System.Text;

namespace JeopardyCore
{
    public class JeoCategory<T> : List<T>
    {
        public string CatName;

        //used to determine if a category contains a Double or Final jeopardy question, has first and/or second round questions,
        //or question with an audio/visual/link component, without having to interate through the entire list
        public bool HasFirst { get; set; } = false;
        public bool HasSecond { get; set; } = false;
        public bool HasDouble { get; set; } = false;
        public bool HasFinal { get; set; } = false;
        public bool HasAVL { get; set; } = false;

        //used to determine if a category has enough questions to fill a standard round
        public bool IsFull { get; set; } = false;

        //calls ToString for each contained question, with a linebreak after each
        override public string ToString()
        {
            string retString = CatName + ":\n";
            return retString + string.Join(Environment.NewLine, this);
        }

    }
}
