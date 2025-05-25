using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;

public abstract class IDamagable : MonoBehaviour
{
    [FoldoutGroup("HitPos", nameof(getHitPosition), nameof(color))] public Void hitposVoid1;
    [SerializeField, HideInInspector] private Vector3 getHitPosition;
    [SerializeField, HideInInspector] public Color color = Color.red;

    [HideInInspector] public HashSet<SubDamageable> subDamagables = new HashSet<SubDamageable>();

    public virtual int Damage(int damageAmount, Transform sender = null, float stunDuration = 0f)
    {
        return 0;
    }

    public virtual int SubObjectDamage(int damageAmount, Transform sender = null, float stunDuration = 0f)
    {
        return 0;
    }

    public virtual void Repel(float force, Vector3 dir)
    {
        GetComponent<Rigidbody2D>().AddForce(dir * force, ForceMode2D.Impulse);
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