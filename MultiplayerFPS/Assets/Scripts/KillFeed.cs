using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeed : MonoBehaviour {

    [SerializeField]
    GameObject KillfeedItemPrefab;

	void Start () {
        GameManager.instance.onPlayerKilledCallback += OnKill;		
	}

    public void OnKill(string player, string source)
    {
        GameObject go = (GameObject)Instantiate(KillfeedItemPrefab, this.transform);
        go.GetComponent<KillFeedItem>().Setup(player, source);
        go.transform.SetAsFirstSibling();

        Destroy(go, 4f);
    }

}
