using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class WeaponManager : NetworkBehaviour {

	[SerializeField]
	private string weaponLayerName = "Weapon";

	[SerializeField]
	private Transform weaponHolder;

	//[SerializeField]
	//private PlayerWeapon primaryWeapon;

    [SerializeField]
    GameObject playerUI;

    [SerializeField]
    PlayerWeapon[] weaponsList;

    public AudioClip[] audioClip;

    private PlayerWeapon currentWeapon;
    private PlayerWeapon secondaryWeapon;
    private PlayerWeapon extraWeapon;

    private PickupInfo pickupWeapon;
    private WeaponGraphics currentGraphics;
    
    private const string PICKUP_TAG = "Pickup";
    private const string WEAPON_TAG = "Weapon";

    public bool isReloading = false;
    public bool isMeleeing = false;
    public bool isActivating = false;
    public bool globalActivating = false;

	void Start ()
	{
		EquipWeapon(weaponsList[0]);
        secondaryWeapon = weaponsList[1];
        playerUI.GetComponents<Text>();
    }

    void Update()
    {
        if (Input.GetButton("Activate"))
        {
            isActivating = true;
        }
        if (Input.GetButtonUp("Activate"))
        {
            isActivating = false;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (secondaryWeapon != null)
            {
                if (!isReloading)
                {
                    StopCoroutine(Reload_Coroutine());
                    extraWeapon = currentWeapon;
                    EquipWeapon(secondaryWeapon);
                    secondaryWeapon = extraWeapon;
                }
            }
        }

    }

    void LateUpdate()
    {
        globalActivating = false;
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

        if (currentWeapon.bullets <= 0)
        {
            currentWeapon.bullets = currentWeapon.maxBullets;
        }

		GameObject _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
		_weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = _weaponIns.GetComponent<WeaponGraphics>();
        if(currentGraphics == null)
        {
            Debug.LogError("No graphics on " + _weaponIns.name);
        }
        

        if (isLocalPlayer)
            Util.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer(weaponLayerName));

        currentWeapon.bullets = _weapon.bullets;
        currentWeapon.mags = _weapon.mags;
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

        currentWeapon.mags--;

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;

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
        pickupWeapon = _pickup.GetComponent<PickupInfo>();
        if (_pickup.tag == PICKUP_TAG)
        {
            if (pickupWeapon.weaponID == currentWeapon.weaponID)
            {
                if (currentWeapon.mags >= currentWeapon.maxMags)
                    return;

                currentWeapon.mags = currentWeapon.mags + pickupWeapon.mags;
                if(currentWeapon.mags > currentWeapon.maxMags)
                {
                    currentWeapon.mags = currentWeapon.maxMags;
                }
                StartCoroutine(Ammo_Coroutine(_pickup.gameObject));
            }
            if (pickupWeapon.weaponID == secondaryWeapon.weaponID)
            {
                if (secondaryWeapon.mags >= secondaryWeapon.maxMags)
                    return;

                secondaryWeapon.mags = secondaryWeapon.mags + pickupWeapon.mags;
                if (secondaryWeapon.mags > secondaryWeapon.maxMags)
                {
                    secondaryWeapon.mags = secondaryWeapon.maxMags;
                }
                StartCoroutine(Ammo_Coroutine(_pickup.gameObject));
            }
        }
    }

    void OnTriggerStay(Collider _pickup)
    {
        if (isActivating == true)
        {
            FindCorrectID(pickupWeapon);
            isReloading = false;
            StartCoroutine(Ammo_Coroutine(_pickup.gameObject));
            globalActivating = true;
        }
    }

    void FindCorrectID(PickupInfo _pickup)
    {
        for (int i = 0; i < weaponsList.Length; i++)
        {    
            if (weaponsList[i].weaponID == _pickup.weaponID)
            {
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

    private IEnumerator Activate_Coroutine()
    {
        yield return new WaitForSeconds(1);
    }


}
