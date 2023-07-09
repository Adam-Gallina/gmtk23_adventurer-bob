using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase : HealthBase
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed;

    protected Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (GameController.Instance.Paused) return;

        HandleMovement();
    }

    protected abstract Vector3 GetMoveTarget();
    protected virtual void HandleMovement()
    {
        Vector2 dir = GetMoveTarget() - transform.position;
        rb.velocity = dir.normalized * moveSpeed;
    }
}
