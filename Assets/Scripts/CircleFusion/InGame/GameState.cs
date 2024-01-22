using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CircleFusion.InGame
{
    public static class GameState
    {
        public static readonly AsyncReactiveProperty<float> CurrentTime = new(0);
        public static int ComboCount;
        public static int Score;
        public static List<string> FormulaStrings;

        public static void GameClear()
        {
            ComboCount++;
            Score += ComboCount + GameInitialData.Instance.diceCount * 2 + GameInitialData.Instance.maxDiceValue;
        }

        public static void GameOver()
        {
            ComboCount = 0;
        }

        public static void InitializePuzzle(int comboCount, int score)
        {
            CurrentTime.Value = 0;
            ComboCount = comboCount;
            Score = score;
        }
    }
}