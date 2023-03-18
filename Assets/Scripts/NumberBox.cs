using System.Threading;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

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

    [SerializeField] public bool isAnswerBox;

    /// <summary>
    /// numberTextの数字を更新
    /// </summary>
    /// <param name="number"></param>
    public void SetNumberText(int number)
    {
        numberText.text = number.ToString();
    }

    /// <summary>
    /// シャッフルが終わった後にクイっと大きくなるやつ
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask FinishedShuffleAnimationAsync(CancellationToken gameCt)
    {
        await gameObject.transform.DOScale(new Vector2(1.2f, 1.2f), 0.2f).SetEase(Ease.InQuart);
        await gameObject.transform.DOScale(new Vector2(1, 1), 0.2f).SetEase(Ease.OutQuart);
    }

    /// <summary>
    /// このNumberBoxを表示させる
    /// </summary>
    public void ShowBox()
    {
        GetComponent<RectTransform>().localPosition = initialPosition;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// このNumberBoxにアニメーションをつけながら、合体する先のボックスまで動いて消える
    /// </summary>
    /// <param name="destination">目的地</param>
    /// <param name="gameCt"></param>
    public async UniTask HideBoxAsync(Vector2 destination, CancellationToken gameCt)
    {
        await gameObject.transform.DOMove(destination, 0.5f).ToUniTask(cancellationToken:gameCt);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// このボックスが最後まで残ったときに、真ん中の答えのボックスに動いていくアニメーションを行う
    /// </summary>
    /// <param name="answerBoxPosition">目標値のサイコロの場所</param>
    /// <param name="gameCt"></param>
    public async UniTask MoveToEqualAsync(Vector2 answerBoxPosition,CancellationToken gameCt)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(answerBoxPosition, 0.8f));
        sequence.Join(transform.DOScale(new Vector2(2f, 2f), 0.8f));
        await sequence.Play().ToUniTask(cancellationToken:gameCt);
    }
}