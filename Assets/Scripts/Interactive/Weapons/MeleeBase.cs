using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBase : WeaponBase
{
    [Header("Stab Anim")]
    [SerializeField] protected RangeF offset;
    [SerializeField] protected float animSpeed;
    protected List<HealthBase> targets = new List<HealthBase>();


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayers.value) > 0)
        {
            HealthBase hb = collision.GetComponent<HealthBase>();
            if (hb)
                targets.Add(hb);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HealthBase hb = collision.GetComponent<HealthBase>();
        if (hb && targets.Contains(hb))
            targets.Remove(hb);
    }

    public override void Aim(Vector2 dir)
    {
        if (!stabbing)
            transform.parent.up = dir;
    }

    private bool stabbing = false;
    protected override IEnumerator AttackAnim(float damageMod)
    {
        stabbing = true;
        nextAttack = Time.time + 2 * animSpeed + speed;
        
        transform.parent.localPosition = transform.up * offset.minVal;
        float startTime = Time.time;
        
        while (Time.time < startTime + animSpeed)
        {
            float t = (Time.time - startTime) / animSpeed;
            transform.parent.localPosition = transform.up * offset.PercentVal(t);
            yield return new WaitForEndOfFrame();
        }

        for (int i = targets.Count - 1; i >= 0; i--)
            targets[i].Damage(damage + damageMod);

        transform.parent.localPosition = transform.up * offset.maxVal;
        startTime = Time.time;

        while (Time.time < startTime + animSpeed)
        {
            float t = 1 - (Time.time - startTime) / animSpeed;
            transform.parent.localPosition = transform.up * offset.PercentVal(t);
            yield return new WaitForEndOfFrame();
        }

        transform.parent.localPosition = Vector2.zero;
        stabbing = false;
    }
}
