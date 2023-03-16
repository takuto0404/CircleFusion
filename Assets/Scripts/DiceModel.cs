using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public static class DiceModel
{
    /// <summary>
    /// 計算目標のサイコロ
    /// </summary>
    public static Dice AnswerDice;
    
    /// <summary>
    /// 式に使用できるサイコロたち
    /// </summary>
    public static List<Dice> Dices;
    
    /// <summary>
    /// 活動状態にあるサイコロ
    /// </summary>
    public static List<Dice> ActiveDices => Dices.Where(dice => dice.IsActive).ToList();

    /// <summary>
    /// サイコロのシャッフルを行う非同期メソッド
    /// </summary>
    /// <param name="gameCt"></param>
    public static async UniTask ShuffleDicesAsync(CancellationToken gameCt)
    {
        
    }

    /// <summary>
    /// もう残り活動状態にあるサイコロが1でそれが目標値と等しければtrue,そうでなければfalse
    /// </summary>
    /// <returns></returns>
    public static bool AnswerCheck()
    {
        return true;
    }

    /// <summary>
    /// 一つ手順を戻る
    /// </summary>
    /// <param name="hist">復元用の履歴</param>
    public static void ReturnStep(Hist hist)
    {
        
    }

    /// <summary>
    /// 二つの選択されたサイコロと演算記号をもとに計算して、片方を非アクティブにする
    /// </summary>
    /// <param name="one">片方(足される、引かれる、掛けられる、割られる方)</param>
    /// <param name="anotherOne">もう片方(足す、引く、掛ける、割る方)</param>
    /// <param name="operatorMark">選択された演算記号</param>
    public static void MergeDiceAsync(Dice one, Dice anotherOne,OperatorMark operatorMark)
    {
        
    }

    /// <summary>
    /// シンプルに受け取った二つのサイコロと演算記号をもとに計算結果を表示する
    /// </summary>
    /// <param name="one">片方(足される、引かれる、掛けられる、割られる方)</param>
    /// <param name="anotherOne">もう片方(足す、引く、掛ける、割る方)</param>
    /// <param name="operatorMark">選択された演算記号</param>
    /// <returns></returns>
    public static int Calculation(Dice one,Dice anotherOne,OperatorMark operatorMark)
    {
        return 0;
    }
}