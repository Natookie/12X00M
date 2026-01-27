using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine("PlayMenuMusic");
    }

    public void OnPlayGame()
    {
        AudioManager.Instance.StopMusic();
        SceneManager.LoadScene("Main Scene");
    }

    public void OnOpenSettings()
    {
        // Open settings
    }

    public void OnQuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }

    private IEnumerator PlayMenuMusic()
    {
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlayMusic("menu");
    }
}
