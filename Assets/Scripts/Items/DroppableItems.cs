using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Items", menuName = "DroppableItems")]
public class DroppableItems : ScriptableObject
{
    
    public List<GameObject> consumableList;
    public float consumableDropProbability;
    public List<PlayerWeapon> weaponList;
    public float weaponDropProbability;
}
