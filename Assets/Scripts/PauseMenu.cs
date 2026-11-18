using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private RawImage frozenFrame;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsPaused { get; private set; }
    private Texture2D screenshot;
    private Coroutine captureRoutine;
    private float previousTimeScale;
    private bool previousAudioPause;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;
    private bool leaving;

    private void Awake()
    {
        pausePanel.SetActive(false);
        frozenFrame.gameObject.SetActive(false);
        foreach (Text label in GetComponentsInChildren<Text>(true))
        {
            if (label.font == null)
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    private void Update()
    {
        if (leaving || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused || leaving) return;
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousCursorVisible = Cursor.visible;
        previousCursorLock = Cursor.lockState;
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        captureRoutine = StartCoroutine(CaptureFrame());
    }

    private IEnumerator CaptureFrame()
    {
        // Render this gameplay frame with the pause graphics still hidden.
        yield return new WaitForEndOfFrame();
        if (!IsPaused) yield break;
        try
        {
            screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            if (screenshot == null)
                throw new InvalidOperationException("The pause screenshot could not be captured.");
            frozenFrame.texture = screenshot;
            frozenFrame.gameObject.SetActive(true);
            pausePanel.SetActive(true);
        }
        catch (Exception error)
        {
            Debug.LogException(error, this);
            RestoreGame();
        }
        captureRoutine = null;
    }

    public void ResumeGame()
    {
        if (captureRoutine != null)
        {
            StopCoroutine(captureRoutine);
            captureRoutine = null;
        }
        RestoreGame();
    }

    private void RestoreGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (frozenFrame != null)
        {
            frozenFrame.gameObject.SetActive(false);
            frozenFrame.texture = null;
        }
        if (screenshot != null)
        {
            Destroy(screenshot);
            screenshot = null;
        }
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ReturnToMainMenu()
    {
        if (leaving) return;
        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError($"Add '{mainMenuSceneName}' to the Build Profiles scene list.", this);
            return;
        }
        leaving = true;
        ResumeGame();
        SceneManager.LoadSceneAsync(mainMenuSceneName);
    }

    private void OnDisable()
    {
        ResumeGame();
    }
}
