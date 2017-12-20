using UnityEngine;
using UnityEngine.UI;

public class KillFeedItem : MonoBehaviour {

    [SerializeField]
    Text text;

    public void Setup (string player, string source)
    {
        text.text = "<color=green>" + source + "</color>" + " killed " + "<Color=red>" + player + "</color>";
    }
}
