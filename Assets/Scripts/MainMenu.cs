using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string levelSceneName = "Level1";
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    private bool loading;

    private void Awake()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        foreach (Text label in GetComponentsInChildren<Text>(true))
        {
            if (label.font == null)
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    public void PlayGame()
    {
        if (loading) return;
        if (!Application.CanStreamedLevelBeLoaded(levelSceneName))
        {
            Debug.LogError($"Add the scene '{levelSceneName}' to the Build Profiles scene list.", this);
            return;
        }
        loading = true;
        if (playButton != null) playButton.interactable = false;
        if (quitButton != null) quitButton.interactable = false;
        SceneManager.LoadSceneAsync(levelSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
