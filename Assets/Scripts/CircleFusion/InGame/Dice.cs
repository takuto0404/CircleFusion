using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CircleFusion.InGame
{
    public class Dice
    {
        public readonly AsyncReactiveProperty<bool> IsDiceRolled = new(false);
        public readonly AsyncReactiveProperty<Dice> MergedDice = new(null);
        public readonly AsyncReactiveProperty<int> DiceNumber = new(-1);
        public bool IsAnswerDice;
        public bool IsActive = true;
        private const float RollIntervalInSeconds = 0.02f;
        
        //TODO:オンライン
        public async UniTask RollDiceAsync(int rollIndex,int answer,CancellationToken gameCt)
        {
            var rollTime = GameInitialData.Instance.rollTime * rollIndex;
            DiceNumber.Value = 0;
            if (IsAnswerDice)
            {
                (int tensPlace, int unitsPlace) randomPair = (0, 0);

                for (var i = 0; i < rollTime; i++)
                {
                    randomPair = (RollDifferentNumber(randomPair.tensPlace),
                        RollDifferentNumber(randomPair.unitsPlace));
                    DiceNumber.Value = randomPair.tensPlace * 10 + randomPair.unitsPlace;
                    await UniTask.Delay(TimeSpan.FromSeconds(RollIntervalInSeconds), cancellationToken: gameCt);
                }

                DiceNumber.Value = answer;
                IsDiceRolled.Value = true;
            }
            else
            {
                var randomNumber = 0;
                for (var i = 0; i < rollTime; i++)
                {
                    randomNumber = RollDifferentNumber(randomNumber);
                    DiceNumber.Value = randomNumber;
                    await UniTask.Delay(TimeSpan.FromSeconds(RollIntervalInSeconds), cancellationToken: gameCt);
                }
                DiceNumber.Value = answer;
                IsDiceRolled.Value = true;
            }
        }

        private int RollDifferentNumber(int originalNumber)
        {
            var maxDiceValue = GameInitialData.Instance.maxDiceValue;
            while (true)
            {
                var randomNumber = UnityEngine.Random.Range(1, maxDiceValue + 1);
                if (randomNumber != originalNumber)
                {
                    return randomNumber;
                }
            }
        }
    }
}