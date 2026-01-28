using UnityEngine;
using System.Collections.Generic;

public static class DialogueLib
{
    private static Dictionary<string, List<string>> dialogues = new Dictionary<string, List<string>>(){
        {
            "Idle", new List<string>()
            {
                "Back in my days, i were involved in new rezime",
                "Nice weather today.",
                "What's up sybau?"
            }
        },
        {
            "Mad", new List<string>()
            {
                "I'm angry!",
                "This is unacceptable!",
                "How dare you!"
            }
        }
    };

    public static string GetRandomDialogue(string category){
        if(dialogues.ContainsKey(category) && dialogues[category].Count > 0) return dialogues[category][Random.Range(0, dialogues[category].Count)];
        return "";
    }
}