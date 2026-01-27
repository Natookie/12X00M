using System.Collections;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine("PlayMenuMusic");
    }

    private IEnumerator PlayMenuMusic()
    {
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlayMusic("menu");
    }
}
