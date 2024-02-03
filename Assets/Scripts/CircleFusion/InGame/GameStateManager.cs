using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CircleFusion.InGame
{
    public static class GameStateManager
    {
        private const int TimeLimit = 60;
        [RuntimeInitializeOnLoadMethod]
        private static void GameInit()
        {
            PuzzleFlowAsync().Forget();
        }

        private static async UniTask CountTimerAsync(CancellationToken gameCt)
        {
            GameState.CurrentTime.Value = TimeLimit;

            var startTime = DateTime.Now;
            while (!gameCt.IsCancellationRequested)
            {
                var diff = DateTime.Now - startTime;
                GameState.CurrentTime.Value = TimeLimit - (float)diff.TotalSeconds;
                await UniTask.DelayFrame(1, cancellationToken: gameCt);
                if (GameState.CurrentTime.Value <= 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: gameCt);
                    return;
                }
            }
        }

        private static void InitializePuzzle()
        {
            var playerData = PlayerDataManager.PlayerData;
            GameState.InitializePuzzle(playerData.ComboCount, playerData.Score);
            GameInitialData.Instance.SetInformation(playerData.DiceMax, playerData.DiceCount);
            GameUIPresenter.Instance.InitializePuzzle(TimeLimit);
            DiceCalculator.InitializePuzzle();
            PuzzleHistory.InitializePuzzle();
        }


        private static async UniTask PreparePuzzle(CancellationToken gameCt)
        {
            (bool isSolvable, List<string> solutionStrings) solutionInfo;
            while (true)
            {
                var rollTask = DiceCalculator.RollDiceAsync(gameCt);
                var animationTask = GameUIPresenter.Instance.EndRollAnimationAsync(gameCt);
                var result = await UniTask.WhenAll(rollTask, animationTask);
                solutionInfo = result.Item1;
                if (solutionInfo.isSolvable) break;
                
                await GameUIPresenter.Instance.ShowMessageAsync();
            }
            GameState.FormulaStrings = solutionInfo.solutionStrings;

            PuzzleHistory.SetHist(DiceCalculator.GetAllDices(),"");
        }

        private static async UniTask PuzzleFlowAsync()
        {
            var loadCts = new CancellationTokenSource();
            PlayerDataManager.LoadPlayerDataAsync(loadCts.Token);
            loadCts.Cancel();

            while (true)
            {
                var gameCts = new CancellationTokenSource();
                
                InitializePuzzle();
                
                var uiTask = GameUIPresenter.Instance.HandlePuzzlePlayAsync(gameCts.Token);
                await PreparePuzzle(gameCts.Token);
                
                var timerTask = CountTimerAsync(gameCts.Token);
                var retirementTask = GameUIPresenter.Instance.WaitForRetirementAsync(gameCts.Token);
                var playerTask = PlayerController.Instance.ProcessPlayerActionAsync(gameCts.Token);
                var gameCompetitionTask =
                    UniTask.WaitUntil(DiceCalculator.IsCorrectReached, cancellationToken: gameCts.Token);
                var result = await UniTask.WhenAny(retirementTask, playerTask, uiTask, gameCompetitionTask,timerTask);
                
                if (result == 3)
                {
                    await GameUIPresenter.Instance.MoveToCenterAsync(gameCts.Token);
                }
                gameCts.Cancel();

                if (result == 2)
                {
                    continue;
                }

                var menuCts = new CancellationTokenSource();
                switch (result)
                {
                    case 0:
                        GameState.GameOver();
                        await GameUIPresenter.Instance.EndGameAnimationAsync(false, menuCts.Token);
                        break;
                    case 4:
                        GameState.GameOver();
                        await GameUIPresenter.Instance.EndGameAnimationAsync(false, menuCts.Token);
                        break;
                    case 3:
                        GameState.GameClear();
                        await GameUIPresenter.Instance.EndGameAnimationAsync(true, menuCts.Token);
                        break;
                }

                PlayerDataManager.SavePlayerDataAsync(new PlayerData(GameState.Score, GameState.ComboCount,
                    GameInitialData.Instance.diceCount, GameInitialData.Instance.maxDiceValue), menuCts.Token);
                menuCts.Cancel();
            }
        }
    }
}