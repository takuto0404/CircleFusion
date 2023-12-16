using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Jamaica
{
    public class Dice
    {
        public readonly AsyncReactiveProperty<int> DiceNumber = new(-1);
        public bool IsAnswerDice;
        public bool IsActive = true;
        public readonly AsyncReactiveProperty<Dice> MergedDice = new(null);
        public readonly AsyncReactiveProperty<bool> IsDiceRolled = new(false);
        private const float UpdateIntervalInSeconds = 0.2f;

        //TODO:時間のテキストのサイズがおかしい
        //TODO:Returnボタンが消えない
        //TODO:初回プレイ時にスコアの変更が反映されていない
        //TODO:タイマー止めるタイミングおかしい
        //TODO:スコアが大きすぎる
        //TODO:タイムが反映されるように
        //TODO:プレイヤーの特徴を分析
        public async UniTask RollDiceAsync(float rollTime, CancellationToken gameCt)
        {
            DiceNumber.Value = 0;
            if (IsAnswerDice)
            {
                (int tensPlace, int unitsPlace) randomPair = (0, 0);

                for (var i = 0; i < rollTime; i++)
                {
                    randomPair = (RollDifferentNumber(randomPair.tensPlace),
                        RollDifferentNumber(randomPair.unitsPlace));
                    DiceNumber.Value = randomPair.tensPlace * 10 + randomPair.unitsPlace;
                    await UniTask.Delay(TimeSpan.FromSeconds(UpdateIntervalInSeconds), cancellationToken: gameCt);
                }
                IsDiceRolled.Value = true;
            }
            else
            {
                var randomNumber = 0;
                for (var i = 0; i < rollTime; i++)
                {
                    randomNumber = RollDifferentNumber(randomNumber);
                    DiceNumber.Value = randomNumber;
                    await UniTask.Delay(TimeSpan.FromSeconds(UpdateIntervalInSeconds), cancellationToken: gameCt);
                }
                IsDiceRolled.Value = true;
            }
        }

        private int RollDifferentNumber(int originalNumber)
        {
            var maxDiceFace = GameInitialData.Instance.diceMaxValue;
            while (true)
            {
                var randomNumber = UnityEngine.Random.Range(1, maxDiceFace + 1);
                if (randomNumber != originalNumber)
                {
                    return randomNumber;
                }
            }
        }
    }
}