using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PickupScript : NetworkBehaviour {

    private PlayerWeapon currentWeapon;
    //private WeaponManager weaponManager;
    private PlayerShoot playerShoot;
    private WeaponManager weaponManager;

    private bool playerPickup = false;

    private WeaponInfoScript weaponInfo;

    private const string PLAYER_TAG = "Player";
    private const string PICKUP_TAG = "Pickup";

    void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();
    }

    void OnTriggerEnter(Collider _pickup)
    {
        if (_pickup.tag == PICKUP_TAG)
        {
            if (currentWeapon.mags >= currentWeapon.maxMags)
                return;

            currentWeapon.mags++;
            StartCoroutine(Ammo_Coroutine(_pickup.gameObject));
        }
    }

    private IEnumerator Ammo_Coroutine(GameObject _pickup)
    {
        DisableOrEnableComponents(false, _pickup);
        yield return new WaitForSeconds(30);
        DisableOrEnableComponents(true, _pickup);
    }

    void DisableOrEnableComponents(bool _bool, GameObject _pickup)
    {
        _pickup.gameObject.GetComponent<Renderer>().enabled = _bool;
        _pickup.gameObject.GetComponent<BoxCollider>().enabled = _bool;
    }

}
