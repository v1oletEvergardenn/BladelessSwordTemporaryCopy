using DG.Tweening;
using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public abstract class IDamagable : MonoBehaviour
{
    [SerializeField] public Vector3 getHitPosition;
    [SerializeField] public Color color = Color.red;
    public bool canBeHitWithoutHSAttack = false;
    public bool resetAttackCDOnHit = false;
    public bool consumeEnergyOnHit = true;
    public List<SubDamageable> subDamagables = new List<SubDamageable>();

    public virtual int Damage(float damageAmount, Transform sender = null, float stunDuration = 0f, bool damageFlash = true, float bossBreakValue = 0)
    {
        return 0;
    }

    public virtual int Damage(IProjectileBasicAttributes attributes, Transform sender = null, bool damageFlash = true)
    {
        Damage(attributes.damage, sender, attributes.stunDuration, damageFlash, attributes.bossBreakValue);
        return 0;
    }

    public virtual int SubObjectDamage(float damageAmount, Transform sender = null, float stunDuration = 0f, float stunValue = 0)
    {
        return 0;
    }

    public virtual void Repel(float force, bool left)
    {
        TryGetComponent<Rigidbody2D>(out var rb);
        if (rb != null)
        {
            rb.AddForce((left ? Vector3.left : Vector3.right) * force, ForceMode2D.Impulse);
        }
    }

    public void ForceRepel(float force, bool left)
    {
        TryGetComponent<Rigidbody2D>(out var rb);
        if (rb != null)
        {
            rb.AddForce((left ? Vector3.left : Vector3.right) * force, ForceMode2D.Impulse);
        }
    }

    public void RepelInDistance(float distance)
    {
        TryGetComponent<Rigidbody2D>(out var rb);
        if (rb != null)
        {
            Vector2 targetPosition = rb.position + (Vector2.right * distance);

            rb.velocity = Vector2.zero;
            rb.DOKill();
            rb.DOMove(targetPosition, 30)
                .SetEase(Ease.OutSine)
                .SetSpeedBased(true)
                .SetUpdate(UpdateType.Fixed);
        }
    }

    public void RepelToPosition(Vector3 targetPosition)
    {
        TryGetComponent<Rigidbody2D>(out var rb);
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.DOKill();
            rb.DOMove(targetPosition, 30)
                .SetEase(Ease.OutSine)
                .SetSpeedBased(true)
                .SetUpdate(UpdateType.Fixed);
        }
    }

    public void RepelToPosition(Transform targetPosition)
    {
        TryGetComponent<Rigidbody2D>(out var rb);
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.DOKill();
            rb.DOMove(targetPosition.position, 1000)
                .SetEase(Ease.OutSine)
                .SetSpeedBased(true)
                .SetUpdate(UpdateType.Fixed);
        }
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