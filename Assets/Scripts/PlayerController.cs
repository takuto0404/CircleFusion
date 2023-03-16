using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// MVPパターンのVに接続する
    /// </summary>
    [SerializeField] private GameUIView gameUIView;

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
        return null;
    }
}