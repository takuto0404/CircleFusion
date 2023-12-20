namespace CircleFusion.InGame
{
    public class Formula
    {
        public Formula(int firstDice ,int secondDice,OperatorSymbol operatorSymbol,int result)
        {
            FirstDice = firstDice;
            SecondDice = secondDice;
            Result = result;
            OperatorSymbol = operatorSymbol;
        }
        public readonly int FirstDice;
        public readonly int SecondDice;
        public readonly int Result;
        public readonly OperatorSymbol OperatorSymbol;
    
    }
}