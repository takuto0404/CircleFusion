using System.Collections.Generic;

namespace CircleFusion.InGame
{
    public static class PuzzleHistory
    {
        private static Stack<Hist> _histories;

        public static Hist RewindHist()
        {
            if (_histories.Count == 1) return null;
            _histories.Pop();
            return LastHist();
        }

        public static Hist LastHist()
        {
            return _histories.Peek();
        }
        
        public static void InitializePuzzle()
        {
            _histories = new Stack<Hist>();
        }

        public static void SetHist(List<Dice> dices, string formulaText)
        {
            _histories.Push(new Hist(dices, formulaText));
        }
    }
}