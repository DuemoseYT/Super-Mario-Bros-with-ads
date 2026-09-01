using System.Collections;
using UnityEngine;
using UnityEngine.Video;
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

    [Header("Intro Video (optional)")]
    [SerializeField] private VideoPlayer videoPlayer;       // assign the VideoPlayer in your scene
    [SerializeField] private GameObject videoCanvasObject;  // the Canvas/panel holding the video display, disabled by default
    [SerializeField] private GameObject menuUI;             // your main menu buttons/panel, to hide while video plays

    [Header("Skip Button (optional)")]
    [SerializeField] private GameObject skipButtonObject; // shown only after skipButtonDelay seconds
    [SerializeField] private float skipButtonDelay = 5f;

    private bool isTransitioning;
    private bool skipRequested;

    private void Awake()
    {
        if (videoCanvasObject != null)
            videoCanvasObject.SetActive(false);
        if (skipButtonObject != null)
            skipButtonObject.SetActive(false);
    }

    /// <summary>
    /// Hook up to the skip button's OnClick.
    /// </summary>
    public void SkipVideo()
    {
        skipRequested = true;
    }

    /// <summary>
    /// Hook up to your "Play" button's OnClick.
    /// </summary>
    public void PlayGame()
    {
        if (isTransitioning) return;

        // Reset timescale in case it was paused from a previous session (e.g. death menu)
        Time.timeScale = 1f;
        StartCoroutine(PlayIntroThenLoad(sceneName));
    }

    /// <summary>
    /// Alternative: load by build index instead of name, if you prefer.
    /// Also plays the intro video first, same as PlayGame().
    /// </summary>
    public void PlayGameByIndex(int buildIndex)
    {
        if (isTransitioning) return;

        Time.timeScale = 1f;
        StartCoroutine(PlayIntroThenLoad(null, buildIndex));
    }

    private IEnumerator PlayIntroThenLoad(string targetSceneName, int targetBuildIndex = -1)
    {
        isTransitioning = true;

        if (menuUI != null)
            menuUI.SetActive(false);

        if (videoPlayer != null)
        {
            if (videoCanvasObject != null)
                videoCanvasObject.SetActive(true);

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();

            skipRequested = false;
            float elapsed = 0f;

            while (videoPlayer.isPlaying && !skipRequested)
            {
                elapsed += Time.deltaTime;

                if (skipButtonObject != null && !skipButtonObject.activeSelf && elapsed >= skipButtonDelay)
                    skipButtonObject.SetActive(true);

                yield return null;
            }

            videoPlayer.Stop();

            if (skipButtonObject != null)
                skipButtonObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(targetSceneName))
            SceneManager.LoadScene(targetSceneName);
        else
            SceneManager.LoadScene(targetBuildIndex);
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