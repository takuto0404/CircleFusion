using System.Collections.Generic;


public class Hist
{
    public Hist(List<Dice> dices,List<Formula> formula)
    {
        Dices = dices;
        Formula = formula;
    }
    public readonly List<Dice> Dices;
    public List<Formula> Formula;
}