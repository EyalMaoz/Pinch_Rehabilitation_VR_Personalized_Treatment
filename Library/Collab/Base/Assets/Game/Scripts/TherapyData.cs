using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Game.Scripts
{
    public class TherapyData
    {
        private static System.Random RandGenerator = new System.Random();

        public enum PinchType
        {
            None, Pad2, Pad3, Tip2, Tip3, Lateral
        }

        public const int _DifficultyMax = 10;
        public const int _DifficultyRange = 10;
        public const int _DifficultyMin = 1;
        // need to get this from user:
        public static int dpT2 = 7;
        public static int dpP2 = 9;
        public static int dpT3 = 8;
        public static int dpP3 = 9;
        //public int dpL2 = 5;

        [System.Serializable]
        public class TreatmentPlan
        {
            //public string sceneID;
            public int TreatmentNumber;
            public string CreationTime;
            public List<Challenge> Plan;
            // TODO add more relevent info
        }
        
        [System.Serializable]
        public class PinchAction
        {
            public PinchType Type;
            public int DifficultyLevel;// dm
            public int DifficultyPerPlayer;// dp

            public PinchAction(PinchType t, int dp, int diffScore = -1)
            {
                Type = t;
                DifficultyPerPlayer = dp;
                if (diffScore == -1) DifficultyLevel = RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);
                else DifficultyLevel = diffScore;
            }
        }

        [System.Serializable]
        public class Challenge
        {
            public List<PinchAction> ActionsList = null;
            public string Name;
            public Challenge Source = null;
            public float LF = 0f;

            private int version = 1;

            public Challenge(string n, List<PinchAction> Actions = null)
            {
                if (Actions == null)
                {
                    ActionsList = new List<PinchAction>();
                }
                else ActionsList = Actions;
                Name = n;
            }

            public Challenge(Challenge original)
            {
                ActionsList = new List<PinchAction>();
                foreach (PinchAction a in original.ActionsList)
                {
                    ActionsList.Add(new PinchAction(a.Type, a.DifficultyPerPlayer, a.DifficultyLevel));
                }
                version = original.version + 1;
                Name = original.Name;
                LF = original.LF;
                if (original.Source == null)
                {
                    this.Source = original;
                }
                else
                {
                    this.Source = original.Source;
                }
            }

            public int GetChallengeDifficulty()
            {
                int answer = 0;
                foreach (PinchAction action in ActionsList)
                {
                    answer += action.DifficultyLevel + action.DifficultyPerPlayer;
                }
                return answer;
            }

            public int CalculateCR(int PT, int ST)
            {
                int RL = (PT + 2) - (ST + 1);
                int PR = 2 * RL - 1;
                return PT - PR;
            }

            public void PrintActions()
            {
                Console.WriteLine(Name + "_Ver" + version);
                Console.Write("{ ");
                foreach (PinchAction a in ActionsList)
                {
                    Console.Write(a.Type.ToString() + "[" + a.DifficultyLevel + "] ");
                }
                Console.WriteLine("}");
            }

            public void AddRandomAction(int Difficulty = -1)
            {
                PinchType newActType = (PinchType)(RandGenerator.Next(0, 4 + 1));
                if (Difficulty == -1)
                {
                    Difficulty = RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);
                }
                switch (newActType)
                {
                    case PinchType.Tip2:
                        ActionsList.Add(new PinchAction(newActType, dpT2, Difficulty));
                        break;
                    case PinchType.Pad2:
                        ActionsList.Add(new PinchAction(newActType, dpP2, Difficulty));
                        break;
                    case PinchType.Tip3:
                        ActionsList.Add(new PinchAction(newActType, dpT3, Difficulty));
                        break;
                    case PinchType.Pad3:
                        ActionsList.Add(new PinchAction(newActType, dpP3, Difficulty));
                        break;
                    //case PinchType.Lateral:
                    //    ActionsList.Add(new PinchAction(newActType, dpL2, Difficulty));
                    //    break;
                }
            }

            public bool LowerDifficulty()
            {
                for (int i = ActionsList.Count - 1; i >= 0; i--)
                {
                    if (ActionsList[i].DifficultyLevel > _DifficultyMin)
                    {
                        ActionsList[i].DifficultyLevel -= 1;
                        return true;
                    }
                }
                return false;
            }

            public bool HigherDifficulty()
            {
                for (int i = ActionsList.Count - 1; i >= 0; i--)
                {
                    if (ActionsList[i].DifficultyLevel < _DifficultyMax)
                    {
                        ActionsList[i].DifficultyLevel += 1;
                        return true;
                    }
                }
                return false;
            }

            public void ReplaceWithRandomAction()
            {
                int actionIndex = (RandGenerator.Next(0, ActionsList.Count));
                int newActionIndex = (RandGenerator.Next(0, 4 + 1));
                while (actionIndex == newActionIndex)
                {
                    newActionIndex = (RandGenerator.Next(0, 4 + 1));
                }

                ActionsList.RemoveAt(actionIndex);
                PinchType newActType = (PinchType)newActionIndex;
                int Difficulty = RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);

                switch (newActType)
                {
                    case PinchType.Tip2:
                        ActionsList.Add(new PinchAction(newActType, dpT2, Difficulty));
                        break;
                    case PinchType.Pad2:
                        ActionsList.Add(new PinchAction(newActType, dpP2, Difficulty));
                        break;
                    case PinchType.Tip3:
                        ActionsList.Add(new PinchAction(newActType, dpT3, Difficulty));
                        break;
                    case PinchType.Pad3:
                        ActionsList.Add(new PinchAction(newActType, dpP3, Difficulty));
                        break;
                    //case PinchType.Lateral:
                    //    ActionsList.Add(new PinchAction(newActType, dpL2, Difficulty));
                    //    break;
                }
            }
        }

        public static class Algorithm
        {
            const float CL = 1f;// Coefficient of Learning
            const long StuckRefreshMaxCount = 3;// Coefficient of Learning
            static long stuckCounter = 0;

            internal static Challenge CreateNewChallenge(Challenge original, int PT, int ST)
            {
                Challenge newChal;
                if (original.Source == null)
                    newChal = new Challenge(original);
                else
                    newChal = new Challenge(original.Source);

                bool shouldCheckCR = true;
                int CR = original.CalculateCR(PT, ST);

                while (true)
                {

                    if (shouldCheckCR)// depends if we get back to this step or only the next step
                    {
                        if (CR >= 0)
                        {
                            newChal.AddRandomAction();
                        }
                        else
                        {
                            //newChal.LowerDifficulty();
                            if (!newChal.LowerDifficulty())
                            {// All the actions are at the minimum difficulty
                                newChal.AddRandomAction(_DifficultyMin);
                            }
                        }
                    }

                    float Hdif = newChal.GetChallengeDifficulty() - original.GetChallengeDifficulty();
                    newChal.LF = CL * CR;

                    if (Hdif < newChal.LF)
                    {
                        if (!newChal.HigherDifficulty())
                        {
                            shouldCheckCR = true;
                        }
                        else
                            shouldCheckCR = false;
                        //shouldCheckCR = true;
                        //continue;
                    }
                    else if (Hdif > newChal.LF)
                    {
                        if (!newChal.LowerDifficulty())
                        {
                            stuckCounter++;
                            if (stuckCounter == StuckRefreshMaxCount)
                            {
                                stuckCounter = 0;
                                newChal.ReplaceWithRandomAction();
                            }
                            break;
                        }
                        else shouldCheckCR = false;
                    }
                    else if (Hdif == newChal.LF) break;
                }
                //newChal.LF /= 10;
                return newChal;
            }

            internal static List<Challenge> SinglePointCrossover(Challenge c1, Challenge c2)
            {
                int crossPoint = (RandGenerator.Next(1, Math.Min(c1.ActionsList.Count, c2.ActionsList.Count)));
                Challenge offspring1 = new Challenge(c1.Name);
                Challenge offspring2 = new Challenge(c2.Name);

                for (int i = 0; i < crossPoint; i++)
                {
                    offspring1.ActionsList.Add(new PinchAction(c1.ActionsList[i].Type, c1.ActionsList[i].DifficultyPerPlayer, c1.ActionsList[i].DifficultyLevel));
                    offspring2.ActionsList.Add(new PinchAction(c2.ActionsList[i].Type, c2.ActionsList[i].DifficultyPerPlayer, c2.ActionsList[i].DifficultyLevel));
                }

                for (int i = crossPoint; i < c1.ActionsList.Count; i++)
                {
                    offspring2.ActionsList.Add(new PinchAction(c1.ActionsList[i].Type, c1.ActionsList[i].DifficultyPerPlayer, c1.ActionsList[i].DifficultyLevel));
                }

                for (int i = crossPoint; i < c2.ActionsList.Count; i++)
                {
                    offspring1.ActionsList.Add(new PinchAction(c2.ActionsList[i].Type, c2.ActionsList[i].DifficultyPerPlayer, c2.ActionsList[i].DifficultyLevel));
                }


                return new List<Challenge>() { offspring1, offspring2 };
            }

            internal static List<Challenge> SelectChromForNextLevel(List<Challenge> chromosomePool)
            {
                List<Challenge> theChosenOnes = new List<Challenge>();
                Dictionary<float, Challenge> challengeDictionary = new Dictionary<float, Challenge>();
                float maxDifficulty = 0;
                for (int i = 0; i < chromosomePool.Count; i++)
                {
                    if (chromosomePool[i].GetChallengeDifficulty() > maxDifficulty)
                        maxDifficulty = chromosomePool[i].GetChallengeDifficulty();
                }

                for (int i = 0; i < chromosomePool.Count; i++)
                {
                    float diff = Math.Abs(chromosomePool[i].GetChallengeDifficulty() / maxDifficulty - chromosomePool[i].LF / 10);
                    while (challengeDictionary.ContainsKey(diff)) diff += 0.0001f;
                    challengeDictionary.Add(Math.Abs(diff), chromosomePool[i]);
                }

                var l = challengeDictionary.OrderBy(key => key.Key);
                var sortedDictionary = l.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

                for (int i = 0; i < challengeDictionary.Count / 2; i++)
                {
                    theChosenOnes.Add(sortedDictionary.ElementAt(i).Value);
                }

                return theChosenOnes;
            }

            public static List<Challenge> GenerateNewLevel(List<Challenge> previousLevel, int PT, int ST)
            {
                List<Challenge> newLevel = new List<Challenge>();
                for (int i = 0; i < previousLevel.Count; i++)
                {
                    newLevel.Add(CreateNewChallenge(previousLevel[i], PT, ST));// generating a new better one
                    newLevel.Add(previousLevel[i]);// adding the previous for crosspoint
                }
                // Now we do the crosspoint
                newLevel = SelectChromForNextLevel(newLevel);

                return newLevel;
            }
        }


    }
}
