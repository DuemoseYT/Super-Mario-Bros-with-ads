using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a GameObject in your Main Menu scene (e.g. an empty "MenuManager" object).
/// Hook up your UI buttons' OnClick events to these public methods in the Inspector.
/// Make sure the scene you want to load is added to File > Build Settings > Scenes In Build.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Scene to load on Play")]
    [SerializeField] private string sceneName = "Level1"; // must match Build Settings exactly

    /// <summary>
    /// Hook up to your "Play" button's OnClick.
    /// </summary>
    public void PlayGame()
    {
        // Reset timescale in case it was paused from a previous session (e.g. death menu)
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Alternative: load by build index instead of name, if you prefer.
    /// </summary>
    public void PlayGameByIndex(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    /// <summary>
    /// Hook up to your "Quit" button's OnClick.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // stops Play mode in the editor
#else
        Application.Quit(); // closes the built application (does nothing in-editor)
#endif
    }
}