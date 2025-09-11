using UnityEngine;

public static class GameSettings
{
    public static float aimSensitivy = 1000;
    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public enum Difficulty
    {
        EASY,
        NORMAL,
        HARD,
    }
    public static Difficulty difficulty = Difficulty.NORMAL;
    
}
