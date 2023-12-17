using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Jamaica
{
    public static class GameStateManager
    {
        [RuntimeInitializeOnLoadMethod]
        private static void GameInit()
        {
            PuzzleFlowAsync().Forget();
        }

        private static async UniTask CountTimerAsync(CancellationToken gameCt)
        {
            GameState.CurrentTime.Value = 0;

            var startTime = DateTime.Now;
            while (!gameCt.IsCancellationRequested)
            {
                var diff = DateTime.Now - startTime;
                GameState.CurrentTime.Value = (float)diff.TotalSeconds;
                await UniTask.DelayFrame(1, cancellationToken: gameCt);
            }
        }

        private static void InitializePuzzle()
        {
            var playerData = PlayerDataManager.PlayerData;
            GameState.InitializePuzzle(playerData.ComboCount, playerData.Score);
            GameInitialData.Instance.SetInformation(playerData.DiceMax, playerData.DiceCount);
            GameUIPresenter.Instance.InitializePuzzle();
            DiceCalculator.InitializePuzzle();
            JamaicaHistory.InitializePuzzle();
        }


        private static async UniTask PreparePuzzle(CancellationToken gameCt)
        {
            InitializePuzzle();
            (bool isSolvable, List<string> solutionStrings) solutionInfo;
            while (true)
            {
                var rollTask = DiceCalculator.RollDiceAsync(gameCt);
                var animationTask = GameUIPresenter.Instance.EndRollAnimationAsync(gameCt);
                await UniTask.WhenAll(rollTask, animationTask);
                solutionInfo = JamaicaSolver.SolveJamaica(DiceCalculator.GetAnswerNumber(),
                    DiceCalculator.ExtractDiceNumbers());
                if (solutionInfo.isSolvable)
                {
                    break;
                }

                await GameUIPresenter.Instance.ShowMessageAsync();
            }

            GameState.FormulaStrings = solutionInfo.solutionStrings;

            JamaicaHistory.SetHist(DiceCalculator.GetAllDices(),"");
        }

        private static async UniTask PuzzleFlowAsync()
        {
            var loadCts = new CancellationTokenSource();
            await PlayerDataManager.LoadPlayerDataAsync(loadCts.Token);
            loadCts.Cancel();

            while (true)
            {
                var gameCts = new CancellationTokenSource();
                await PreparePuzzle(gameCts.Token);

                CountTimerAsync(gameCts.Token).Forget();

                var retirementTask = GameUIPresenter.Instance.WaitForRetirementAsync(gameCts.Token);
                var playerTask = PlayerController.Instance.ProcessPlayerActionAsync(gameCts.Token);
                var uiTask = GameUIPresenter.Instance.HandlePuzzlePlayAsync(gameCts.Token);
                var gameCompetitionTask =
                    UniTask.WaitUntil(DiceCalculator.IsCorrectReached, cancellationToken: gameCts.Token);
                var result = await UniTask.WhenAny(retirementTask, playerTask, uiTask, gameCompetitionTask);


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
                    case 3:
                        GameState.GameClear();
                        await GameUIPresenter.Instance.EndGameAnimationAsync(true, menuCts.Token);
                        break;
                }

                await PlayerDataManager.SavePlayerDataAsync(new PlayerData(GameState.Score, GameState.ComboCount,
                    GameInitialData.Instance.diceCount, GameInitialData.Instance.maxDiceValue), menuCts.Token);
                menuCts.Cancel();
            }
        }
    }
}