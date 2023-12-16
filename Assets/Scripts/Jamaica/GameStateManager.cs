using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Jamaica
{
    public static class GameStateManager
    {
        /// <summary>
        /// ゲーム初期化時に行う処理
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void GameInit()
        {
            PuzzleFlowAsync().Forget();
        }

        /// <summary>
        /// ゲームのおおまかな流れを進行する非同期メソッド
        /// </summary>
        private static async UniTask PuzzleFlowAsync()
        {
            while (true)
            {
                var gameCts = new CancellationTokenSource();
                await PlayerDataManager.LoadPlayerDataAsync(gameCts.Token);
                var data = PlayerDataManager.PlayerData;
                
                GameData.CurrentTime.Value = 0;
                GameData.Score = data.Score;
                GameData.ComboCount = data.Combo;
                GameInitialData.Instance.numberOfDice = data.NumberOfDice;
                GameInitialData.Instance.diceMaxValue = data.DiceMaxNumber;

                GameUIPresenter.Instance.PuzzleInit();
                DiceCalculator.InitializePuzzle();
                JamaicaHistory.PuzzleInit();

                var uiTask = GameUIPresenter.Instance.PuzzleBehaviorAsync(gameCts.Token);

                (bool canSolve, List<string> solutions) solveResult = (false,null);
                var i = 0;
                while (!solveResult.canSolve)
                {
                    if(i != 0)await GameUIPresenter.Instance.ShowNotice();
                    await DiceCalculator.RollDiceAsync(gameCts.Token);
                    solveResult = JamaicaSolver.SolveJamaica(DiceCalculator.GetAnswerNumber(), DiceCalculator.ExtractDiceNumbers());
                    i++;
                }
                CountTimerAsync(gameCts.Token).Forget();
                
                GameData.FormulaString = solveResult.solutions;

                JamaicaHistory.SetInitHist(DiceCalculator.GetAllDices());
            
                var retireTask = GameUIPresenter.Instance.RetireAsync(gameCts.Token);
                var gameTask = PlayerController.Instance.PlayerBehavior(gameCts.Token);
                var clearTask = UniTask.WaitUntil(DiceCalculator.IsCorrectReached, cancellationToken: gameCts.Token);
                var result = await UniTask.WhenAny(retireTask, gameTask, uiTask, clearTask);

                if (result == 3)
                {
                    await GameUIPresenter.Instance.MoveToEqualAsync(gameCts.Token);
                }
                
                gameCts.Cancel();
                if (result == 2)
                {
                    continue;
                }
                var menuCts = new CancellationTokenSource();
                if (result == 0)
                {
                    GameData.GameOver();
                    await GameOveredAsync(menuCts.Token);
                }

                if (result == 3)
                {
                    GameData.GameClear();
                    await GameClearedAsync(menuCts.Token);
                }

                await PlayerDataManager.SavePlayerDataAsync(new PlayerData(GameData.Score, GameData.ComboCount,
                    GameInitialData.Instance.numberOfDice, GameInitialData.Instance.diceMaxValue),menuCts.Token);
                menuCts.Cancel();
            }
        }


        /// <summary>
        /// パズルソルブ中にタイマーのカウントを行う非同期メソッド
        /// </summary>
        private static async UniTask CountTimerAsync(CancellationToken gameCt)
        {
            GameData.CurrentTime.Value = 0;

            var startTime = DateTime.Now;
            while (!gameCt.IsCancellationRequested)
            {
                var diff = DateTime.Now - startTime;
                GameData.CurrentTime.Value = (float)diff.TotalSeconds;
                await UniTask.DelayFrame(1, cancellationToken: gameCt);
            }
        }

        /// <summary>
        /// ゲームオーバー時のアニメーション等の非同期メソッド
        /// </summary>
        private static async UniTask GameOveredAsync(CancellationToken gameCt)
        {
            await GameUIPresenter.Instance.GameFinished(false, gameCt);
        }

        /// <summary>
        /// ゲームクリア時のアニメーション等の非同期メソッド
        /// </summary>
        private static async UniTask GameClearedAsync(CancellationToken gameCt)
        {
            await GameUIPresenter.Instance.GameFinished(true, gameCt);
        }
    }
}