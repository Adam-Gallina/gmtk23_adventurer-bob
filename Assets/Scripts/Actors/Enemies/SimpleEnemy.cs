using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : CombatBase
{
    protected Animator anim;

    [Header("Loot drops")]
    [SerializeField] [Range(0, 1)] protected float lootChance;
    [SerializeField] protected LootWeights[] dropWeights;
    [SerializeField] protected int dropWeight;
    [SerializeField] protected float dropSpread;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        if (GameController.Instance.Paused) return;

        base.Update();

        anim.SetBool("Walking", rb.velocity.magnitude > 0);
    }

    protected override void OnDeath()
    {
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
