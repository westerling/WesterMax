using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour {

   // private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;
    private const string PLAYER_TAG = "Player";

    // Use this for initialization
    void Start () {
        weaponManager = GetComponent<WeaponManager>();
        //currentWeapon = weaponManager.GetCurrentWeapon();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider _other)
    {
        if (_other.tag == PLAYER_TAG)
        {
            StartCoroutine(Ammo_Coroutine(_other.gameObject));
        }
    }

    private IEnumerator Ammo_Coroutine(GameObject _player)
    {
      //  currentWeapon.mags++;
        DisableOrEnableComponents(false);
        yield return new WaitForSeconds(30);
        DisableOrEnableComponents(true);
    }

    void DisableOrEnableComponents(bool _bool)
    {
        this.gameObject.GetComponent<Renderer>().enabled = _bool;
        this.gameObject.GetComponent<BoxCollider>().enabled = _bool;
    }

}
