using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof (WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

	private const string PLAYER_TAG = "Player";
    private const string PICKUP_TAG = "Pickup";

    [SerializeField]
	private Camera cam;

	[SerializeField]
	private LayerMask mask;

	private PlayerWeapon currentWeapon;
	private WeaponManager weaponManager;
    private SoundScript soundScript;

    private AudioClip audioClip;

    private System.Random rnd = new System.Random();

    private bool isMeleeing = false;

	void Start ()
	{
		if (cam == null)
		{
			Debug.LogError("PlayerShoot: No camera referenced!");
			this.enabled = false;
		}

		weaponManager = GetComponent<WeaponManager>();
        soundScript = GetComponent<SoundScript>();
	}

	void Update ()
	{
		currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.IsOn)
            return;

        if(currentWeapon.bullets < currentWeapon.maxBullets && currentWeapon.mags > 0)
        {
            if (Input.GetButtonDown("Reload"))
            {
                weaponManager.Reload();
                return;
            }
        }

        if(Input.GetButtonDown("Melee"))
        {
            weaponManager.Melee();
            Melee();
            return;
        }


		if (currentWeapon.fireRate <= 0f)
		{
			if (Input.GetButtonDown("Fire1"))
			{
                Shoot();
			}
		} else
		{
			if (Input.GetButtonDown("Fire1"))
			{
                if (currentWeapon.bullets > 0)
                {
                    InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
                }
			} else if (Input.GetButtonUp ("Fire1"))
			{
				CancelInvoke("Shoot");
			}
		}
	}

    //called on server when player shoot
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    //called on all clients when shoot effect is needed
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
       // currentWeapon.shootEffect.Play();
    }

    //called on server when hit something
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal, bool _human, bool shot)
    {
        RpcDoHitEffect(_pos, _normal, _human, shot);
    }

    //called on client when hit something
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal, bool _human, bool shot)
    {
        GameObject _hitEffect;
        if (_human)
        {
            _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitHumanEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        }
        else
        {
            _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        }
        if (!shot)
            soundScript.playSound(4);
//        soundScript.playSound(rnd.Next(0, 2));

        Destroy(_hitEffect, 2f);
    }

    [Client]
	void Shoot ()
	{
        if (!isLocalPlayer || weaponManager.isReloading || isMeleeing)
        {
            return;
        }

        if(currentWeapon.bullets <= 0 && currentWeapon.mags > 0)
        {
            weaponManager.Reload();
            return;
        }
        currentWeapon.bullets--;

        //call onshoot on server
           // soundScript.playSound(3);
           
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = weaponManager.GetCurrentAudioClip(2);
            audio.Play();
        
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
            CmdOnShoot();
        

        RaycastHit _hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
            {
                if (_hit.collider.tag == PLAYER_TAG)
                {
                    CmdPlayerHit(_hit.collider.name, currentWeapon.damage, transform.name);
                    CmdOnHit(_hit.point, _hit.normal, true, true);
                }
                else
                {
                    CmdOnHit(_hit.point, _hit.normal, false, true);
                }
                if (currentWeapon.bullets <= 0 && currentWeapon.mags > 0)
                {
                    weaponManager.Reload();
                }
            }
	}
    
    [Client]
    void Melee()
    {
        if (!isLocalPlayer || weaponManager.isReloading || isMeleeing)
        {
            return;
        }
        /*soundScript.playSound(rnd.Next(0, 2));*/
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = weaponManager.GetCurrentAudioClip(rnd.Next(0, 1));
        audio.Play();

        //call onshoot on server
        StartCoroutine(Melee_Coroutine());
        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.meleeRange, mask))
        {
            if (_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerHit(_hit.collider.name, currentWeapon.meleeDamage, transform.name);
                CmdOnHit(_hit.point, _hit.normal, true, false);
            }
            else
            {
                CmdOnHit(_hit.point, _hit.normal, false, false);
            }
        }
    }

    [Command]
	void CmdPlayerHit (string _playerID, int _damage, string _sourceID)
	{
		Debug.Log(_playerID + " has been shot.");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
	}

    private IEnumerator Melee_Coroutine()
    {
        isMeleeing = true;

        yield return new WaitForSeconds(currentWeapon.meleeTime);

        isMeleeing = false;
    }

    bool magCheck()
    {
        if (currentWeapon.mags >= currentWeapon.maxMags)
            return false;
        else
            currentWeapon.mags++;
        return true;    
    }

    /*
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
    */
}
