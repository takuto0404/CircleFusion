using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

public class Dice
{
    public Dice()
    {
        
    }
    public Dice(Dice dice)
    {
        Number = dice.Number;
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
    public readonly AsyncReactiveProperty<bool> IsActive = new(false);

    public Dice MergedDice = null;
    
    /// <summary>
    /// このサイコロのシャッフルが終わったかどうか
    /// </summary>
    public readonly AsyncReactiveProperty<bool> IsFinishedShuffle = new(false);

    /// <summary>
    /// このサイコロをシャッフルする
    /// </summary>
    /// <param name="shuffleLength">シャッフルする時間</param>
    /// <param name="gameCt"></param>
    public async UniTask ShuffleAsync(float shuffleLength,CancellationToken gameCt)
    {
        var randomNum = 0;
        for (var i = 0; i < shuffleLength; i++)
        {
            while (randomNum == Number.Value)
            {
                randomNum = UnityEngine.Random.Range(1, 7);
            }

            Number.Value = randomNum;
            await UniTask.Delay(TimeSpan.FromSeconds(0.02f), cancellationToken: gameCt);
        }

        IsFinishedShuffle.Value = true;
    }
}