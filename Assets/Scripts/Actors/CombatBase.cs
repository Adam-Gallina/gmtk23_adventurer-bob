using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class CombatBase : MovementBase
{
    [Header("Combat")]
    [SerializeField] protected float maxAggroDist;
    [SerializeField] protected LayerMask damageLayers;
    [SerializeField] protected LayerMask targetLayers;
    [SerializeField] protected Transform weaponParent;
    [SerializeField] protected WeaponBase defaultWeaponPrefab;
    protected WeaponBase defaultWeapon;
    [SerializeField] protected WeaponBase spawnWeaponPrefab;
    [HideInInspector] public WeaponBase currWeapon;
    protected HealthBase currTarget;
    [SerializeField] protected float damageMod = 0;
    public HealthBase Target { get { return currTarget; } }

    protected virtual void Start()
    {
        if (defaultWeaponPrefab)
        {
            defaultWeapon = Instantiate(defaultWeaponPrefab, transform);
            EquipWeapon(defaultWeapon, false);
        }

        if (spawnWeaponPrefab)
            EquipWeapon(Instantiate(spawnWeaponPrefab));
    }

    public bool EquipWeapon(WeaponBase weapon, bool replacePrevious=true)
    {
        if (weapon.EquipWeapon(this, weaponParent, damageLayers))
        {
            if (replacePrevious)
                currWeapon = weapon;
            return true;
        }

        return false;
    }

    public bool UnequipWeapon()
    {
        if (currWeapon && !currWeapon.UnequipWeapon())
            return false;
        currWeapon = null;

        return true;
    }


    #region Targeting
    protected virtual bool IsValidTarget(HealthBase target) { return target != this; }

    protected Collider2D[] GetPossibleTargets(float range)
    {
        return Physics2D.OverlapCircleAll(transform.position, range == -1 ? 50 : range, targetLayers.value);
    }

    protected HealthBase SelectClosestTarget(Collider2D[] targets)
    {
        float closestDist = Mathf.Infinity;
        HealthBase closest = null;
        foreach (Collider2D h in targets)
        {
            HealthBase hb = h.GetComponentInParent<HealthBase>();
            if (hb)
            {
                if (!IsValidTarget(hb))
                    continue;

                float d = Vector2.Distance(transform.position, h.ClosestPoint(transform.position));
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = hb;
                }
            }
        }

        return closest;
    }

    protected virtual HealthBase SelectTarget()
    {
        return SelectClosestTarget(GetPossibleTargets(maxAggroDist));
    }
    #endregion

    protected override Vector3 GetMoveTarget()
    {
        if (currTarget || (currTarget = SelectTarget()))
            return currTarget.transform.position;

        return transform.position;
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();

        if (currTarget)
        {
            WeaponBase w = currWeapon ? currWeapon : defaultWeapon;

            if (w)
            {
                w.Aim(currTarget.transform);
                if (Vector3.Distance(transform.position, currTarget.transform.position) < w.range)
                    w.Attack(damageMod);
            }

            transform.up = currTarget.transform.position - transform.position;
        }
        else 
        {
            if (rb.velocity.magnitude > 0)
                transform.up = rb.velocity;

            WeaponBase w = currWeapon ? currWeapon : defaultWeapon;
            if (w)
                w.Aim(transform.forward);
        }
    }
}
