using UnityEngine.SceneManagement;

public enum SceneType
{
    Init,
    Game
}
public static class SceneHelper
{
    public static void LoadScene(SceneType type)
    {
        SceneManager.LoadScene((int)type);
    }
}