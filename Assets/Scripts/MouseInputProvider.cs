using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class MouseInputProvider : SingletonMonoBehaviour<MouseInputProvider>
{
    /// <summary>
    /// 
    /// </summary>
    public Vector2 MousePosition => GetMousePosition();

    /// <summary>
    /// マウスが押される/タップされるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldDownAsync(CancellationToken gameCt)
    {
        await UniTask.WaitUntil(IsMouseGettingDown,cancellationToken:gameCt);
    }
    
    /// <summary>
    /// マウスが離される/タップが終わるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldUpAsync(CancellationToken gameCt)
    {
        await UniTask.WaitWhile(IsMouseGettingDown, cancellationToken: gameCt);
    }

    /// <summary>
    /// マウスが押されているか/タップされているかどうかを返す
    /// </summary>
    /// <returns>タップされていたらtrue,そうでなければfalse</returns>
    private bool IsMouseGettingDown()
    {
        if (Application.isEditor)
        {
            return Mouse.current.leftButton.isPressed;
        }
        else
        {
            TouchControl touchControl = Touchscreen.current.touches[0];
            return touchControl.IsPressed();
        }
    }

    /// <summary>
    /// マウス/タップの位置を調べる
    /// </summary>
    /// <returns>マウス/タップの位置</returns>
    private Vector2 GetMousePosition()
    {
        if (Application.isEditor)
        {
            return Mouse.current.position.ReadValue();
        }
        else
        {
            TouchControl touchControl = Touchscreen.current.touches[0];
            return touchControl.position.ReadValue();
        }
    }
}