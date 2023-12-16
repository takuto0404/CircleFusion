using System.Collections.Generic;
using System.Linq;

namespace Jamaica
{
    public class DiceInfoClass
    {
        public (bool isActive, int diceNumber) DiceInfo;

        public DiceInfoClass(Dice dice)
        {
            DiceInfo.isActive = dice.IsActive;
            DiceInfo.diceNumber = dice.DiceNumber.Value;
        }
    }

    public class FormulaInfoClass
    {
        public string FormulaText;

        public FormulaInfoClass(string formulaText)
        {
            FormulaText = formulaText;
        }
    }
    public class Hist
    {
        public Hist(List<Dice> dices,string formulaText)
        {
            Dices = dices.Select(dice => new DiceInfoClass(dice)).ToList();
            FormulaText = formulaText;
        }
        public readonly List<DiceInfoClass> Dices;
        public readonly string FormulaText;
    }
}