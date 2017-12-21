using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PickupScript : NetworkBehaviour {

    private PlayerWeapon currentWeapon;
    //private WeaponManager weaponManager;
    private PlayerShoot playerShoot;

    private const string PLAYER_TAG = "Player";

    // Use this for initialization
    void Start () {
      //  weaponManager = GetComponent<WeaponManager>();

    }
	
	// Update is called once per frame
	void Update () {
    /*    currentWeapon = weaponManager.GetCurrentWeapon();
        Debug.Log(currentWeapon.damage);
        if (currentWeapon == null)
            Debug.Log("NULL REFERENTIATIO");
            */
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
