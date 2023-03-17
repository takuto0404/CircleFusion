using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class GameStateManager
{
    private static CancellationToken _gameCts;

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
        
    }

    /// <summary>
    /// パズルソルブ中にタイマーのカウントを行う非同期メソッド
    /// </summary>
    private static async UniTask CountTimerAsync()
    {
        
    }

    /// <summary>
    /// ゲームオーバー時のアニメーション等の非同期メソッド
    /// </summary>
    private static async UniTask GameOveredAsync()
    {
        
    }

    /// <summary>
    /// ゲームクリア時のアニメーション等の非同期メソッド
    /// </summary>
    private static async UniTask GameClearedAsync()
    {
        
    }
}
