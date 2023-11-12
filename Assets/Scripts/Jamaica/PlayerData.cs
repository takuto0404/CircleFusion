using System;
using MemoryPack;

namespace Jamaica
{
    [MemoryPackable]
    public partial class PlayerData
    {
        public PlayerData(int score, int combo, int numberOfDice, int diceMaxNumber)
        {
            Score = score;
            Combo = combo;
            NumberOfDice = numberOfDice;
            DiceMaxNumber = diceMaxNumber;
        }
        public readonly int Score;
        public readonly int Combo;
        public readonly int NumberOfDice;
        public readonly int DiceMaxNumber;
    }
}