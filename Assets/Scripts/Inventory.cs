using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class Inventory: MonoBehaviour
{
    public List<PlayerWeapon> weapons;
    private PlayerControllerTest pcTest;
    public bool hasKey = false;

    private void Start()
    {
        pcTest=GetComponent<PlayerControllerTest>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "treasure_2")
        {
            hasKey = true;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.name == "treasure_1")
        {
            weapons.Add(other.gameObject.GetComponent<PropsList>().playerWeapon);
            pcTest.EquipWeapon(weapons.Last());
            Destroy(other.gameObject);
        }
    }
    
    public bool HasKey()
    {
        return hasKey;
    }
    
    
}