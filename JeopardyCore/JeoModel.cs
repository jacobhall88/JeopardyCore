using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace JeopardyCore
{
    class JeoModel
    {
        Random rando = new Random();
        public List<JeoCategory<JeoQuestion>> Categories { get; set; } = new List<JeoCategory<JeoQuestion>>();

        public JeoModel()
        {
            using (StreamReader sr = new StreamReader("jeopardydb - full.json"))
            {
                string json = sr.ReadToEnd();
                JObject db = JObject.Parse(json);

                //iterate through each category
                int totalCount = 0;
                foreach (JProperty category in db.Properties())
                {
                    //iterate through each question within each category
                    foreach (JToken question in category)
                    {
                        JeoCategory<JeoQuestion> catList = new JeoCategory<JeoQuestion>();
                        catList.CatName = category.Name;

                        //keeps track of how many questions in the category will fit into a standard round
                        int qCount = 0;

                        //create a JeoQuestion based on the entries in each quesiton
                        foreach (JToken entry in question.Children())
                        {
                            ++totalCount;
                            string clue = entry["clue"].ToString();
                            string answer = entry["answer"].ToString();
                            string cat = category.Name.ToString().ToUpper();

                            //check if the value has a dollar amount, else determine what kind of question it is
                            QType type = QType.Standard;
                            string value = entry["value"].ToString();
                            if (int.TryParse(value, out int val));
                            else
                            {
                                if (value.Contains("Final")) type = QType.Final;
                                else if (value.Contains("Double")) type = QType.Double;
                                else type = QType.Special;
                            }

                            //determine and assign which round the question is from
                            string roundStr = entry["round"].ToString();
                            RoundType round = RoundType.Special;
                            if (int.TryParse(roundStr, out int roun))
                            {
                                if (roun == 1)
                                {
                                    round = RoundType.First;
                                    catList.HasFirst = true;
                                }
                                if (roun == 2)
                                {
                                    round = RoundType.Second;
                                    catList.HasSecond = true;
                                }
                            }
                            else round = RoundType.Final;

                            //create the JeoQuestion object and check for special flags
                            JeoQuestion addQ = new JeoQuestion(cat, clue, answer, val, round, type);
                            if (clue.ToUpper().Contains("AUDIO DAILY DOUBLE")) addQ.IsAudio = true;
                            if (clue.ToUpper().Contains("VIDEO DAILY DOUBLE")) addQ.IsVideo = true;
                            if (clue.ToUpper().Contains("A HREF")) addQ.HasLink = true;

                            //update category based on flags
                            if (addQ.IsAudio || addQ.IsVideo || addQ.HasLink) catList.HasAVL = true;
                            if (addQ.Type == QType.Double) catList.HasDouble = true;
                            if (addQ.Type == QType.Final) catList.HasFinal = true;
                            if (addQ.Type == QType.Standard || addQ.Type == QType.Double) ++qCount;

                            catList.Add(addQ);
                        }

                        if (qCount > 4) catList.IsFull = true;
                        Categories.Add(catList);
                    }
                }
                sr.Dispose();
            }
        }

        //return random category of specified RoundType. Alternate definition allows preference for Audio/Visual/Link quesitons.
        public JeoCategory<JeoQuestion> RandomCat(RoundType type)
        {
            JeoCategory<JeoQuestion> retCat = new JeoCategory<JeoQuestion>();
            retCat = Categories[rando.Next(Categories.Count)];

            //find new random category until parameters correct round type is found
            //also verify if category has enough questions to fill a standard round (IsFull property)
            if (type == RoundType.First) while (!retCat.HasFirst || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            else if (type == RoundType.Second) while (!retCat.HasSecond || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            else if (type == RoundType.Final) while (!retCat.HasFinal || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];

            return retCat;
        }
        public JeoCategory<JeoQuestion> RandomCat(RoundType type, bool hasAVL)
        {
            JeoCategory < JeoQuestion > retCat = new JeoCategory<JeoQuestion>();
            retCat = Categories[rando.Next(Categories.Count)];

            //find new random category until parameters (correct round and/or presence of audio/visual/link questions) are met
            //also verify if category has enough questions to fill a standard round (IsFull property)
            if (type == RoundType.First)
            {
                if (!hasAVL) while (retCat.HasAVL || !retCat.HasFirst || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
                else while (!retCat.HasFirst || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            }
            else if (type == RoundType.Second)
            {
                if (!hasAVL) while (retCat.HasAVL || !retCat.HasSecond || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
                else while (!retCat.HasSecond || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            }
            else if (type == RoundType.Final)
            {
                if (!hasAVL) while (retCat.HasAVL || !retCat.HasFinal || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
                else while (!retCat.HasFinal || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            }
            return retCat;
        }

        //return a category matching the sent string
        //returns null if not found
        public JeoCategory<JeoQuestion> CatByName(string nm)
        {
            JeoCategory<JeoQuestion> retCat = new JeoCategory<JeoQuestion>();
            string name = nm.ToUpper();

            return Categories.Find(x => x.CatName.Contains(name));
        }
    }
}
