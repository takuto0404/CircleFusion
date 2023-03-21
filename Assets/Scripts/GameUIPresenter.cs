using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks.Linq;
using UniRx;


public class GameUIPresenter : SingletonMonoBehaviour<GameUIPresenter>
{
    /// <summary>
    /// MVPパターンのV
    /// </summary>
    [SerializeField] private GameUIView gameUIView;
    
    /// <summary>
    /// 演算記号の種類と文字列のディク
    /// </summary>
    private readonly Dictionary<OperatorMark, string> _operatorDic = new ()
    {
        { OperatorMark.Plus, "+" },
        { OperatorMark.Minus, "-" },
        { OperatorMark.Times, "×" },
        { OperatorMark.Devided, "÷" }
    };

    

    /// <summary>
    /// View担当のNumberBoxとModel担当のDiceクラスの紐付け
    /// </summary>
    private Dictionary<Dice, NumberBox> _diceAndNumberBoxPairDic = new();
    private Dictionary<NumberBox,Dice> _numberAndDicePairDic = new();

    public string GetFormulaText()
    {
        return gameUIView.GetFormulaText();
    }
    
    public bool[] CanCalculate(NumberBox one,NumberBox anotherOne)
    {
        var diceOneNumber = _numberAndDicePairDic[one].Number.Value;
        var diceAnotherOneNumber = _numberAndDicePairDic[anotherOne].Number.Value;
        return new[]
        {
            true,
            diceOneNumber >= diceAnotherOneNumber,
            true,
            diceAnotherOneNumber != 0 && ((float)diceOneNumber / (float)diceAnotherOneNumber) % 1 == 0,
            true
        };
    }

    public async UniTask RetireAsync(CancellationToken gameCt)
    {
        await gameUIView.RetireButtonOnClickAsync(gameCt);
    }
    public void Calculation(NumberBox one,NumberBox anotherOne,OperatorMark operatorMark)
    {
        DiceModel.MergeDice(_numberAndDicePairDic[one],_numberAndDicePairDic[anotherOne],operatorMark);
    }
    
    public string MakeFormulaText(Formula newFormula)
    {
        var newFormulaText = 
            $"{newFormula.One} {_operatorDic[newFormula.OperatorMark]} {newFormula.AnotherOne} = {newFormula.Answer}\n";
        var text = GameUIPresenter.Instance.GetFormulaText();
        text += newFormulaText;
        return text;
    }

    public void PuzzleInit()
    {
        _diceAndNumberBoxPairDic = new Dictionary<Dice, NumberBox>();
        gameUIView.numberBoxes.ForEach(box => { _diceAndNumberBoxPairDic.Add(new Dice(), box); });
        var answerDice = new Dice();
        DiceModel.SetDice(_diceAndNumberBoxPairDic.Keys.ToList(),answerDice);
        _diceAndNumberBoxPairDic.Add(answerDice,gameUIView.answerBox);

        _numberAndDicePairDic = new Dictionary<NumberBox, Dice>();
        _diceAndNumberBoxPairDic.Keys.ToList().ForEach(dice => {_numberAndDicePairDic.Add(_diceAndNumberBoxPairDic[dice],dice);});
        
        gameUIView.PuzzleInit();
    }
    

    /// <summary>
    /// パズル中の非同期処理
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask PuzzleBehaviorAsync(CancellationToken gameCt)
    {
        bool isGameRestart = false;
        
        var disposable = _diceAndNumberBoxPairDic
            .Select(keyValue => keyValue.Key.Number.Subscribe(num => keyValue.Value.SetNumberText(num))).ToList();
        disposable.Add(GameData.Timer.Subscribe(time => gameUIView.SetTimerText(time)));
        disposable.Add(gameUIView.BackButtonOnClickAsObservable().Subscribe(_ =>
        {
            var step = JamaicaHistory.BackHist();
            if (step == null) return;
            DiceModel.BackStep(step);
            SetFormulaText(step.FormulaText);
            _diceAndNumberBoxPairDic.ToList().ForEach(keyValue =>
            {
                if(keyValue.Key.IsActive) keyValue.Value.ShowBox();
            });
        }));
        gameUIView.SettingButtonOnClickAsObservable()
            .Subscribe(async _ =>
            {
                isGameRestart = true;
                await gameUIView.SettingProgress(gameCt);
            });

        var diceList = _diceAndNumberBoxPairDic.Keys.ToList();
        var disposeList = diceList.Select(dice => dice.MergedDice.Where(_ => !dice.IsActive).Subscribe(async merged =>
            {
                await _diceAndNumberBoxPairDic[dice]
                    .HideBoxAsync(_diceAndNumberBoxPairDic[merged].GetComponent<RectTransform>().position, gameCt);
            }))
            .ToList();
        disposeList.AddRange(_diceAndNumberBoxPairDic.ToList().Select(keyValue => keyValue.Key.IsFinishedShuffle.Subscribe(async _ => await keyValue.Value.FinishedShuffleAnimationAsync(gameCt))).ToList());

        var result = await UniTask.WhenAny(UniTask.WaitUntilCanceled(gameCt),UniTask.WaitUntil(() =>isGameRestart,cancellationToken:gameCt));
        disposable.ForEach(item => item.Dispose());
    }

    public void SetFormulaText(string formulaText)
    {
        gameUIView.SetFormulaText(formulaText);
    }
    public async UniTask GameFinished(bool wasCleared, CancellationToken gameCt)
    {
        await gameUIView.GameFinishedAnimationAsync(wasCleared, gameCt);
    }

    public async UniTask MoveToEqualAsync(CancellationToken gameCt)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.75),cancellationToken:gameCt);
        await gameUIView.MoveToEqualAsync(_diceAndNumberBoxPairDic[DiceModel.GetLastDice()],gameCt);
    }
}