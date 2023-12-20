using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CircleFusion.Share;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace CircleFusion.InGame
{
    public class GameUIPresenter : SingletonMonoBehaviour<GameUIPresenter>
    {
        [SerializeField] private GameUIView gameUIView;
        private readonly AsyncReactiveProperty<string> _formulaString = new("");
        private Dictionary<Dice, NumberBox> _diceToNumberBoxMap = new();
        private Dictionary<NumberBox, Dice> _numberBoxToDiceMap = new();

        private readonly Dictionary<OperatorSymbol, string> _operationSymbolDictionary = new()
        {
            { OperatorSymbol.Plus, "+" },
            { OperatorSymbol.Minus, "-" },
            { OperatorSymbol.Times, "×" },
            { OperatorSymbol.Devide, "÷" }
        };

        public void Calculation(NumberBox firstNumberBox, NumberBox secondNumberBox, OperatorSymbol operatorSymbol)
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

        public async UniTask<bool> EndRollAnimationAsync(CancellationToken gameCt)
        {
            var tasks = _diceToNumberBoxMap.ToList().Select(keyValue =>
                UniTask.Create(async () =>
                {
                    await keyValue.Key.IsDiceRolled.WaitAsync(gameCt);
                    await keyValue.Value.EndRollAnimationAsync(gameCt);
                }));
            await UniTask.WhenAll(tasks);
            return true;
        }

        public async UniTask HandlePuzzlePlayAsync(CancellationToken gameCt)
        {
            var uiCts = new CancellationTokenSource();
            var mergedCt =
                CancellationTokenSource.CreateLinkedTokenSource(gameCt, uiCts.Token).Token;
            var updateNumberBoxTask = UniTask.WhenAll(_diceToNumberBoxMap.Select(keyValue =>
                keyValue.Key.DiceNumber.WithoutCurrent()
                    .ForEachAsync(number => keyValue.Value.SetNumberText(number), cancellationToken: mergedCt)));
            var updateTimerTextTask = GameState.CurrentTime.WithoutCurrent()
                .ForEachAsync(time => gameUIView.SetTimerText(time), cancellationToken: mergedCt);
            var undoProcedureTask = gameUIView.BackButtonOnClickAsAsyncEnumerable()
                .ForEachAsync(_ => UndoStep(), cancellationToken: mergedCt);
            var openSettingsTask = gameUIView.SettingButtonOnClickAsAsyncEnumerable()
                .ForEachAwaitAsync(async _ =>
                {
                    await gameUIView.ProcessSettingsAsync(mergedCt);
                    await PlayerDataManager.SavePlayerDataAsync(
                        new PlayerData(GameState.Score, GameState.ComboCount, GameInitialData.Instance.diceCount,
                            GameInitialData.Instance.maxDiceValue), gameCt);
                }, cancellationToken: mergedCt);
            var hideNumberBoxTask = UniTask.WhenAll(DiceCalculator.GetAllDices().Select(dice =>
                dice.MergedDice.Where(_ => !dice.IsActive).ForEachAwaitAsync(
                    mergedDice => _diceToNumberBoxMap[dice]
                        .MergeAsync(_diceToNumberBoxMap[mergedDice].GetComponent<RectTransform>().position, mergedCt),
                    cancellationToken: mergedCt)));
            var updateFormulaTextTask = _formulaString.WithoutCurrent()
                .ForEachAsync(text => gameUIView.UpdateFormulaText(text), cancellationToken: mergedCt);
            await UniTask.WhenAny(UniTask.WaitUntilCanceled(mergedCt), updateNumberBoxTask, updateTimerTextTask,
                undoProcedureTask, openSettingsTask, hideNumberBoxTask, updateFormulaTextTask);

            uiCts.Cancel();
        }

        public void InitializePuzzle()
        {
            _formulaString.Value = "";
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

        public async UniTask<OperatorSymbol> SelectOperatorAsync(bool[] isCalculable,CancellationToken gameCt)
        {
            return await gameUIView.SelectOperatorAsync(isCalculable, gameCt);
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
            var step = PuzzleHistory.RewindHist();
            if (step == null) return;
            gameUIView.HideAll();
            DiceCalculator.UndoStep(step.Dices);
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