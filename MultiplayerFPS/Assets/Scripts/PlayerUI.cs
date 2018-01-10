using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    RectTransform healthBarFill;

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    Text ammoText;

    [SerializeField]
    Text magText;

    [SerializeField]
    Text pickupText;

    [SerializeField]
    GameObject scoreBoard;

    private Player player;
    //private PlayerController controller;
    private WeaponManager weaponManager;

    public void SetPlayer(Player _player)
    {
        player = _player;
        //controller = player.GetComponent<PlayerController>();
        weaponManager = player.GetComponent<WeaponManager>();
    }
    
    void Start()
    {
        pickupText.text = "";
        PauseMenu.IsOn = false;
    }

    void Update()
    {
        SetHealthAmount(player.GetHealthPct());
        SetAmmoAmount(weaponManager.GetCurrentWeapon().bullets, weaponManager.GetCurrentWeapon().mags);

        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreBoard.SetActive(true);
        }   else if(Input.GetKeyUp(KeyCode.Tab))
        {
            scoreBoard.SetActive(false);
        }

        if (weaponManager.globalActivating)
        {
            PlayerWeapon playerWeapon = weaponManager.GetCurrentWeapon();
            SetPickupLine(playerWeapon.name);
        }
    }

   public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.IsOn = pauseMenu.activeSelf;
    }


    void SetHealthAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(1f, _amount, 1f);
    }

    void SetAmmoAmount(int _ammoAmount, int _magAmount)
    {
        ammoText.text = _ammoAmount.ToString();
        magText.text = _magAmount.ToString();
    }

    void SetPickupLine(string _string)
    {
        StartCoroutine(Pickup_Coroutine(_string));
    }

    private IEnumerator Pickup_Coroutine(string _string)
    {
        pickupText.text = "Picked up " + _string;

        yield return new WaitForSeconds(2);

        pickupText.text = "";
    }
}
