using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JeopardyCore
{
    public class JeoModel
    {
        const string DBPATH = "jeopardydb - partial.json";
        Random rando = new Random();
        public List<JeoCategory<JeoQuestion>> Categories { get; set; } = new List<JeoCategory<JeoQuestion>>();

        public JeoModel()
        {
            bool oldCat = false;
            using (StreamReader sr = new StreamReader(DBPATH))
            {
                string json = sr.ReadToEnd();
                JObject db = JObject.Parse(json);

                //iterate through each category
                int totalCount = 0;
                foreach (JProperty category in db.Properties())
                {
                    JeoCategory<JeoQuestion> catList = new JeoCategory<JeoQuestion>();
                    catList.CatName = category.Name.ToString().ToUpper();
                    oldCat = false;
                    //iterate through each question within each category
                    foreach (JToken question in category)
                    {
                        
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

                            //jeopardy questions used to be worth half as much; for consistency, update older values to newer standard
                            if ((addQ.Round == RoundType.First) && ((addQ.Value % 200F) != 0) && (addQ.Value > 0))
                            {
                                oldCat = true;
                            }
                            if ((addQ.Round == RoundType.Second) && ((addQ.Value % 400F) != 0) && (addQ.Value > 0))
                            {
                                oldCat = true;
                            }
                            if (oldCat) addQ.Value = addQ.Value * 2;
                            catList.Add(addQ);
                        }

                        if (qCount > 4) catList.IsFull = true;
                        Categories.Add(catList);
                    }
                }
                sr.Dispose();
            }
        }

        //return random category of specified RoundType. Alternate definition allows preference for Audio/Visual/Link quesitons
        //and/or daily doubles
        //todo: add a definition that respects AVL, but does not check hasDouble. How to do without duplicate method signatures?
        public JeoCategory<JeoQuestion> RandomCat(RoundType type)
        {
            JeoCategory<JeoQuestion> retCat = new JeoCategory<JeoQuestion>();
            retCat = Categories[rando.Next(Categories.Count)];

            //find new random category until parameters correct round type is found
            //also verify if category has enough questions to fill a standard round (IsFull property)
            if (type == RoundType.First) while (!retCat.HasFirst || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            else if (type == RoundType.Second) while (!retCat.HasSecond || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];
            else if (type == RoundType.Final) while (!retCat.HasFinal || !retCat.IsFull) retCat = Categories[rando.Next(Categories.Count)];

            //if there are more than 5 questions in a category, randomly select one each of the correct value
            if (retCat.Count > 5 && type == RoundType.First)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 200 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 600 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1000 && x.Round == RoundType.First));

                retCat = newCat;
            }
            if (retCat.Count > 5 && type == RoundType.Second)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1200 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1600 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 2000 && x.Round == RoundType.Second));

                retCat = newCat;
            }

            return retCat;
        }
        public JeoCategory<JeoQuestion> RandomCat(RoundType type, bool hasDouble)
        {
            JeoCategory<JeoQuestion> retCat = new JeoCategory<JeoQuestion>();
            retCat = Categories[rando.Next(Categories.Count)];

            //find new random category until parameters (correct round and/or presence of audio/visual/link questions) are met
            //also verify if category has enough questions to fill a standard round (IsFull property)
            if (type == RoundType.First)
            {
                if (hasDouble)
                {
                    while (!retCat.HasDouble || !retCat.HasFirst || !retCat.IsFull)
                    {
                        retCat = new JeoCategory<JeoQuestion>();
                        retCat = Categories[rando.Next(Categories.Count)];
                    }
                }
                else
                {
                    while (retCat.HasDouble || !retCat.HasFirst || !retCat.IsFull)
                    {
                        retCat = new JeoCategory<JeoQuestion>();
                        retCat = Categories[rando.Next(Categories.Count)];
                    }
                }
            }
            else if (type == RoundType.Second)
            {
                if (hasDouble)
                {
                    while (!retCat.HasDouble || !retCat.HasSecond || !retCat.IsFull)
                    {
                        retCat = new JeoCategory<JeoQuestion>();
                        retCat = Categories[rando.Next(Categories.Count)];
                    }
                }
                else
                {
                    while (retCat.HasDouble || !retCat.HasSecond || !retCat.IsFull)
                    {
                        retCat = new JeoCategory<JeoQuestion>();
                        retCat = Categories[rando.Next(Categories.Count)];
                    }
                }
            }
            else if (type == RoundType.Final)
            {
                if (hasDouble)
                {
                    while (!retCat.HasDouble || !retCat.HasFinal || !retCat.IsFull)
                    {
                        retCat = new JeoCategory<JeoQuestion>();
                        retCat = Categories[rando.Next(Categories.Count)];
                    }
                }
                else
                {
                    while (retCat.HasDouble || !retCat.HasFinal || !retCat.IsFull)
                    {
                        retCat = new JeoCategory<JeoQuestion>();
                        retCat = Categories[rando.Next(Categories.Count)];
                    }
                }
            }

            //if there are more than 5 questions in a category, randomly select one each of the correct value
            if (retCat.Count > 5 && type == RoundType.First)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();
                newCat.CatName = retCat.CatName;
                newCat.HasFirst = retCat.HasFirst;
                newCat.HasDouble = retCat.HasDouble;
                newCat.HasFinal = retCat.HasFinal;
                newCat.HasAVL = retCat.HasAVL;
                newCat.IsFull = retCat.IsFull;
                newCat.Clear();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 200 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 600 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1000 && x.Round == RoundType.First));

                //if method call has a daily double, replace any empty spots, else randomly replace one of the questions with it
                //todo: replace the value that the daily double was originally at. not possible?
                if (hasDouble)
                {
                    newCat[rando.Next(4)] = retCat.Find(x => x.Type == QType.Double);
                }

                retCat = newCat;
            }
            if (retCat.Count > 5 && type == RoundType.Second)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();
                newCat.CatName = retCat.CatName;
                newCat.HasFirst = retCat.HasFirst;
                newCat.HasDouble = retCat.HasDouble;
                newCat.HasFinal = retCat.HasFinal;
                newCat.HasAVL = retCat.HasAVL;
                newCat.IsFull = retCat.IsFull;
                newCat.Clear();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1200 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1600 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 2000 && x.Round == RoundType.Second));

                //if method call has a daily double, randomly replace one of the questions with it
                //todo: replace the value that the daily double was originally at. not possible?
                if (hasDouble)
                {
                    newCat[rando.Next(4)] = retCat.Find(x => x.Type == QType.Double);
                }

                retCat = newCat;
            }

            return retCat;
        }
        public JeoCategory<JeoQuestion> RandomCat(RoundType type, bool hasDouble, bool hasAVL)
        {
            JeoCategory<JeoQuestion> retCat = new JeoCategory<JeoQuestion>();

            if (!hasAVL)
            {
                //loop the find category logic until a category with an audio/visual/link quesiton is found
                do
                {
                    retCat = new JeoCategory<JeoQuestion>();
                    retCat = Categories[rando.Next(Categories.Count)];
                    //find new random category until parameters (correct round and/or presence of audio/visual/link questions) are met
                    //also verify if category has enough questions to fill a standard round (IsFull property)
                    if (type == RoundType.First)
                    {
                        if (hasDouble)
                        {
                            while (!retCat.HasDouble || !retCat.HasFirst || !retCat.IsFull)
                            {
                                retCat = new JeoCategory<JeoQuestion>();
                                retCat = Categories[rando.Next(Categories.Count)];
                            }
                        }
                        else
                        {
                            while (retCat.HasDouble || !retCat.HasFirst || !retCat.IsFull)
                            {
                                retCat = new JeoCategory<JeoQuestion>();
                                retCat = Categories[rando.Next(Categories.Count)];
                            }
                        }
                    }
                    else if (type == RoundType.Second)
                    {
                        if (hasDouble)
                        {
                            while (!retCat.HasDouble || !retCat.HasSecond || !retCat.IsFull)
                            {
                                retCat = new JeoCategory<JeoQuestion>();
                                retCat = Categories[rando.Next(Categories.Count)];
                            }
                        }
                        else
                        {
                            while (retCat.HasDouble || !retCat.HasSecond || !retCat.IsFull)
                            {
                                retCat = new JeoCategory<JeoQuestion>();
                                retCat = Categories[rando.Next(Categories.Count)];
                            }
                        }
                    }
                    else if (type == RoundType.Final)
                    {
                        {
                            if (hasDouble)
                            {
                                while (!retCat.HasDouble || !retCat.HasFinal || !retCat.IsFull)
                                {
                                    retCat = new JeoCategory<JeoQuestion>();
                                    retCat = Categories[rando.Next(Categories.Count)];
                                }
                            }
                            else
                            {
                                while (retCat.HasDouble || !retCat.HasFinal || !retCat.IsFull)
                                {
                                    retCat = new JeoCategory<JeoQuestion>();
                                    retCat = Categories[rando.Next(Categories.Count)];
                                }
                            }
                        }
                    }
                } while (retCat.HasAVL);
            }
            else
            {
                //find new random category until parameters (correct round and/or presence of audio/visual/link questions) are met
                //also verify if category has enough questions to fill a standard round (IsFull property)
                if (type == RoundType.First)
                {
                    if (hasDouble)
                    {
                        while (!retCat.HasDouble || !retCat.HasFirst || !retCat.IsFull)
                        {
                            retCat = new JeoCategory<JeoQuestion>();
                            retCat = Categories[rando.Next(Categories.Count)];
                        }
                    }
                    else
                    {
                        while (retCat.HasDouble || !retCat.HasFirst || !retCat.IsFull)
                        {
                            retCat = new JeoCategory<JeoQuestion>();
                            retCat = Categories[rando.Next(Categories.Count)];
                        }
                    }
                }
                else if (type == RoundType.Second)
                {
                    if (hasDouble)
                    {
                        while (!retCat.HasDouble || !retCat.HasSecond || !retCat.IsFull)
                        {
                            retCat = new JeoCategory<JeoQuestion>();
                            retCat = Categories[rando.Next(Categories.Count)];
                        }
                    }
                    else
                    {
                        while (retCat.HasDouble || !retCat.HasSecond || !retCat.IsFull)
                        {
                            retCat = new JeoCategory<JeoQuestion>();
                            retCat = Categories[rando.Next(Categories.Count)];
                        }
                    }
                }
                else if (type == RoundType.Final)
                {
                    if (hasDouble)
                    {
                        while (!retCat.HasDouble || !retCat.HasFinal || !retCat.IsFull)
                        {
                            retCat = new JeoCategory<JeoQuestion>();
                            retCat = Categories[rando.Next(Categories.Count)];
                        }
                    }
                    else
                    {
                        while (retCat.HasDouble || !retCat.HasFinal || !retCat.IsFull)
                        {
                            retCat = new JeoCategory<JeoQuestion>();
                            retCat = Categories[rando.Next(Categories.Count)];
                        }
                    }
                }
            }

            if (retCat.Count > 5 && type == RoundType.First)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();
                newCat.CatName = retCat.CatName;
                newCat.HasFirst = retCat.HasFirst;
                newCat.HasDouble = retCat.HasDouble;
                newCat.HasFinal = retCat.HasFinal;
                newCat.HasAVL = retCat.HasAVL;
                newCat.IsFull = retCat.IsFull;
                newCat.Clear();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 200 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 600 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1000 && x.Round == RoundType.First));

                //if method call has a daily double, randomly replace one of the questions with it
                //todo: replace the value that the daily double was originally at. not possible?
                if (hasDouble)
                {
                    newCat[rando.Next(4)] = retCat.Find(x => x.Type == QType.Double);
                }

                retCat = newCat;
            }
            if (retCat.Count > 5 && type == RoundType.Second)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();
                newCat.CatName = retCat.CatName;
                newCat.HasFirst = retCat.HasFirst;
                newCat.HasDouble = retCat.HasDouble;
                newCat.HasFinal = retCat.HasFinal;
                newCat.HasAVL = retCat.HasAVL;
                newCat.IsFull = retCat.IsFull;
                newCat.Clear();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1200 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1600 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 2000 && x.Round == RoundType.Second));

                //if method call has a daily double, randomly replace one of the questions with it
                //todo: replace the value that the daily double was originally at. not possible?
                if (hasDouble)
                {
                    newCat[rando.Next(4)] = retCat.Find(x => x.Type == QType.Double);
                }

                retCat = newCat;
            }

            return retCat;
        }
        
        //returns a random Final Jeopardy question
        public JeoQuestion GetFinal()
        {
            bool found = false;
            JeoQuestion retQ = new JeoQuestion();
            do
            {
                JeoCategory<JeoQuestion> checkCat = Categories[rando.Next(Categories.Count)];
                if (checkCat.HasFinal)
                {
                    found = true;
                    List<JeoQuestion> finalReg = new List<JeoQuestion>();

                    //register any questions with QType of final, and return a random one
                    foreach (JeoQuestion q in checkCat)
                    {
                        if (q.Type == QType.Final) finalReg.Add(q);
                    }
                    retQ = finalReg[rando.Next(finalReg.Count)];
                }
            } while (!found);

            return retQ;
        }

        //returns a populated category. Alternate definition respects Daily Double ratio (1 in first round, 2 in second)
        //uses JeoCategory instead of List for the top level container to utilize the overriden ToString() method
        //todo: add option that respects AVL. Need to complete RandomCat todo first
        public JeoCategory<JeoCategory<JeoQuestion>> RandomRound(RoundType type)
        {
            JeoCategory<JeoCategory<JeoQuestion>> retCat = new JeoCategory<JeoCategory<JeoQuestion>>();

            for (int i = 0; i < 6; i++)
            {
                retCat.Add(RandomCat(type));
            }

            return retCat;
        }
        public JeoCategory<JeoCategory<JeoQuestion>> RandomRound(RoundType type, bool strictDouble)
        {
            JeoCategory<JeoCategory<JeoQuestion>> retCat = new JeoCategory<JeoCategory<JeoQuestion>>();

            int firstDouble = rando.Next(6);
            int secondDouble = rando.Next(6);
            int thirdDouble = rando.Next(6);

            while (secondDouble == thirdDouble) thirdDouble = rando.Next(6);

            if (strictDouble)
            {
                if (type == RoundType.First)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (i == firstDouble) retCat.Add(RandomCat(type, true, false));
                        else retCat.Add(RandomCat(type, false, false));
                    }
                }
                else if (type == RoundType.Second)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (i == secondDouble || i == thirdDouble) retCat.Add(RandomCat(type, true, false));
                        else retCat.Add(RandomCat(type, false, false));
                    }
                }
            }

            else
            {
                for (int i = 0; i < 6; i++)
                {
                    retCat.Add(RandomCat(type));
                }
            }

            return retCat;
        }
       
        //return a category matching the sent string
        //returns null if not found
        public JeoCategory<JeoQuestion> CatByName(string nm)
        {
            string name = nm.ToUpper();
            return Categories.Find(x => x.CatName.Contains(name));
        }

        public JeoCategory<JeoQuestion> TestCat(RoundType type, bool hasDouble, bool hasAVL)
        {
            JeoCategory<JeoQuestion> retCat = new JeoCategory<JeoQuestion>();
            retCat = CatByName("HOUSES OF THE HOLY");

            if (retCat.Count > 5 && type == RoundType.First)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();
                newCat.CatName = retCat.CatName;
                newCat.HasFirst = retCat.HasFirst;
                newCat.HasDouble = retCat.HasDouble;
                newCat.HasFinal = retCat.HasFinal;
                newCat.HasAVL = retCat.HasAVL;
                newCat.IsFull = retCat.IsFull;
                newCat.Clear();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 200 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 600 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.First));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1000 && x.Round == RoundType.First));

                //if method call has a daily double, randomly replace one of the questions with it
                //todo: replace the value that the daily double was originally at. not possible?
                if (hasDouble)
                {
                    newCat[rando.Next(4)] = retCat.Find(x => x.Type == QType.Double);
                }

                retCat = newCat;
            }
            if (retCat.Count > 5 && type == RoundType.Second)
            {
                JeoCategory<JeoQuestion> newCat = new JeoCategory<JeoQuestion>();
                newCat.CatName = retCat.CatName;
                newCat.HasFirst = retCat.HasFirst;
                newCat.HasDouble = retCat.HasDouble;
                newCat.HasFinal = retCat.HasFinal;
                newCat.HasAVL = retCat.HasAVL;
                newCat.IsFull = retCat.IsFull;
                newCat.Clear();

                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 400 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 800 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1200 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 1600 && x.Round == RoundType.Second));
                retCat.OrderBy(x => rando.Next()).FirstOrDefault();
                newCat.Add(retCat.Find(x => x.Value == 2000 && x.Round == RoundType.Second));

                //if method call has a daily double, randomly replace one of the questions with it
                //todo: replace the value that the daily double was originally at. not possible?
                if (hasDouble)
                {
                    newCat[rando.Next(4)] = retCat.Find(x => x.Type == QType.Double);
                }

                retCat = newCat;
            }

            return retCat;
        }
    }
}
