using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Jamaica
{
    public class Dice
    {
        public readonly ReactiveProperty<int> Number = new();
        public bool IsAnswerBox;
        public bool IsActive = true;
        public readonly AsyncReactiveProperty<Dice> MergedDice = new(null);
        public readonly AsyncReactiveProperty<bool> IsFinishedShuffle = new(false);

        //TODO:時間のテキストのサイズがおかしい
        //TODO:Returnボタンが消えない
        //TODO:初回プレイ時にスコアの変更が反映されていない
        //TODO:タイマー止めるタイミングおかしい
        //TODO:スコアが大きすぎる
        //TODO:タイムが反映されるように
        //TODO:プレイヤーの特徴を分析
        public async UniTask ShuffleAsync(float shuffleLength,CancellationToken gameCt)
        {
            Number.Value = 0;
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
}