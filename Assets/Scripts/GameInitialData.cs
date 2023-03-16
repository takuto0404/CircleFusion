using UnityEngine;

public class GameInitialData : SingletonMonoBehaviour<GameInitialData>
{
    [SerializeField] private int shuffleLength;
    [SerializeField] private int diceMaxValue;
}