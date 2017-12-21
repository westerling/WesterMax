using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeaponInfoScript : MonoBehaviour{

    public string name;
    public int damage;
    public int meleeDamage;
    public float range;
    public float meleeRange;
    public float fireRate;
    public int maxMags;
    public int maxBullets;
    public AudioSource shootEffect;
    public AudioSource meleeEffect;

    [HideInInspector]
    public int bullets;

    public int mags;

    public float reloadTime;

    public float meleeTime;

    public GameObject graphics;

    public WeaponInfoScript()
    {
        bullets = maxBullets;
    }

}
