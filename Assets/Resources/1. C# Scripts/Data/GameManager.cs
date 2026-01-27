using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine("PlayMenuMusic");
    }

    private IEnumerator PlayMenuMusic()
    {
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlayMusic("game");
    }

    public void EndGame(){
        
    }
}