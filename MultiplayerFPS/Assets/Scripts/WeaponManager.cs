using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WeaponManager : NetworkBehaviour {

	[SerializeField]
	private string weaponLayerName = "Weapon";

	[SerializeField]
	private Transform weaponHolder;

	[SerializeField]
	private PlayerWeapon primaryWeapon;

	private PlayerWeapon currentWeapon; 
    private WeaponGraphics currentGraphics;
    private AudioSource currentShootSound;
    private AudioSource currentMeleeSound;

    public bool isReloading = false;
    public bool isMeleeing = false;

	void Start ()
	{
		EquipWeapon(primaryWeapon);
        primaryWeapon.bullets = primaryWeapon.maxBullets;
        primaryWeapon.mags = primaryWeapon.maxMags;
    }

	public PlayerWeapon GetCurrentWeapon ()
	{
		return currentWeapon;
	}

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }


    void EquipWeapon (PlayerWeapon _weapon)
	{
		currentWeapon = _weapon;

		GameObject _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
		_weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = _weaponIns.GetComponent<WeaponGraphics>();
        if(currentGraphics == null)
        {
            Debug.LogError("No graphics on " + _weaponIns.name);
        }


        if (isLocalPlayer)
            Util.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer(weaponLayerName));

	}

    public void Melee()
    {
        if (isReloading || isMeleeing)
            return;

        StartCoroutine(Melee_Coroutine());
    }

    public void Reload()
    {
        if (isReloading || isMeleeing)
            return;

        StartCoroutine(Reload_Coroutine());
    }

    private IEnumerator Reload_Coroutine()
    {
        
        isReloading = true;

        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;

        currentWeapon.mags--;

        isReloading = false;
    }

    private IEnumerator Melee_Coroutine()
    {
        isMeleeing = true;

        CmdOnMelee();

        yield return new WaitForSeconds(currentWeapon.meleeTime);

        isMeleeing = false;
    }

    [Command]
    void CmdOnMelee()
    {
        RpcOnMelee();
    }

    [Command]
    void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    void RpcOnMelee()
    {
        Animator anim = currentGraphics.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Melee");
        }
    }


    [ClientRpc]
    void RpcOnReload()
    {
        Animator anim = currentGraphics.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Reload");
        }
    }
}
