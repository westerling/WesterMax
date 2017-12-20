using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent (typeof (WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

	private const string PLAYER_TAG = "Player";

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private LayerMask mask;

	private PlayerWeapon currentWeapon;
	private WeaponManager weaponManager;

    private bool isMeleeing = false;

	void Start ()
	{
		if (cam == null)
		{
			Debug.LogError("PlayerShoot: No camera referenced!");
			this.enabled = false;
		}

		weaponManager = GetComponent<WeaponManager>();
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
            currentWeapon.meleeEffect.Play();

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
            currentWeapon.shootEffect.Play();
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

}
