using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx.Triggers;
using UniRx;

public class PlayerController : SingletonMonoBehaviour<PlayerController>
{
    /// <summary>
    /// MVPパターンのVに接続する
    /// </summary>
    [SerializeField] private GameUIView gameUIView;

    [SerializeField] private Camera mainCamera;

    /// <summary>
    /// パズルごとのプレイヤーの行動・処理を行う非同期メソッド
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask PlayerBehavior(CancellationToken gameCt)
    {
        NumberBox hoveredOne = null;
        NumberBox hoveredAnotherOne = null;

        while (hoveredAnotherOne == null)
        {
            hoveredOne = null;
            await MouseInputProvider.Instance.OnHoldDownAsync(gameCt);
            while (hoveredOne == null)
            {
                hoveredOne = GetHoveredNumberBox();
            }

            var one = hoveredOne;
            using (this.UpdateAsObservable().Subscribe(_ =>
                       gameUIView.DrawLine(one.transform.position, MouseInputProvider.Instance.mousePosition)))
            {
                await MouseInputProvider.Instance.OnHoldUpAsync(gameCt);
                hoveredAnotherOne = GetHoveredNumberBox();
            }

            gameUIView.ClearLine();
        }

        var canCalculate =
            GameUIPresenter.Instance.CanCalculate(hoveredOne, hoveredAnotherOne);
        var result = await gameUIView.SelectOperatorsAsync(canCalculate, gameCt);
        GameUIPresenter.Instance.Calculation(hoveredOne,hoveredAnotherOne,result);
    }

    /// <summary>
    /// 今マウス上にあるサイコロを返す
    /// </summary>
    /// <returns></returns>
    private NumberBox GetHoveredNumberBox()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var distance = 10f;
        var hit = Physics2D.Raycast(ray.origin, ray.direction, distance);
        return hit.collider.GetComponent<NumberBox>();
    }
}