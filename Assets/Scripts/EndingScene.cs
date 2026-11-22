using System.Collections;
using UnityEngine;

// Ending: she wakes up in her room at sunrise (Assets/Art/frame_4.jpg). Once EyeTransition has
// flickered her eyes open, she thinks back on the dream, then "The End" stays up.
public class EndingScene : MonoBehaviour
{
    public float charsPerSecond = 32f;
    public float linePause = 2.4f;
    [TextArea]
    public string[] lines =
    {
        "...Morning already?",
        "That dream. The garden, the party, the house.",
        "I got to hold on to all of it, one more time.",
        "The room is still empty. But it doesn't feel scary anymore.",
        "Those memories come with me, wherever I go.",
    };

    IEnumerator Start()
    {
        DialogueBox box = DialogueBox.Create(transform, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.22f), 44f, 999);
        yield return null; // EyeTransition sets Playing before this scene finishes loading
        while (EyeTransition.Playing) yield return null;
        yield return new WaitForSeconds(0.6f);
        yield return box.Say(lines, charsPerSecond, linePause, true);
        yield return new WaitForSeconds(0.5f);
        box.Show("The End");
    }
}
