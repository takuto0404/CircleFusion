using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CircleFusion.InGame
{
    public static class PuzzleSolver
    {
        public static (bool isSolvable, List<string> solutionStrings) Solution;
        private static readonly Stack<(int number, string text, OperatorSymbol beforeOperatorSymbol)[]> NumberHist =
            new();

        private static readonly Dictionary<OperatorSymbol, string> OperatorDic = new()
        {
            { OperatorSymbol.Plus, "+" },
            { OperatorSymbol.Minus, "-" },
            { OperatorSymbol.Times, "*" },
            { OperatorSymbol.Devide, "/" }
        };

        private static (int number, string text, OperatorSymbol beforeOperatorSymbol)[] _numbers;
        private static List<string> _solutions;
        private static int _targetAnswer;


        public static async UniTask SolvePuzzle(int targetAnswer,
            int[] originalNumbers)
        {
            Debug.Log(targetAnswer);
            _solutions = new List<string>();
            _numbers = originalNumbers.Select(number => (number, number.ToString(), OperatorSymbol.None)).ToArray();

            NumberHist.Push(_numbers);
            _targetAnswer = targetAnswer;
            await Solve();

            Solution =  (_solutions.Count > 0, _solutions);
        }

        private static async UniTask Solve()
        {
            var last = NumberHist.Peek();
            var usedNumber1 = new List<int>();
            for (var i = 0; i < _numbers.Length; i++)
            {
                if(usedNumber1.Contains(last[i].number))continue;
                usedNumber1.Add(last[i].number);
                if (last[i].number == -1) continue;

                var usedNumber2 = new List<int>();
                for (var l = 0; l < _numbers.Length; l++)
                {
                    if(usedNumber2.Contains(last[l].number))continue;
                    usedNumber2.Add(last[l].number);
                    if (i == l) continue;
                    if (last[l].number == -1 || last[i].number == -1) continue;
                    if (last[i].number < last[l].number) continue;
                    for (var operatorNumber = 0; operatorNumber < 4; operatorNumber++)
                    {
                        var newArray = CopyArray(last);

                        var beforeFirstNumber = newArray[i];
                        var beforeSecondNumber = newArray[l];
                        var firstNumber = newArray[i];
                        var secondNumber = newArray[l];
                        var canCalculate = new[]
                        {
                            true,
                            firstNumber.number <= secondNumber.number,
                            true,
                            ((float)firstNumber.number / (float)secondNumber.number) % 1 == 0 &&
                            secondNumber.number != 0 &&
                            secondNumber.number != 1
                        };

                        if (!canCalculate[operatorNumber]) continue;
                        if ((beforeFirstNumber.beforeOperatorSymbol is OperatorSymbol.Plus or OperatorSymbol.Times &&
                             beforeFirstNumber.beforeOperatorSymbol == (OperatorSymbol)operatorNumber) ||
                            (beforeSecondNumber.beforeOperatorSymbol is OperatorSymbol.Plus or OperatorSymbol.Times &&
                             beforeSecondNumber.beforeOperatorSymbol == (OperatorSymbol)operatorNumber))
                        {
                            if (beforeFirstNumber.number < secondNumber.number)
                            {
                                continue;
                            }
                        }

                        if (beforeFirstNumber.beforeOperatorSymbol == OperatorSymbol.Devide &&
                            beforeSecondNumber.beforeOperatorSymbol == OperatorSymbol.Times)
                        {
                            continue;
                        }

                        firstNumber.number = CalculateResult(firstNumber.number, secondNumber.number,
                            (OperatorSymbol)operatorNumber);


                        firstNumber.beforeOperatorSymbol = (OperatorSymbol)operatorNumber;

                        firstNumber.text = CreateStringText(beforeFirstNumber, beforeSecondNumber,
                            (OperatorSymbol)operatorNumber);

                        secondNumber.text = "";
                        secondNumber.number = -1;
                        secondNumber.beforeOperatorSymbol = OperatorSymbol.None;

                        newArray[i] = firstNumber;
                        newArray[l] = secondNumber;
                        NumberHist.Push(newArray);

                        var activeList = newArray.Where(x => x.number != -1).ToArray();
                        if (activeList.Length == 1)
                        {
                            if (activeList.First().number == _targetAnswer)
                            {
                                var text = $"{activeList.First().text} = {_targetAnswer}";
                                if (_solutions.Contains(text)) continue;
                                _solutions.Add(text);
                            }
                        }

                        Solve();

                        NumberHist.Pop();
                    }
                }
            }
        }

        private static int CalculateResult(int firstNumber, int secondNumber, OperatorSymbol operatorSymbol)
        {
            switch (operatorSymbol)
            {
                case OperatorSymbol.Plus:
                    return firstNumber + secondNumber;
                case OperatorSymbol.Minus:
                    return firstNumber - secondNumber;
                case OperatorSymbol.Times:
                    return firstNumber * secondNumber;
                case OperatorSymbol.Devide:
                    return firstNumber / secondNumber;
                default:
                    return -1;
            }
        }

        private static string CreateStringText(
            (int number, string text, OperatorSymbol beforeOperatorSymbol) beforeFirstNumber,
            (int number, string text, OperatorSymbol beforeOperatorSymbol) beforeSecondNumber,
            OperatorSymbol operatorSymbol)
        {
            var formulaText = "";
            if (operatorSymbol is OperatorSymbol.Times or OperatorSymbol.Devide)
            {
                if (beforeFirstNumber.beforeOperatorSymbol is OperatorSymbol.Plus or OperatorSymbol.Minus)
                {
                    formulaText += $"({beforeFirstNumber.text}) ";
                }
                else
                {
                    formulaText += $"{beforeFirstNumber.text} ";
                }

                formulaText += OperatorDic[operatorSymbol];

                if (beforeSecondNumber.beforeOperatorSymbol is OperatorSymbol.Plus or OperatorSymbol.Minus)
                {
                    formulaText += $" ({beforeSecondNumber.text})";
                }
                else
                {
                    formulaText += $" {beforeSecondNumber.text}";
                }
            }
            else
            {
                formulaText = $"{beforeFirstNumber.text} {OperatorDic[operatorSymbol]} {beforeSecondNumber.text}";
            }

            return formulaText;
        }

        private static (int number, string text, OperatorSymbol beforeOperatorSymbol)[] CopyArray(
            (int number, string text, OperatorSymbol beforeOpeartorMark)[] array)
        {
            var newArray = new (int number, string text, OperatorSymbol beforeOperatorSymbol)[array.Length];
            Array.Copy(array,newArray,array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            return newArray;
        }
    }
}