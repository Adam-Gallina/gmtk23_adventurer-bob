using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBase : MonoBehaviour
{
    public float maxHealth;
    [SerializeField] protected bool canDamage = true;
    [HideInInspector] public float currHealth;

    protected virtual void Awake()
    {
        currHealth = maxHealth;
    }

    public bool Damage(float amount)
    {
        if (!canDamage)
            return false;

        currHealth -= amount;

        if (currHealth <= 0)
        {
            Death();
        }

        return true;
    }

    protected virtual void OnDeath() { }

    protected virtual void Death()
    {
        OnDeath();
        Destroy(gameObject);
    }
}
