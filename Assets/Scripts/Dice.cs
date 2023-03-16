using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

public class Dice
{
    public Dice(Dice dice)
    {
        this.Number = dice.Number;
    }
    /// <summary>
    /// サイコロの数字の実体
    /// </summary>
    public readonly ReactiveProperty<int> Number;
    
    /// <summary>
    /// このサイコロが答え用のサイコロかどうか
    /// </summary>
    private bool _isAnswerBox;
    
    /// <summary>
    /// このサイコロが活動状態かどうか
    /// </summary>
    public bool IsActive;
    
    /// <summary>
    /// kのサイコロのシャッフルが終わったかどうか
    /// </summary>
    public bool IsFinishedShuffle;

    /// <summary>
    /// このサイコロをシャッフルする
    /// </summary>
    /// <param name="shuffleLength">シャッフルする時間</param>
    /// <param name="gameCt"></param>
    public async UniTask ShuffleAsync(float shuffleLength,CancellationToken gameCt)
    {
        
    }
}