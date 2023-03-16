using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameUIPresenter : MonoBehaviour
{
    /// <summary>
    /// MVPパターンのV
    /// </summary>
    [SerializeField] private GameUIView _gameUIView;
    
    /// <summary>
    /// Canvas上で線を引いてくれるクラス
    /// </summary>
    [SerializeField] private UGUILineRenderer _drawLine;
    
    /// <summary>
    /// View担当のNumberBoxとModel担当のDiceクラスの紐付け
    /// </summary>
    private Dictionary<NumberBox, Dice> _numberAndDicePairDic;

    /// <summary>
    /// パズル開始時に毎回リセットすること
    /// </summary>
    public void PuzzleInit()
    {
        
    }

    /// <summary>
    /// パズル中の非同期処理
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask PuzzleBehaviorAsync(CancellationToken gameCt)
    {
        
    }
}