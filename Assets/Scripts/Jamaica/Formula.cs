namespace Jamaica
{
    public class Formula
    {
        public Formula(int one ,int anotherOne,OperatorMark operatorMark,int answer)
        {
            One = one;
            AnotherOne = anotherOne;
            OperatorMark = operatorMark;
            Answer = answer;
        }
        public readonly int One;
        public readonly int AnotherOne;
        public readonly OperatorMark OperatorMark;
        public readonly int Answer;
    
    }
}