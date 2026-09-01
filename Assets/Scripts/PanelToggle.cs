using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic show/hide controller for a UI panel (e.g. the slot machine).
/// Wire an "Open" button elsewhere in your UI to OpenPanel(), and put a
/// "Close" (X) button inside the panel itself wired to ClosePanel().
/// </summary>
public class PanelToggle : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel; // the panel GameObject to show/hide

    [Header("Optional Buttons (auto-wired if assigned)")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhileOpen = false; // set true if this pauses gameplay like a menu
    [SerializeField] private bool startClosed = true;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(!startClosed);

        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel()
    {
        if (panel != null)
            panel.SetActive(true);

        if (pauseGameWhileOpen)
            Time.timeScale = 0f;
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);

        if (pauseGameWhileOpen)
            Time.timeScale = 1f;
    }

    public void TogglePanel()
    {
        if (panel == null) return;

        if (panel.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }
}