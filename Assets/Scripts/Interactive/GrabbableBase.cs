using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrabbableType { Unknown, Weapon, Object, Item, Loot }
public abstract class GrabbableBase : HealthBase
{
    public GrabbableType grabbableType;
    [SerializeField] protected bool canGrab = true;
    [SerializeField] protected bool forcePlayerGrab = false;
    protected bool grabbed = false;

    [Header("Collisions")]
    [SerializeField] protected float collisionDamage;
    [SerializeField] protected float minCollisionSpeed;
    [SerializeField] protected float damageOnCollision;
    private Vector2 lastPos;
    [SerializeField] protected float hitDelay = 0.5f;
    protected float nextHit;

    [Header("Drops")]
    [SerializeField] protected Transform[] drops;
    [SerializeField] protected RangeI dropCount;
    [SerializeField] protected float dropSpread;
    [Header("Loot drops")]
    [SerializeField][Range(0, 1)] protected float lootChance;
    [SerializeField] protected LootWeights[] dropWeights;
    [SerializeField] protected int dropWeight;

    protected BoxCollider2D coll;

    protected override void Awake()
    {
        base.Awake();

        lastPos = transform.position;
        coll = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HealthBase hb = collision.gameObject.GetComponent<HealthBase>();
        if (hb && ((Vector2)transform.position - lastPos).magnitude / Time.deltaTime > minCollisionSpeed)
        {
            if (canDamage && Time.time > nextHit)
            {
                nextHit = Time.time + hitDelay;
                hb.Damage(collisionDamage);
                Damage(damageOnCollision);
            }
        }
    }

    protected virtual void Update()
    {
        if (GameController.Instance.Paused) return;
        
        if (coll.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            if (!grabbed && canGrab && Input.GetKey(KeyCode.Mouse0))
                PlayerController.Instance.TryGrab(this, forcePlayerGrab);
    }

    private void LateUpdate()
    {
        lastPos = transform.position;
    }

    public virtual bool GrabStart() { grabbed = true; return true; }
    public abstract void GrabUpdate(Vector3 pos);
    public virtual bool GrabEnd() { grabbed = false; return true; }


    protected override void OnDeath()
    {
        if (drops.Length > 0) {
            for (int i = dropCount.RandomVal(); i > 0; i--)
            {
                Instantiate(drops[Random.Range(0, drops.Length)], (Vector2)transform.position + Random.insideUnitCircle * dropSpread, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform.parent);
            }
        }

        if (dropWeights.Length > 0) {
            while (dropWeight > 0)
            {
                if (Random.Range(0, 100) / 100f < lootChance)
                {
                    LootWeights drop = dropWeights[Random.Range(0, dropWeights.Length)];
                    dropWeight -= drop.weight;
                    Instantiate(drop.item, (Vector2)transform.position + Random.insideUnitCircle * dropSpread, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform.parent);
                }
                else
                    dropWeight = 0;
            }
        }
    }
}
