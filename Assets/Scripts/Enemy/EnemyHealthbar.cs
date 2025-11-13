using System;
using UnityEngine;
using UnityEngine.UI;
public class EnemyHealthbar : MonoBehaviour
{
    [NonSerialized] private EnemyStats enemyStats;
    [NonSerialized] private Vector3 cachedscale;
    private void Awake()
    {
        cachedscale=transform.localScale;
        enemyStats = GetComponentInParent<EnemyStats>();

    }
    private void Start()
    {
        enemyStats.OnHealthChanged += HandleHealthChanged;                  
    }
    //private void Update()
    //{
    //    healthbarImage.fillAmount = enemyStats.health / enemyStats.maxHealth;
    //}
    public void HandleHealthChanged()
    {
        transform.localScale = new Vector3(enemyStats.health / enemyStats.maxHealth * cachedscale.x, cachedscale.y, cachedscale.z);
    }
}
