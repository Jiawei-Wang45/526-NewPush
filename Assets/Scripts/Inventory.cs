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
    }
    
    public bool HasKey()
    {
        return hasKey;
    }
    
    
}