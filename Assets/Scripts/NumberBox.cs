using System.Threading;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class NumberBox : MonoBehaviour
{
    /// <summary>
    /// パズル開始時に初期化される座標
    /// </summary>
    [SerializeField] private Vector2 initialPosition;
    
    /// <summary>
    /// 数字を表示するテキスト
    /// </summary>
    [SerializeField] private TMP_Text numberText;

    /// <summary>
    /// numberTextの数字を更新
    /// </summary>
    /// <param name="number"></param>
    public void SetNumberText(int number)
    {
        
    }

    /// <summary>
    /// シャッフルが終わった後にクイっと大きくなるやつ
    /// </summary>
    /// <param name="gameCt"></param>
    private async UniTask FinishedShuffleAnimationAsync(CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// このNumberBoxを表示させる
    /// </summary>
    public void ShowBox()
    {
        
    }

    /// <summary>
    /// このNumberBoxにアニメーションをつけながら、合体する先のボックスまで動いて消える
    /// </summary>
    /// <param name="destination">目的地</param>
    /// <param name="gameCt"></param>
    public async UniTask HideBoxAsync(Vector2 destination, CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// このボックスが最後まで残ったときに、真ん中の答えのボックスに動いていくアニメーションを行う
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask MoveToEqualAsync(CancellationToken gameCt)
    {
        
    }
}