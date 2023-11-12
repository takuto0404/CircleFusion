using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
            GameUIPresenter.Instance.PuzzleInit();
            DiceModel.PuzzleInit();
            JamaicaHistory.PuzzleInit();
            var gameCts = new CancellationTokenSource();
            CountTimerAsync(gameCts.Token).Forget();
            
            var uiTask = GameUIPresenter.Instance.PuzzleBehaviorAsync(gameCts.Token);

            (bool canSolve, List<string> solutions) solveResult = (false,null);
            var i = 0;
            while (!solveResult.canSolve)
            {
                if(i != 0)await GameUIPresenter.Instance.ShowNotice();
                await DiceModel.ShuffleDicesAsync(gameCts.Token);
                solveResult = JamaicaSolver.SolveJamaica(DiceModel.GetAnswerNumber(), DiceModel.GetDiceNumbers());
                i++;
            }

            Debug.Log("Solution変更");
            GameData.Solutions = solveResult.solutions;

            JamaicaHistory.SetInitHist(DiceModel.GetDices());
            
            var retireTask = GameUIPresenter.Instance.RetireAsync(gameCts.Token);
            var gameTask = PlayerController.Instance.PlayerBehavior(gameCts.Token);
            var clearTask = UniTask.WaitUntil(DiceModel.AnswerCheck, cancellationToken: gameCts.Token);
            var result = await UniTask.WhenAny(retireTask, gameTask, uiTask, clearTask);
            if (result == 2) continue;
            var menuCts = new CancellationTokenSource();
            if (result == 0)
            {
                GameData.Lose();
                await GameOveredAsync(menuCts.Token);
            }

            if (result == 3)
            {
                await GameUIPresenter.Instance.MoveToEqualAsync(gameCts.Token);
                GameData.Win();
                await GameClearedAsync(menuCts.Token);
            }

            gameCts.Cancel();
        }
    }


    /// <summary>
    /// パズルソルブ中にタイマーのカウントを行う非同期メソッド
    /// </summary>
    private static async UniTask CountTimerAsync(CancellationToken gameCt)
    {
        GameData.Timer.Value = 0;

        var startTime = DateTime.Now;
        while (!gameCt.IsCancellationRequested)
        {
            var diff = DateTime.Now - startTime;
            GameData.Timer.Value = (float)diff.TotalSeconds;
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