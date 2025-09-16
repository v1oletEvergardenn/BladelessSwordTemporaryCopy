using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;

public abstract class IDamagable : MonoBehaviour
{
    [FoldoutGroup("HitPos", nameof(getHitPosition), nameof(color))] public Void hitposVoid1;
    [SerializeField, HideInInspector] public Vector3 getHitPosition;
    [SerializeField, HideInInspector] public Color color = Color.red;

    [HideInInspector] public HashSet<SubDamageable> subDamagables = new HashSet<SubDamageable>();

    public virtual int Damage(float damageAmount, Transform sender = null, float stunDuration = 0f, bool damageFlash = true, float stunValue = 0)
    {
        return 0;
    }

    public virtual int SubObjectDamage(float damageAmount, Transform sender = null, float stunDuration = 0f, float stunValue = 0)
    {
        return 0;
    }

    public virtual void Repel(float force, bool left)
    {
        GetComponent<Rigidbody2D>().AddForce((left ? Vector3.left : Vector3.right) * force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        Gizmos.DrawLine(this.transform.position, getHitPosition + this.transform.position);
        Gizmos.DrawWireSphere(this.transform.position + getHitPosition, 0.05f);
    }

    /// <summary>
    /// offset the pivot/hit position for other to aim
    /// </summary>
    /// <returns> a position to aim</returns>
    public Vector3 GetHitPos()
    {
        return getHitPosition + transform.position;
    }
}