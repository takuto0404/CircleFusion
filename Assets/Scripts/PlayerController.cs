using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerController : MonoBehaviour
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