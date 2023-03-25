using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

public class Dice
{
    public Dice()
    {
        
    }
    public Dice(DiceInfoClass diceInfo)
    {
        Number.Value = diceInfo.DiceInfo.diceNumber;
        IsActive = diceInfo.DiceInfo.isActive;
    }
    /// <summary>
    /// サイコロの数字の実体
    /// </summary>
    public readonly ReactiveProperty<int> Number = new();
    
    /// <summary>
    /// このサイコロが答え用のサイコロかどうか
    /// </summary>
    public bool IsAnswerBox;
    
    /// <summary>
    /// このサイコロが活動状態かどうか
    /// </summary>
    public bool IsActive = true;

    public readonly AsyncReactiveProperty<Dice> MergedDice = new(null);
    
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
        if (IsAnswerBox)
        {
            (int number1, int number2) randomAnswer = (0, 0);
            int randomNum1 = randomAnswer.number1;
            int randomNum2 = randomAnswer.number2;
            for (int i = 0; i < shuffleLength; i++)
            {
                while (randomNum1 == randomAnswer.number1)
                {
                    randomNum1 = UnityEngine.Random.Range(1,GameInitialData.Instance.diceMaxValue + 1);
                }

                while (randomNum2 == randomAnswer.number2)
                {
                    randomNum2 = UnityEngine.Random.Range(1,GameInitialData.Instance.diceMaxValue + 1);
                }
                randomAnswer = (randomNum1, randomNum2);
                Number.Value = randomAnswer.number1 * 10 + randomAnswer.number2;
                await UniTask.Delay(TimeSpan.FromSeconds(0.02f),cancellationToken : gameCt);
            }

            IsFinishedShuffle.Value = true;
            return;
        }
        var randomNum = 0;
        for (var i = 0; i < shuffleLength; i++)
        {
            while (randomNum == Number.Value)
            {
                randomNum = UnityEngine.Random.Range(1, GameInitialData.Instance.diceMaxValue + 1);
            }

            Number.Value = randomNum;
            await UniTask.Delay(TimeSpan.FromSeconds(0.02f), cancellationToken: gameCt);
        }

        IsFinishedShuffle.Value = true;
    }
}