using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : ItemBase
{
    [Header("Stats")]
    public float damage;
    public float speed;
    public float range;
    protected float nextAttack;
    protected LayerMask targetLayers;

    protected CombatBase equipped;

    public override bool GrabStart()
    {
        if (equipped)
        {
            if (!RoomGenerator.Instance.CurrRoom.roomCleared)
                return false;

            if (!equipped.UnequipWeapon())
                return false;
        }

        return base.GrabStart();
    }

    #region (Un)Equip
    protected virtual void OnEquipped() { }
    public virtual bool EquipWeapon(CombatBase holder, Transform parent, LayerMask targetLayers)
    {
        if (equipped)
            return false;

        OnEquipped();
        equipped = holder;
        //canGrab = false;
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        this.targetLayers = targetLayers;
        return true;
    }

    protected virtual void OnUnequipped() { }
    public virtual bool UnequipWeapon()
    {
        OnUnequipped();
        equipped = null;
        //canGrab = true;
        transform.parent = null;
        return true;
    }
    #endregion

    public virtual void Aim(Transform target) { Aim(target.position - transform.position); }
    public virtual void Aim(Vector2 dir) { }

    public void Attack(float damageMod) 
    { 
        if (Time.time > nextAttack)
        {
            nextAttack = Time.time + speed;
            
            OnAttack(damageMod);
        }
    }

    protected virtual void OnAttack(float damageMod) { StartCoroutine(AttackAnim(damageMod)); }

    protected abstract IEnumerator AttackAnim(float damageMod);
}
