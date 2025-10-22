using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Items", menuName = "DroppableItems")]
public class DroppableItems : ScriptableObject
{
    public List<BaseItem> ConsumableList;
    public List<PlayerWeapon> weaponList;

}
