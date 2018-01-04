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

    void OnTriggerEnter(Collider _pickup)
    {
        pickupWeapon = _pickup.GetComponent<WeaponInfoScript>();
        if (_pickup.tag == PICKUP_TAG)
        {
            Debug.Log("Pickup har ID " + pickupWeapon.weaponID + " och vapnet på hand har ID " + currentWeapon.weaponID);
            if (pickupWeapon.weaponID == currentWeapon.weaponID)
            {
                if (currentWeapon.mags >= currentWeapon.maxMags)
                    return;

                currentWeapon.mags = currentWeapon.maxMags;
                StartCoroutine(Ammo_Coroutine(_pickup.gameObject));
            }
            else
            {
                FindCorrectID(pickupWeapon);
            }
        }
    }

    void FindCorrectID(WeaponInfoScript _pickup)
    {
        Debug.Log("Upplockade pickadollen har annan id");
        for (int i = weaponsList.Length; i < weaponsList.Length; i++)
        {
            Debug.Log(weaponsList[i].name + " har id " + weaponsList[i].weaponID);
            if (weaponsList[i].weaponID == _pickup.weaponID)
            {
                Debug.Log("The correct weapon is actually" + weaponsList[i].name);
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
