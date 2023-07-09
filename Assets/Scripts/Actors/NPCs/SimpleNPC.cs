using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNPC : CombatBase
{
    [HideInInspector] public string Nickname = "Bob";

    [SerializeField] protected Transform backpackParent;

    [Header("Wandering")]
    [SerializeField] private RangeF wanderTime;
    private bool wandering = true;
    private float nextChange;
    private Vector2 wanderTarget;

    protected Transform moveTarget;

    protected BoxCollider2D coll;
    protected Animator anim;

    protected override void Awake()
    {
        base.Awake();

        coll = GetComponent<BoxCollider2D>();
        anim = GetComponentInChildren<Animator>();
    }

    public void SetBackpack(PlayerController backpack)
    {
        backpack.SetCarryNpc(this);
        backpack.transform.parent = backpackParent;
        backpack.transform.localPosition = Vector3.zero;
        backpack.transform.localRotation = Quaternion.identity;
        backpack.transform.localScale = Vector3.one;
    }

    public void OnStartRun()
    {
        maxHealth = Upgrades.Instance.healthTotal.Val;
        currHealth = maxHealth;
        damageMod = Upgrades.Instance.damageMod.Val;
    }

    protected override void Update()
    {
        if (GameController.Instance.Paused) return;

        base.Update();

        anim.SetBool("Walking", rb.velocity.magnitude > 0);
    }

    public void MoveTowardsDoor(Transform door)
    {
        moveTarget = door;
    }

    protected override Vector3 GetMoveTarget()
    {
        if (moveTarget)
        {
            Debug.DrawLine(transform.position, moveTarget.position);
            if (Vector3.Distance(transform.position, moveTarget.position) > 3)
                return moveTarget.position;
            else
                return transform.position;
        }

        if (currTarget || (currTarget = SelectTarget()))
            return base.GetMoveTarget();

        if (Time.time > nextChange)
        {
            nextChange = Time.time + wanderTime.RandomVal;
            wandering = !wandering;
            wanderTarget = (Vector2)transform.position + Random.insideUnitCircle.normalized * 50;
        }

        return wandering ? wanderTarget : transform.position;
    }

    public void SetColliders(bool active)
    {
        coll.enabled = active;
    }

    protected override void Death()
    {
        if (currWeapon)
        {
            GameController.Instance.currNpcWeapon = currWeapon;
            UnequipWeapon();
        }

        GameUI.Instance.SetEnd(true);

        base.Death();
    }
}
