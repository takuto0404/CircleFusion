using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;


public class MouseInputProvider : SingletonMonoBehaviour<MouseInputProvider>
{
    [SerializeField] private InputAction pressAction;
    [SerializeField] private InputAction releaseAction;
    private bool _isMouseGettingDown = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        Instance.Init();
    }
    
    private void Init()
    {
        pressAction.Enable();
        releaseAction.Enable();

        pressAction.performed += OnPress;
        releaseAction.performed += OnRelease;
    }
    

    public void OnPress(InputAction.CallbackContext context)
    {
        Debug.Log("VAR");
        if(context.performed)_isMouseGettingDown = true;
    }

    public void OnRelease(InputAction.CallbackContext context)
    {
        if(context.performed)_isMouseGettingDown = false;
    }

    public Vector2 mousePosition;

    public void GetMousePosition(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// マウスが押される/タップされるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldDownAsync(CancellationToken gameCt)
    {
        await UniTask.WaitUntil(() => _isMouseGettingDown, cancellationToken: gameCt);
    }

    /// <summary>
    /// マウスが離される/タップが終わるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask OnHoldUpAsync(CancellationToken gameCt)
    {
        await UniTask.WaitWhile(() => _isMouseGettingDown, cancellationToken: gameCt);
    }
}