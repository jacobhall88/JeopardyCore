using System;
using System.Collections.Generic;
using System.Text;

namespace JeopardyCore
{
    public enum QType { Standard, Double, Final, Special }
    public enum RoundType { First, Second, Final, Special }

    public class JeoQuestion
    {

        public string Category { get; set; }
        public string Clue { get; set; }
        public string Answer { get; set; }
        public int Value { get; set; }
        public QType Type { get; set; }
        public RoundType Round { get; set; }
        public bool IsVideo { get; set; } = false;
        public bool IsAudio { get; set; } = false;
        public bool HasLink { get; set; } = false;

        public JeoQuestion(string cat, string clu, string ans, int val, RoundType roun, QType typ)
        {
            Category = cat; Clue = clu; Answer = ans; Value = val; Round = roun; Type = typ;
        }

        override public string ToString()
        {
            string val = Value.ToString();
            string typ = Type.ToString();
            string ron = Round.ToString();
            string vid = IsVideo.ToString();
            string aud = IsAudio.ToString();
            string lin = HasLink.ToString();
            string retString = "Clue: " + Clue + "\n" +
                " Answer: " + Answer + "\n" +
                " Category " + Category + "\n" +
                " Round: " + ron + "\n" +
                " Value: " + val + "\n" +
                " Type: " + typ + "\n" +
                " Has Video: " + vid + "\n" +
                " Has Audio: " + aud + "\n" +
                " Has Link: " + lin + "\n";
            return retString;
        }

        //dummy constructor
        public JeoQuestion()
        {

        }
    }
}
