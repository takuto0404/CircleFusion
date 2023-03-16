using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MouseInputProvider : SingletonMonoBehaviour<MouseInputProvider>
{
    /// <summary>
    /// 
    /// </summary>
    public Vector2 mousePosition;

    /// <summary>
    /// マウスが押される/タップされるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldDownAsync(CancellationToken gameCt)
    {
        
    }
    
    /// <summary>
    /// マウスが離される/タップが終わるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldUpAsync(CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// マウスが押されているか/タップされているかどうかを返す
    /// </summary>
    /// <returns>タップされていたらtrue,そうでなければfalse</returns>
    private bool IsMouseGettingDown()
    {
        return true;
    }

    /// <summary>
    /// マウス/タップの位置を調べる
    /// </summary>
    /// <returns>マウス/タップの位置</returns>
    private Vector2 GetMousePosition()
    {
        return new Vector2();
    }
}