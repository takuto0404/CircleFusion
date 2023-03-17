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
    public int AnotherOne;
    public readonly OperatorMark OperatorMark;
    public int Answer;
}