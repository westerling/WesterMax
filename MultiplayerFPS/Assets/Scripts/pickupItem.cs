using UnityEngine;
using UnityEngine.UI;

public class pickupItem : MonoBehaviour {

    [SerializeField]
    Text text;

    public void Setup(string player, string pickup)
    {
        text.text = "<color=green>" + player + "</color>" + " picked up " + "<Color=red>" + pickup + "</color>";
    }
}
