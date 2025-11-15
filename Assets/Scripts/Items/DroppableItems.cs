using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedDropItem<T> where T : BaseItem 
{
    public T itemPrefab;
    public int weight;
}

public enum DropCategory
{
    Consumable,
    Weapon,
    Ability,
    None
}

[Serializable]
public class DropCategoryWeight
{
    public DropCategory category;
    public int weight;
}



[CreateAssetMenu(fileName = "New Items", menuName = "DroppableItems")]
public class DroppableItems : ScriptableObject
{
    public List<DropCategoryWeight> catrgoryWeightList;
    public List<WeightedDropItem<ConsumableItem>> consumableList;
    public List<WeightedDropItem<WeaponItem>> weaponList;
    public List<WeightedDropItem<AbilityItem>> abilityList;

    public GameObject DropItem()
    {
        switch(RollCategory())
        {
            case DropCategory.Consumable:
                return RollItem(consumableList);               
            case DropCategory.Weapon:
                return RollItem(weaponList);
            case DropCategory.Ability:
                return RollItem(abilityList);
            default:
                break;
        }
        return null;
    }
    public GameObject RollItem<T>(List<WeightedDropItem<T>> list) where T : BaseItem
    {
        int totalWeight=0;
        foreach(WeightedDropItem<T> item in list) 
        { 
            totalWeight += item.weight; 
        }
        int value=UnityEngine.Random.Range(0, totalWeight);
        foreach(WeightedDropItem<T> item in list)
        {
            if (value<item.weight)
            {
                return item.itemPrefab.gameObject;
            }
            value-=item.weight;
        }
        return null;
    }
    public DropCategory RollCategory()
    {
        int totalWeight = 0;
        foreach (DropCategoryWeight categoryweight in catrgoryWeightList)
        {
            totalWeight += categoryweight.weight;
        }
        int value= UnityEngine.Random.Range(0,totalWeight);
        foreach(DropCategoryWeight categoryweight in catrgoryWeightList)
        {
            if (value<categoryweight.weight)
            {
                return categoryweight.category;
            }
            value-=categoryweight.weight;
        }
        return DropCategory.None;
    }

}
