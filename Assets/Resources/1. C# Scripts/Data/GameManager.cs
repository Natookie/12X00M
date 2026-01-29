using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public bool isInitialized;
    public bool isEnded;
    [SerializeField] private InteractableManager im;
    [SerializeField] private GameOver gameOver;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    void Start(){
        StartCoroutine(StartGame());
        StartCoroutine("PlayMenuMusic");

        //if(inputManager != null) inputManager.SetActive(false);
    }

    IEnumerator PlayMenuMusic(){
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlayMusic("game");
    }

    IEnumerator StartGame(){
        yield return new WaitForSeconds(0.1f);
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "Heyy blurry face, look at this. LOOK AT THIS...\nWhy is my room so cramped?"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Npc,
            "Ma'am. Respectfully. This is a 9x9 room. You asked for a bed, a sofa, a reading corner,"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Npc,
            "feng shui, workspace, ch-"
        );
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "So?"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Npc,
            "MA'AM.. WHAT DO YOU MEAN \"SO\"?. It is physically not possible."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "Don't get smart with me faceless young man."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Npc,
            "First of all. I have a face. You are just senile and close to being blind."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Npc,
            "Second, I'm not being smart, I'm being tired. Besides, you keep changing your mind."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Npc,
            "You know what. I am DONE. Here, take your remaining money. Bye."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "Hmph. Whatever. I already bought a replacement anyway."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "Clanker. Surely you can make <color=#4995f3>Room For One More</color>.. right?"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Player,
            "Of course...~ Not. Are you senile? We need to start over."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "You do you. Here, take this man's previous budget, and buy me some furniture."
        );
        MoneyManager.Instance.AddMoney(1000);
        MoneyUI.Instance.UpdateMoneyDisplay(MoneyManager.Instance.Money);

        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "Here is my list! If you do the main tas-~.., eh, i am too lazy to explain it."
        );
        TaskManager.Instance.AssignRandomTasks();

        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "I will write down the instruction for you at <color=#3ec54b>Guide</color> instead!"
        );

        SetInitialized();
    }

    public void SetInitialized(){
        isInitialized = true;
        InformationPanelUI.Instance.OnButtonClicked(2);
        im.EnableAllInteractables();

        DialogueManager.Instance.ResetSkip();
    }

    public IEnumerator EndGame(){
        isEnded = true;
        gameOver.PlayVignette();

        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "Hmph..~ I knew it."
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "This sussy clanker can't do it's job properly!"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Grandma,
            "I'm taking you back to Tokopedia! ~~\n<color=#b22741>C'MERE BOYY!!</color>"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Player,
            "Please yiyi i need this"
        );

        gameOver.ShowResult();
    }
}