using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Jamaica
{
    public static class DiceCalculator
    {
        private static Dice _answerDice;
        private static List<Dice> _dices;
        private static Formula _currentFormula;

        public static async UniTask RollDiceAsync(CancellationToken gameCt)
        {
            var shuffleTasks = _dices.Select((dice,i) =>
                dice.RollDiceAsync(i + 1, gameCt)).ToList();
            shuffleTasks.Add(_answerDice.RollDiceAsync(_dices.Count + 1, gameCt));
            await UniTask.WhenAll(shuffleTasks);
        }

        public static int[] ExtractDiceNumbers()
        {
            return _dices.Where(dice => !dice.IsAnswerDice).Select(dice => dice.DiceNumber.Value).ToArray();
        }

        public static int GetAnswerNumber()
        {
            return _answerDice.DiceNumber.Value;
        }

        public static Dice GetLastDice()
        {
            return _dices.First(dice => dice.IsActive);
        }
        
        public static bool IsCorrectReached()
        {
            var activeDices = _dices.Where(dice => dice.IsActive).ToArray();
            if (activeDices.Length != 1) return false;
            var lastBoxNumber = activeDices.First().DiceNumber.Value;
            return lastBoxNumber == _answerDice.DiceNumber.Value;
        }

        public static void UndoStep(Hist lastHistory)
        {
            for (var i = 0; i < _dices.Count; i++)
            {
                var diceInfo = lastHistory.Dices[i].DiceInfo;
                _dices[i].DiceNumber.Value = diceInfo.diceNumber;
                _dices[i].IsActive = diceInfo.isActive;
            }
        }

        public static List<Dice> GetAllDices()
        {
            return _dices;
        }

        public static void InitializePuzzle()
        {
            _currentFormula = null;
        }

        public static void SetDice(List<Dice> dices, Dice answerDice)
        {
            _dices = new List<Dice>(dices);
            _answerDice = answerDice;
            _answerDice.IsAnswerDice = true;
        }

        public static Formula FetchCurrentFormula()
        {
            return _currentFormula;
        }
        
        public static void MergeDice(Dice firstDice, Dice secondDice, OperatorMark operatorSymbol)
        {
            var result = CalculateResult(firstDice, secondDice, operatorSymbol);
            _currentFormula = new Formula(firstDice.DiceNumber.Value, secondDice.DiceNumber.Value, operatorSymbol, result);
            firstDice.DiceNumber.Value = result;
            secondDice.IsActive = false;
            secondDice.MergedDice.Value = firstDice;
        }
        
        private static int CalculateResult(Dice firstDice, Dice secondDice, OperatorMark operatorSymbol)
        {
            var value1 = firstDice.DiceNumber.Value;
            var value2 = secondDice.DiceNumber.Value;
            switch (operatorSymbol)
            {
                case OperatorMark.Plus:
                    return value1 + value2;
                case OperatorMark.Minus:
                    return value1 - value2;
                case OperatorMark.Times:
                    return value1 * value2;
                case OperatorMark.Devided:
                    return value1 / value2;
            }

            return -1;
        }
    }
}