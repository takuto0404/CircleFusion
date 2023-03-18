using System;
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
        DiceModel.PuzzleInit();
        GameUIPresenter.Instance.PuzzleInit();
        JamaicaHistory.PuzzleInit();
        var gameCts = new CancellationTokenSource();
        CountTimerAsync(gameCts.Token).Forget();
        GameUIPresenter.Instance.PuzzleBehaviorAsync(gameCts.Token).Forget();

        await DiceModel.ShuffleDicesAsync(gameCts.Token);
        
        JamaicaHistory.SetInitHist(DiceModel.GetDices());

        var isFinished = false;
        var isCleared = true;
        while (!isFinished)
        {
            var retireTask = GameUIPresenter.Instance.RetireAsync(gameCts.Token);
            var gameTask = PlayerController.Instance.PlayerBehavior(gameCts.Token);
            var result = await UniTask.WhenAny(retireTask, gameTask);
            if (result == 0)
            {
                isCleared = false;
                break;
            }
            JamaicaHistory.SetHist(DiceModel.GetDices(),DiceModel.GetThisTimeFormula());
            GameUIPresenter.Instance.SetFormulaText(JamaicaHistory.LastHist().Formula);
            if (DiceModel.AnswerCheck())
            {
                await GameUIPresenter.Instance.MoveToEqualAsync(gameCts.Token);
                isFinished = true;
            }
        }
        gameCts.Cancel();

        var menuCts = new CancellationTokenSource();
        if (isCleared)
        {
            GameData.Win();
            await GameClearedAsync(menuCts.Token);
        }
        else
        {
            GameData.Lose();
            await GameOveredAsync(menuCts.Token);
        }
    }

    /// <summary>
    /// パズルソルブ中にタイマーのカウントを行う非同期メソッド
    /// </summary>
    private static async UniTask CountTimerAsync(CancellationToken gameCt)
    {
        GameData.Timer.Value = 0;

        var startTime = DateTime.Now;
        while (gameCt.IsCancellationRequested)
        {
            var diff = DateTime.Now - startTime;
            GameData.Timer.Value = diff.Seconds + diff.Milliseconds * 0.001f;
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