using System.Collections;
using Nova;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private UIBlock buttons;
    [SerializeField] private UIBlock settingsPanel;
    [SerializeField] private UIBlock creditsPanel;

    void Start()
    {
        // Initialize buttons and panels
        buttons.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(false);
        // Play menu music
        StartCoroutine("PlayMenuMusic");
    }

    public void OnPlayGame()
    {
        AudioManager.Instance.StopMusic();
        SceneManager.LoadScene("Main Scene");
    }

    public void OnOpenSettings()
    {
        buttons.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(true);
    }

    public void OnCloseSettings()
    {
        buttons.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
    }

    public void OnOpenCredits()
    {
        buttons.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(true);
    }

    public void OnCloseCredits()
    {
        buttons.gameObject.SetActive(true);
        creditsPanel.gameObject.SetActive(false);
    }

    public void OnQuitGame()
    {
        Debug.Log("Quit game");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private IEnumerator PlayMenuMusic()
    {
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlayMusic("menu");
    }
}
