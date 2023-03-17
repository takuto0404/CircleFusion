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
    /// View担当のNumberBoxとModel担当のDiceクラスの紐付け
    /// </summary>
    private Dictionary<Dice, NumberBox> _diceAndNumberBoxPairDic = new();
    private Dictionary<NumberBox,Dice> _numberAndDicePairDic = new();

    public bool[] CanCalculate(NumberBox one,NumberBox anotherOne)
    {
        var diceOneNumber = _numberAndDicePairDic[one].Number.Value;
        var diceAnotherOneNumber = _numberAndDicePairDic[anotherOne].Number.Value;
        return new[]
        {
            true,
            diceOneNumber >= diceAnotherOneNumber,
            true,
            diceAnotherOneNumber != 0 && diceOneNumber / diceAnotherOneNumber % 1 == 0,
            true
        };
    }

    public async UniTask RetireAsync(CancellationToken gameCt)
    {
        await gameUIView.RetireAsync(gameCt);
    }
    public void Calculation(NumberBox one,NumberBox anotherOne,OperatorMark operatorMark)
    {
        DiceModel.MergeDice(_numberAndDicePairDic[one],_numberAndDicePairDic[anotherOne],operatorMark);
    }

    public void PuzzleInit()
    {
        _diceAndNumberBoxPairDic = new Dictionary<Dice, NumberBox>();
        gameUIView.numberBoxes.ForEach(box => { _diceAndNumberBoxPairDic.Add(new Dice(), box); });

        _numberAndDicePairDic = new Dictionary<NumberBox, Dice>();
        _diceAndNumberBoxPairDic.Keys.ToList().ForEach(dice => {_numberAndDicePairDic.Add(_diceAndNumberBoxPairDic[dice],dice);});
    }

    /// <summary>
    /// パズル中の非同期処理
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask PuzzleBehaviorAsync(CancellationToken gameCt)
    {
        var keyList = _diceAndNumberBoxPairDic.Keys.ToList();
        var disposable = keyList
            .Select(dice => dice.Number.Subscribe(num => _diceAndNumberBoxPairDic[dice].SetNumberText(num))).ToList();
        disposable.Add(GameData.Timer.Subscribe(time => gameUIView.SetTimerText(time)));

        keyList.ForEach(dice => dice.IsActive.Subscribe(async _ =>
        {
            await _diceAndNumberBoxPairDic[dice].HideBoxAsync(_diceAndNumberBoxPairDic[dice.MergedDice].GetComponent<RectTransform>().position, gameCt);
            }));

        await gameCt.WaitUntilCanceled();

        disposable.ForEach(item => item.Dispose());
    }

    public async UniTask GameFinished(bool wasCleared, CancellationToken gameCt)
    {
        await gameUIView.GameFinishedAnimationAsync(wasCleared, gameCt);
    }
}