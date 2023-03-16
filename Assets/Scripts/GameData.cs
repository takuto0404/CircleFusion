using UniRx;

/// <summary>
/// いろいろなところからアクセスしたいゲームのデータを入れるstaticクラス
/// </summary>
public static class GameData
{
    /// <summary>
    /// パズルを揃えている途中だったらtrue,メニュー画面などにいるときであればfalseを返す
    /// </summary>
    public static bool IsPuzzling;
    
    /// <summary>
    /// ゲーム中のタイマーの数値
    /// </summary>
    public static ReactiveProperty<float> Timer;
    
    /// <summary>
    /// ゲーム中のスコア
    /// </summary>
    public static int Score;
    
    /// <summary>
    /// 連続正解数
    /// </summary>
    public static int Combo;
}