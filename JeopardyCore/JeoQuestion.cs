﻿using System;
using System.Collections.Generic;
using System.Text;

namespace JeopardyCore
{
    enum QType { Standard, Double, Final, Special }
    enum RoundType { First, Second, Final, Special }

    class JeoQuestion
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

        //constructor to be called with standard questions
        public JeoQuestion(string cat, string clu, string ans, int val, RoundType roun, QType typ)
        {
            Category = cat; Clue = clu; Answer = ans; Value = val; Round = roun; Type = typ;
        }

    }
}