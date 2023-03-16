using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIView : MonoBehaviour
{
    /// <summary>
    /// 答え(計算目標)となる箱
    /// </summary>
    [SerializeField] private NumberBox answerBox;
    
    /// <summary>
    /// 式に利用できるサイコロの箱
    /// </summary>
    [SerializeField] private List<NumberBox> numberBoxes;
    
    /// <summary>
    /// リタイアするときに押すボタン
    /// </summary>
    [SerializeField] private Button retireButton;
    
    /// <summary>
    /// 連続で何回正解したかを示すUIのバックの図形
    /// </summary>
    [SerializeField] private GameObject comboImage;
    
    /// <summary>
    /// ゲーム終了時のリスタートボタン
    /// </summary>
    [SerializeField] private Button restartButton;
    
    /// <summary>
    /// 右上の方に出てくる式の履歴のテキスト
    /// </summary>
    [SerializeField] private TMP_Text formulaText;
    
    /// <summary>
    /// 左上に出てくるスコア表示用テキスト
    /// </summary>
    [SerializeField] private TMP_Text scoreText;
    
    /// <summary>
    /// 左上に出てくるタイム表示用テキスト
    /// </summary>
    [SerializeField] private TMP_Text timeText;
    
    /// <summary>
    /// ゲームクリア時に上から出てくるパネル
    /// </summary>
    [SerializeField] private GameObject gameClearPanel;
    
    /// <summary>
    /// ゲームオーバー時に上から出てくるパネル
    /// </summary>
    [SerializeField] private GameObject gameOverPanel;
    
    /// <summary>
    /// ゲーム終了時の画面に出てくるスコア結果のテキスト
    /// </summary>
    [SerializeField] private TMP_Text scoreResultText;
    
    /// <summary>
    /// ゲーム終了時の画面に出てくるタイム結果のテキスト
    /// </summary>
    [SerializeField] private TMP_Text timeResultText;
    
    /// <summary>
    /// ゲーム終了時の画面に出てくるコンボ結果のテキスト
    /// </summary>
    [SerializeField] private TMP_Text comboResultText;
    
    /// <summary>
    /// ゲーム起動時の初期化内容
    /// </summary>
    public void GameInit()
    {
        
    }
    
    /// <summary>
    /// パズルを解く前に情報をリセット・初期化する
    /// </summary>
    public void PuzzleInit()
    {
        
    }

    /// <summary>
    /// 右上の式のテキストを更新する
    /// </summary>
    /// <param name="formulas">式の履歴リスト</param>
    public void SetFormulaText(List<Formula> formulas)
    {
        
    }

    /// <summary>
    /// タイマーの表示用テキストを更新する
    /// </summary>
    public void SetTimerText()
    {
        
    }

    /// <summary>
    /// 演算記号を順々にアニメーションをつけて表示する
    /// </summary>
    /// <param name="gameCt"></param>
    private async UniTask ShowOperatorsAsync(CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// 演算記号を順々にアニメーションをつけて隠す
    /// </summary>
    /// <param name="gameCt"></param>
    private async UniTask HideOperatorsAsync(CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// どれかの演算記号のボタンが押されるまで待つ
    /// </summary>
    /// <param name="gameCt"></param>
    /// <returns>押された結果</returns>
    private async UniTask<OperatorMark> SelectOperatorsAsync(CancellationToken gameCt)
    {
        return OperatorMark.Plus;
    }

    /// <summary>
    /// ゲーム終了時のアニメション
    /// </summary>
    /// <param name="wasGameCleared">ゲームクリアだったらtrue,ゲームオーバーならfalse</param>
    /// <param name="gameCt"></param>
    private async UniTask GameFinishedAnimationAsync(bool wasGameCleared, CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// ゲームクリア時の処理
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask GameClear(CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// ゲームオーバー時の処理
    /// </summary>
    /// <param name="gameCt"></param>
    public async UniTask GameOver(CancellationToken gameCt)
    {
        
    }
}