using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public static class JamaicaHistory
{
    /// <summary>
    /// そのパズル上でどのようにボックスが動かされていったかの履歴
    /// </summary>
    private static Stack<Hist> _histories;

    public static void PuzzleInit()
    {
        _histories = new Stack<Hist>();
    }

    public static Hist LastHist()
    {
        return _histories.Peek();
    }

    public static void SetInitHist(List<Dice> dices)
    {
        _histories.Push(new Hist(dices,""));
    }

    public static void SetHist(List<Dice> dices, string formulaText)
    {
        _histories.Push(new Hist(dices,formulaText));
    }
    
    public static Hist BackHist()
    {
        if (_histories.Count == 1) return null;
        _histories.Pop();
        return _histories.Peek();
    }
}