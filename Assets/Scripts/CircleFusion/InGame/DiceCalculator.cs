using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CircleFusion.InGame
{
    public static class DiceCalculator
    {
        private static Dice _answerDice;
        private static Formula _currentFormula;
        private static List<Dice> _dices;

        private static int CalculateResult(Dice firstDice, Dice secondDice, OperatorSymbol operatorSymbol)
        {
            var value1 = firstDice.DiceNumber.Value;
            var value2 = secondDice.DiceNumber.Value;
            return operatorSymbol switch
            {
                OperatorSymbol.Plus => value1 + value2,
                OperatorSymbol.Minus => value1 - value2,
                OperatorSymbol.Times => value1 * value2,
                OperatorSymbol.Devide => value1 / value2,
                _ => -1
            };
        }
        
        public static Formula FetchCurrentFormula()
        {
            return _currentFormula;
        }
        
        public static List<Dice> GetAllDices()
        {
            return _dices;
        }
        
        public static Dice GetLastDice()
        {
            return _dices.First(dice => dice.IsActive);
        }
        
        public static void InitializePuzzle()
        {
            _currentFormula = null;
        }
        //275,350
        
        public static bool IsCorrectReached()
        {
            var activeDices = _dices.Where(dice => dice.IsActive && !dice.IsAnswerDice).ToArray();
            if (activeDices.Length != 1) return false;
            var lastBoxNumber = activeDices.First().DiceNumber.Value;
            return lastBoxNumber == _answerDice.DiceNumber.Value;
        }
        
        public static void MergeDice(Dice firstDice, Dice secondDice, OperatorSymbol operatorSymbol)
        {
            var result = CalculateResult(firstDice, secondDice, operatorSymbol);
            _currentFormula = new Formula(firstDice.DiceNumber.Value, secondDice.DiceNumber.Value, operatorSymbol,
                result);
            firstDice.DiceNumber.Value = result;
            secondDice.IsActive = false;
            secondDice.MergedDice.Value = firstDice;
        }

        public static async UniTask<(bool isSolvable,List<string> solutionStrings)> RollDiceAsync(CancellationToken gameCt)
        {
            var solvable = false;
            int[] randomNumbers = {};
            int randomAnswerNumber = -1;
            while (!solvable)
            {
                randomNumbers = Enumerable.Range(1, _dices.Count)
                    .Select(_ => Random.Range(1, GameInitialData.Instance.maxDiceValue + 1)).ToArray();
                randomAnswerNumber = Random.Range(1, GameInitialData.Instance.maxDiceValue) * 10 +
                                         Random.Range(1, GameInitialData.Instance.maxDiceValue);
                
                await PuzzleSolver.SolvePuzzle(randomAnswerNumber, randomNumbers);
                solvable = PuzzleSolver.Solution.isSolvable;
            }
            
            var shuffleTasks = _dices.Select((dice, i) =>
                dice.RollDiceAsync(i + 1, randomNumbers[i],gameCt)).ToList();
            shuffleTasks.Add(_answerDice.RollDiceAsync(_dices.Count + 1, randomAnswerNumber,gameCt));
            await UniTask.WhenAll(shuffleTasks);
            return PuzzleSolver.Solution;
        }
        
        public static void SetDice(List<Dice> dices, Dice answerDice)
        {
            _dices = new List<Dice>(dices);
            _dices.RemoveAt(dices.Count - 1);
            _answerDice = answerDice;
            _answerDice.IsAnswerDice = true;
        }

        public static void UndoStep(List<DiceInfoClass> dices)
        {
            for (var i = 0; i < _dices.Count; i++)
            {
                var diceInfo = dices[i].DiceInfo;
                _dices[i].DiceNumber.Value = diceInfo.diceNumber;
                _dices[i].IsActive = diceInfo.isActive;
            }
        }
    }
}