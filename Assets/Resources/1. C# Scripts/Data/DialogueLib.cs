using UnityEngine;
using System.Collections.Generic;

public static class DialogueLib
{
    private static Dictionary<string, List<string>> dialogues = new Dictionary<string, List<string>>(){
        {
            "Idle", new List<string>()
            {
                "A little advice for you, if you leave more open space, I might tip you extra.",
                "Ouchie! My back hurts. Hurry up and place that furniture!",
                "Back in the day, my husband used to handle this sort of thing.",
                "Time is money! And right now, you're wasting my money.",
                "Is the gravity too strong today? Why is nothing moving?"
            }
        },
        {
            "Mad", new List<string>()
            {
                "I'm angry!",
                "This is unacceptable!",
                "How dare you!",
                "Why can't you do it right!",
                "Disappointing. Truly disappointing."
            }
        }
    };

    public static string GetRandomDialogue(string category){
        if(dialogues.ContainsKey(category) && dialogues[category].Count > 0) return dialogues[category][Random.Range(0, dialogues[category].Count)];
        return "";
    }
}