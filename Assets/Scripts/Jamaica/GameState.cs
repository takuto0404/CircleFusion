using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Jamaica
{
    public static class GameState
    {
        public static int ComboCount = 0;
        public static int Score = 0;
        public static List<string> FormulaString;
        public static readonly AsyncReactiveProperty<float> CurrentTime = new(0);

        public static void GameClear()
        {
            ComboCount++;
            Score += ComboCount * (GameInitialData.Instance.diceCount + GameInitialData.Instance.maxDiceValue);
        }

        public static void GameOver()
        {
            ComboCount = 0;
        }

        public static void InitializePuzzle(int comboCount,int score)
        {
            CurrentTime.Value = 0;
            ComboCount = comboCount;
            Score = score;
        }
    }
}