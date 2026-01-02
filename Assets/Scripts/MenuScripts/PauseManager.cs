using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuRoot; // Canvas/PauseMenu
    private bool paused;

    void Start()
    {
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        paused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!paused) Pause();
            else Resume();
        }
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool IsPaused => paused;
}

