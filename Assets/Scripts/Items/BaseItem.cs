using System;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    [NonSerialized] protected PlayerController pc;
    [NonSerialized] protected SpriteRenderer spriteRenderer;
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    protected virtual void Start()
    {
        pc = FindFirstObjectByType<PlayerController>();
    }
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //place holder, nothing to do here.
    }
    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        //place holder, nothing to do here.
    }
}
