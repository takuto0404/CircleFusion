using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Jamaica.Share;
using UnityEngine;

namespace Jamaica.InGame
{
    public class GameUIPresenter : SingletonMonoBehaviour<GameUIPresenter>
    {
        [SerializeField] private GameUIView gameUIView;
        private AsyncReactiveProperty<string> _formulaString;
        private Dictionary<Dice, NumberBox> _diceToNumberBoxMap = new();
        private Dictionary<NumberBox, Dice> _numberBoxToDiceMap = new();

        private readonly Dictionary<OperatorMark, string> _operationSymbolDictionary = new()
        {
            { OperatorMark.Plus, "+" },
            { OperatorMark.Minus, "-" },
            { OperatorMark.Times, "×" },
            { OperatorMark.Devide, "÷" }
        };

        public void Calculation(NumberBox firstNumberBox, NumberBox secondNumberBox, OperatorMark operatorSymbol)
        {
            DiceCalculator.MergeDice(_numberBoxToDiceMap[firstNumberBox], _numberBoxToDiceMap[secondNumberBox],
                operatorSymbol);
        }

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

        public string CreateFormulaText(Formula newFormula)
        {
            var newFormulaText =
                $"{newFormula.FirstDice} {_operationSymbolDictionary[newFormula.OperatorSymbol]} {newFormula.SecondDice} = {newFormula.Result}\n";
            _formulaString.Value += newFormulaText;
            return _formulaString;
        }

        public async UniTask EndGameAnimationAsync(bool isCleared, CancellationToken gameCt)
        {
            await gameUIView.EndGameAnimationAsync(isCleared, gameCt);
        }

        public async UniTask EndRollAnimationAsync(CancellationToken gameCt)
        {
            await UniTask.WhenAll(_diceToNumberBoxMap.ToList().Select(keyValue =>
                keyValue.Key.IsDiceRolled.WithoutCurrent().ForEachAwaitAsync(async _ =>
                    await keyValue.Value.EndRollAnimationAsync(gameCt), cancellationToken: gameCt)));
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
            var undoProcedureTask = gameUIView.BackButtonOnClickAsAsyncEnumerable()
                .ForEachAsync(_ => UndoStep(), cancellationToken: mergedCt);
            var openSettingsTask = gameUIView.SettingButtonOnClickAsAsyncEnumerable()
                .ForEachAwaitAsync(async _ => await gameUIView.ProcessSettingsAsync(mergedCt), cancellationToken: mergedCt);
            var hideNumberBoxTask = UniTask.WhenAll(DiceCalculator.GetAllDices().Select(dice => dice.MergedDice
                .Where(_ => !dice.IsActive)
                .ForEachAwaitAsync(
                    async mergedDice => await _diceToNumberBoxMap[dice]
                        .MergeAsync(_diceToNumberBoxMap[mergedDice].GetComponent<RectTransform>().position, mergedCt),
                    cancellationToken: mergedCt)));
            var updateFormulaTextTask = _formulaString.WithoutCurrent()
                .ForEachAsync(text => gameUIView.UpdateFormulaText(text), cancellationToken: mergedCt);
            await UniTask.WhenAny(UniTask.WaitUntilCanceled(mergedCt), updateNumberBoxTask, updateTimerTextTask,
                undoProcedureTask, openSettingsTask, hideNumberBoxTask);

            uiCts.Cancel();
        }

        public void InitializePuzzle()
        {
            gameUIView.InitializePuzzle();
            _diceToNumberBoxMap = new Dictionary<Dice, NumberBox>();
            foreach (var numberBox in gameUIView.numberBoxes)
            {
                _diceToNumberBoxMap.Add(new Dice(), numberBox);
            }

            var answerDice = new Dice();
            _diceToNumberBoxMap.Add(answerDice, gameUIView.answerNumberBox);

            DiceCalculator.SetDice(_diceToNumberBoxMap.Keys.ToList(), answerDice);

            _numberBoxToDiceMap =
                _diceToNumberBoxMap.ToDictionary(keyValuePair => keyValuePair.Value, keyValuePair => keyValuePair.Key);
        }

        public async UniTask MoveToCenterAsync(CancellationToken gameCt)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f), cancellationToken: gameCt);
            await gameUIView.MoveToCenterAsync(_diceToNumberBoxMap[DiceCalculator.GetLastDice()], gameCt);
        }

        public async UniTask ShowMessageAsync()
        {
            await gameUIView.ShowMessageAsync();
        }

        public void UpdateFormulaText(string formulaString)
        {
            _formulaString.Value = formulaString;
        }

        private void UndoStep()
        {
            var step = JamaicaHistory.RewindHist();
            if (step == null) return;
            gameUIView.HideAll();
            DiceCalculator.UndoStep(step);
            UpdateFormulaText(step.FormulaText);
            _diceToNumberBoxMap.ToList().ForEach(keyValue =>
            {
                if (keyValue.Key.IsActive) keyValue.Value.ShowNumberBox();
            });
        }

        public async UniTask WaitForRetirementAsync(CancellationToken gameCt)
        {
            await gameUIView.RetireButtonOnClickAsync(gameCt);
        }
    }
}