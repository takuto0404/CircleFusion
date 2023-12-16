using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;

namespace Jamaica
{
    public class GameUIPresenter : SingletonMonoBehaviour<GameUIPresenter>
    {
        [SerializeField] private GameUIView gameUIView;

        private Dictionary<Dice, NumberBox> _diceToNumberBoxMap = new();
        private Dictionary<NumberBox, Dice> _numberBoxToDiceMap = new();

        private readonly Dictionary<OperatorMark, string> _operationSymbolDictionary = new()
        {
            { OperatorMark.Plus, "+" },
            { OperatorMark.Minus, "-" },
            { OperatorMark.Times, "×" },
            { OperatorMark.Devided, "÷" }
        };

        public bool[] CheckCalculations(NumberBox firstNumberBox, NumberBox secondNumberBox)
        {
            var firstNumber = _numberBoxToDiceMap[firstNumberBox].DiceNumber.Value;
            var secondNumber = _numberBoxToDiceMap[secondNumberBox].DiceNumber.Value;
            return new[]
            {
                true,
                firstNumber >= secondNumber,
                true,
                secondNumber != 0 && (float)firstNumber / (float)secondNumber % 1 == 0,
                true
            };
        }

        public async UniTask WaitForRetirementAsync(CancellationToken gameCt)
        {
            await gameUIView.RetireButtonOnClickAsync(gameCt);
        }

        public void Calculation(NumberBox firstNumberBox, NumberBox secondNumberBox, OperatorMark operatorSymbol)
        {
            DiceCalculator.MergeDice(_numberBoxToDiceMap[firstNumberBox], _numberBoxToDiceMap[secondNumberBox],
                operatorSymbol);
        }

        public string CreateFormulaText(Formula newFormula)
        {
            var newFormulaText =
                $"{newFormula.FirstDice} {_operationSymbolDictionary[newFormula.OperatorSymbol]} {newFormula.SecondDice} = {newFormula.Result}\n";
            return gameUIView.GetFormulaText() + newFormulaText;
        }

        public void InitializePuzzle()
        {
            gameUIView.PuzzleInit();
            _diceToNumberBoxMap = new Dictionary<Dice, NumberBox>();
            foreach (var numberBox in gameUIView.numberBoxes)
            {
                _diceToNumberBoxMap.Add(new Dice(), numberBox);
            }

            var answerDice = new Dice();
            _diceToNumberBoxMap.Add(answerDice, gameUIView.answerBox);

            DiceCalculator.SetDice(_diceToNumberBoxMap.Keys.ToList(), answerDice);

            _numberBoxToDiceMap =
                _diceToNumberBoxMap.ToDictionary(keyValuePair => keyValuePair.Value, keyValuePair => keyValuePair.Key);
        }

        private void UndoStep()
        {
            var step = JamaicaHistory.BackHist();
            if (step == null) return;
            gameUIView.HideEverything();
            DiceCalculator.UndoStep(step);
            UpdateFormulaText(step.FormulaText);
            _diceToNumberBoxMap.ToList().ForEach(keyValue =>
            {
                if (keyValue.Key.IsActive) keyValue.Value.ShowBox();
            });
        }

        public async UniTask EndRollAnimationAsync(CancellationToken gameCt)
        {
            await UniTask.WhenAll(_diceToNumberBoxMap.ToList().Select(keyValue =>
                keyValue.Key.IsDiceRolled.WithoutCurrent().ForEachAwaitAsync(async _ =>
                    await keyValue.Value.FinishedShuffleAnimationAsync(gameCt), cancellationToken: gameCt)));
        }

        public async UniTask HandlePuzzlePlayAsync(CancellationToken gameCt)
        {
            var uiCts = new CancellationTokenSource();
            var mergedCt =
                CancellationTokenSource.CreateLinkedTokenSource(gameCt, uiCts.Token).Token;
            var updateNumberBoxTask = UniTask.WhenAll(_diceToNumberBoxMap.Select(keyValue =>
                keyValue.Key.DiceNumber.WithoutCurrent().ForEachAsync(number => keyValue.Value.SetNumberText(number),
                    cancellationToken: mergedCt)));
            var updateTimerTextTask = GameState.CurrentTime.WithoutCurrent()
                .ForEachAsync(time => gameUIView.SetTimerText(time), cancellationToken: mergedCt);
            var undoProcedureTask = gameUIView.BackButtonOnClickAsAsyncEnumerable().ForEachAsync(_ => UndoStep(), cancellationToken: mergedCt);
            var openSettingsTask = gameUIView.SettingButtonOnClickAsAsyncEnumerable()
                .ForEachAwaitAsync(async _ => await gameUIView.SettingProgress(mergedCt), cancellationToken:mergedCt);
            var hideNumberBoxTask = UniTask.WhenAll(DiceCalculator.GetAllDices().Select(dice => dice.MergedDice
                .Where(_ => !dice.IsActive)
                .ForEachAwaitAsync(
                    async mergedDice => await _diceToNumberBoxMap[dice]
                        .HideBoxAsync(_diceToNumberBoxMap[mergedDice].GetComponent<RectTransform>().position, mergedCt),
                    cancellationToken: mergedCt)));
            await UniTask.WhenAny(UniTask.WaitUntilCanceled(mergedCt), updateNumberBoxTask, updateTimerTextTask,
                undoProcedureTask, openSettingsTask, hideNumberBoxTask);

            uiCts.Cancel();
        }

        public async UniTask ShowMessageAsync()
        {
            await gameUIView.ShowMessageAsync();
        }

        public void UpdateFormulaText(string formulaString)
        {
            gameUIView.UpdateFormulaText(formulaString);
        }

        public async UniTask EndGameAnimationAsync(bool isCleared, CancellationToken gameCt)
        {
            await gameUIView.GameFinishedAnimationAsync(isCleared, gameCt);
        }

        public async UniTask MoveToCenterAsync(CancellationToken gameCt)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f), cancellationToken: gameCt);
            await gameUIView.MoveToEqualAsync(_diceToNumberBoxMap[DiceCalculator.GetLastDice()], gameCt);
        }
    }
}