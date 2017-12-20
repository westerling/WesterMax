using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreboardItem : MonoBehaviour {

    [SerializeField]
    Text usernameText;

    [SerializeField]
    Text KillsText;

    [SerializeField]
    Text DeathsText;

    public void Setup(string username, int kills, int deaths)
    {
        usernameText.text = username;
        KillsText.text = "" + kills;
        DeathsText.text = "" + deaths;
    }
}
