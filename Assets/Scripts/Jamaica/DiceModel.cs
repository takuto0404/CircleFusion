using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Jamaica
{
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

        private static Formula _thisTimeFormula;

        /// <summary>
        /// サイコロのシャッフルを行う非同期メソッド
        /// </summary>
        /// <param name="gameCt"></param>
        public static async UniTask ShuffleDicesAsync(CancellationToken gameCt)
        {
            var shuffleTasks = Enumerable.Range(0,GameInitialData.Instance.numberOfDice).Select(i => _dices[i].RollDiceAsync((i + 1) * GameInitialData.Instance.shuffleLength,gameCt)).ToList();
            shuffleTasks.Add(_answerDice.RollDiceAsync((1 + GameInitialData.Instance.numberOfDice) * GameInitialData.Instance.shuffleLength,gameCt));
            await UniTask.WhenAll(shuffleTasks);
        }
        //TODO:Refactoring
        //TODO:シャッフル中の設定変更をすると何度も振り直せてしまう
        //TODO:先にシャッフル

        public static int[] GetDiceNumbers()
        {
            return _dices.Where(dice => !dice.IsAnswerDice).Select(dice => dice.DiceNumber.Value).ToArray();
        }

        public static int GetAnswerNumber()
        {
            return _answerDice.DiceNumber.Value;
        }

        public static Dice GetLastDice()
        {
            return _dices.First(dice => dice.IsActive);
        }

        /// <summary>
        /// もう残り活動状態にあるサイコロが1でそれが目標値と等しければtrue,そうでなければfalse
        /// </summary>
        /// <returns></returns>
        public static bool AnswerCheck()
        {
            var activeBoxes = _dices.Where(item => item.IsActive).ToArray();
            if (activeBoxes.Length != 1) return false;
            var lastBoxNumber = activeBoxes.First().DiceNumber.Value;
            return lastBoxNumber == _answerDice.DiceNumber.Value;
        }

        /// <summary>
        /// 一つ手順を戻る
        /// </summary>
        /// <param name="hist">復元用の履歴</param>
        public static void BackStep(Hist hist)
        {
            for (var i = 0; i < _dices.Count; i++)
            {
                _dices[i].DiceNumber.Value = hist.Dices[i].DiceInfo.diceNumber;
                _dices[i].IsActive = hist.Dices[i].DiceInfo.isActive;
            }
        }

        public static List<Dice> GetDices()
        {
            return _dices;
        }

        public static void PuzzleInit()
        {
            _thisTimeFormula = null;
        }

        public static void SetDice(List<Dice> dices,Dice answerDice)
        {
            _dices = new List<Dice>();
            _answerDice = answerDice;
            _answerDice.IsAnswerDice = true;
            dices.ForEach(item => _dices.Add(item));
        }

        public static Formula GetThisTimeFormula()
        {
            return _thisTimeFormula;
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
            _thisTimeFormula = new Formula(one.DiceNumber.Value, anotherOne.DiceNumber.Value, operatorMark, result);
            one.DiceNumber.Value = result;
            anotherOne.IsActive = false;
            anotherOne.MergedDice.Value = one;
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
            var value1 = one.DiceNumber.Value;
            var value2 = anotherOne.DiceNumber.Value;
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
}