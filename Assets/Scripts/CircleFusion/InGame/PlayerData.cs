using MemoryPack;

namespace CircleFusion.InGame
{
    [MemoryPackable]
    public partial class PlayerData
    {
        public PlayerData(int score, int comboCount, int diceCount, int diceMax)
        {
            Score = score;
            ComboCount = comboCount;
            DiceCount = diceCount;
            DiceMax = diceMax;
        }
        public readonly int ComboCount;
        public readonly int DiceCount;
        public readonly int DiceMax;
        public readonly int Score;
    }
}