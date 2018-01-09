using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class WeaponManager : NetworkBehaviour {

	[SerializeField]
	private string weaponLayerName = "Weapon";

	[SerializeField]
	private Transform weaponHolder;

	[SerializeField]
	private PlayerWeapon primaryWeapon;

    [SerializeField]
    GameObject playerUI;

    [SerializeField]
    PlayerWeapon[] weaponsList;

    public AudioClip[] audioClip;

    private PlayerWeapon currentWeapon;
    private WeaponInfoScript pickupWeapon;
    private WeaponGraphics currentGraphics;

    private const string PICKUP_TAG = "Pickup";
    private const string WEAPON_TAG = "Weapon";

    public bool isReloading = false;
    public bool isMeleeing = false;

	void Start ()
	{
		EquipWeapon(primaryWeapon);
        primaryWeapon.bullets = primaryWeapon.maxBullets;
        primaryWeapon.mags = primaryWeapon.maxMags;
        playerUI.GetComponents<Text>();
    }

	public PlayerWeapon GetCurrentWeapon ()
	{
		return currentWeapon;
	}

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public AudioClip GetCurrentAudioClip(int clip)
    {
        return audioClip[clip];
    }

    void EquipWeapon (PlayerWeapon _weapon)
	{
        if (currentWeapon != null)
            UnequipWeapon();

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

    void UnequipWeapon()
    {
        foreach (Transform child in weaponHolder)
        {
            if (child.tag == WEAPON_TAG)
                Destroy(child.gameObject);
        }
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

    void OnTriggerEnter(Collider _pickup)
    {
        pickupWeapon = _pickup.GetComponent<WeaponInfoScript>();
        if (_pickup.tag == PICKUP_TAG)
        {
            Debug.Log("Pickup har ID " + pickupWeapon.weaponID + " och vapnet pa hand har ID " + currentWeapon.weaponID);
            if (pickupWeapon.weaponID == currentWeapon.weaponID)
            {
                if (currentWeapon.mags >= currentWeapon.maxMags)
                    return;

                currentWeapon.mags = currentWeapon.maxMags;
                StartCoroutine(Ammo_Coroutine(_pickup.gameObject));
            }
            else
            {
                Debug.Log("Gar alternativ vag");
                FindCorrectID(pickupWeapon);
            }
        }
    }

    void FindCorrectID(WeaponInfoScript _pickup)
    {
        for (int i = 0; i < weaponsList.Length; i++)
        {    
            if (weaponsList[i].weaponID == _pickup.weaponID)
            {
                Debug.Log("The weapon lying on the ground is " + weaponsList[i].name);
                EquipWeapon(weaponsList[i]);
            }
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
