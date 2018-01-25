using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class HostGame : MonoBehaviour {

    [SerializeField]
    private uint roomSize = 8;

    [SerializeField]
    private Dropdown dropdown;

    private string roomName;

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
    }

    void Update()
    {
        int sceneName = dropdown.value;
    }

    public void SetRoomName(string _name)
    {
        roomName = _name;
    }

    public void CreateRoom()
    {
        if (roomName != "" && roomName != null)
        {
            Debug.Log("Creating room " + roomName + " with " + roomSize + " players.");

            networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        }
    }

    public void ChangeMap()
    {
        Debug.Log("Stubbe");
        int sceneNumber = dropdown.value;
        string SceneName = ReturnSceneName(sceneNumber);
        networkManager.onlineScene = SceneName;
    }

    public string ReturnSceneName(int sceneNumber)
    {
        switch (sceneNumber)
        {
            case 0:
                Debug.Log("Case 1");
                return ("MainLevel01");
            case 1:
                Debug.Log("Case 2");
                return ("SecondLevel");
            case 2:
                Debug.Log("Case 3");
                return ("ThirdLevel");
            default:
                Debug.Log("Case default");
                return ("MainLevel01");
        }
    }
}
