using System;
using System.Collections.Generic;
using System.Text;

namespace JeopardyCore
{
    class JeoCategory<T> : List<T>
    {
        public string CatName { get; set; }

        //used to determine if a category contains a Double or Final jeopardy question, has first and/or second round questions,
        //or question with an audio/visual/link component, without having to interate through the entire list
        public bool HasFirst { get; set; } = false;
        public bool HasSecond { get; set; } = false;
        public bool HasDouble { get; set; } = false;
        public bool HasFinal { get; set; } = false;
        public bool HasAVL { get; set; } = false;


        //used to determine if a category has enough questions to fill a standard round
        public bool IsFull { get; set; } = false;
    }
}
