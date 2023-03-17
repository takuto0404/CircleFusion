using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;

public class GameUIPresenter : MonoBehaviour
{
    /// <summary>
    /// MVPパターンのV
    /// </summary>
    [SerializeField] private GameUIView gameUIView;

    /// <summary>
    /// Canvas上で線を引いてくれるクラス
    /// </summary>
    [SerializeField] private UGUILineRenderer drawLine;

    /// <summary>
    /// View担当のNumberBoxとModel担当のDiceクラスの紐付け
    /// </summary>
    private Dictionary<Dice, NumberBox> _numberAndDicePairDic;
    

    /// <summary>
    /// パズル中の非同期処理
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask PuzzleBehaviorAsync(CancellationToken gameCt)
    {
        var keyList = _numberAndDicePairDic.Keys.ToList();
        var disposable = keyList.Select(dice => dice.Number.Subscribe(num => _numberAndDicePairDic[dice].SetNumberText(num))).ToList();
        disposable.Add(GameData.Timer.Subscribe(time => gameUIView.SetTimerText(time)));
        
        await gameCt.WaitUntilCanceled();
        
        disposable.ForEach(item => item.Dispose());
    }
}