using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Jamaica
{
    public static class GameData
    {
        public static int ComboCount = 0;
        public static int Score = 0;
        public static List<string> FormulaString;
        public static readonly AsyncReactiveProperty<float> CurrentTime = new(0);

        public static void GameClear()
        {
            ComboCount++;
            Score += ComboCount * (GameInitialData.Instance.numberOfDice + GameInitialData.Instance.diceMaxValue);
        }

        public static void GameOver()
        {
            ComboCount = 0;
        }
    }
}