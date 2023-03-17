using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public static class DiceModel
{
    /// <summary>
    /// 計算目標のサイコロ
    /// </summary>
    private static Dice _answerDice;
    
    /// <summary>
    /// 式に使用できるサイコロたち
    /// </summary>
    private static List<Dice> _dices;
    
    /// <summary>
    /// 活動状態にあるサイコロ
    /// </summary>
    public static List<Dice> ActiveDices => _dices.Where(dice => dice.IsActive).ToList();

    /// <summary>
    /// サイコロのシャッフルを行う非同期メソッド
    /// </summary>
    /// <param name="gameCt"></param>
    public static async UniTask ShuffleDicesAsync(CancellationToken gameCt)
    {
        var shuffleTasks = Enumerable.Range(0,5).Select(i => _dices[i].ShuffleAsync(i * GameInitialData.Instance.shuffleLength,gameCt)).ToList();
        shuffleTasks.Add(_answerDice.ShuffleAsync(_dices.Count * GameInitialData.Instance.shuffleLength,gameCt));
        await UniTask.WhenAll(shuffleTasks);
    }

    /// <summary>
    /// もう残り活動状態にあるサイコロが1でそれが目標値と等しければtrue,そうでなければfalse
    /// </summary>
    /// <returns></returns>
    public static bool AnswerCheck()
    {
        var activeBoxes = _dices.Where(item => item.IsActive).ToArray();
        if (activeBoxes.Length != 1) return false;
        var lastBoxNumber = activeBoxes.First().Number.Value;
        return lastBoxNumber == _answerDice.Number.Value;
    }

    /// <summary>
    /// 一つ手順を戻る
    /// </summary>
    /// <param name="hist">復元用の履歴</param>
    public static void ReturnStep(Hist hist)
    {
        for (var i = 0; i < _dices.Count; i++)
        {
            _dices[i] = new Dice(hist.Dices[i]);
        }
    }

    /// <summary>
    /// 二つの選択されたサイコロと演算記号をもとに計算して、片方を非アクティブにする
    /// </summary>
    /// <param name="one">片方(足される、引かれる、掛けられる、割られる方)</param>
    /// <param name="anotherOne">もう片方(足す、引く、掛ける、割る方)</param>
    /// <param name="operatorMark">選択された演算記号</param>
    public static void MergeDice(Dice one, Dice anotherOne,OperatorMark operatorMark)
    {
        var result = Calculation(one, anotherOne, operatorMark);
        one.Number.Value = result;
        anotherOne.MergedDice = one;
        anotherOne.IsActive.Value = false;
    }

    /// <summary>
    /// シンプルに受け取った二つのサイコロと演算記号をもとに計算結果を表示する
    /// </summary>
    /// <param name="one">片方(足される、引かれる、掛けられる、割られる方)</param>
    /// <param name="anotherOne">もう片方(足す、引く、掛ける、割る方)</param>
    /// <param name="operatorMark">選択された演算記号</param>
    /// <returns></returns>
    private static int Calculation(Dice one,Dice anotherOne,OperatorMark operatorMark)
    {
        var value1 = one.Number.Value;
        var value2 = anotherOne.Number.Value;
        switch (operatorMark)
        {
            case OperatorMark.Plus:
                return value1 + value2;
            case OperatorMark.Minus:
                return value1 - value2;
            case OperatorMark.Times:
                return value1 * value2;
            case OperatorMark.Devided:
                return value1 / value2;
        }

        return -1;
    }
}