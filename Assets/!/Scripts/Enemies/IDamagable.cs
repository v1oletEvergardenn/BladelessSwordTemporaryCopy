using DG.Tweening;
using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public abstract class IDamagable : MonoBehaviour
{
    [SerializeField] public Vector3 getHitPosition;
    [SerializeField] public Color color = Color.red;
    [SerializeField] public IDamagbleActionKey questKey_Idmg;
    public bool canBeHitWithoutHSAttack = false;
    public bool resetAttackCDOnHit = false;
    public bool consumeEnergyOnHit = true;
    public List<SubDamageable> subDamagables = new List<SubDamageable>();

    public virtual int Damage(float damageAmount, Transform sender = null, float stunDuration = 0f, bool damageFlash = true, float bossBreakValue = 0)
    {
        Death();
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

    public virtual void Repel(float distance, bool left)
    {
        Vector3 targetPosition = transform.position + (left ? Vector3.left : Vector3.right) * distance;

        transform.DOKill();
        transform.DOMove(targetPosition, GetRepelTime(targetPosition))
            .SetEase(Ease.OutSine)
            .SetTimeDt(this, TimeChannel.Gameplay);
    }

    public virtual void Death()
    {
        if (questKey_Idmg != null)
        {
            questKey_Idmg.OnAction(questKey_Idmg.dead);
        }
    }

    public void RepelWithoutDirection(float distance)
    {
        Vector3 targetPosition = transform.position + (Vector3.right * distance);
        transform.DOKill();
        transform.DOMove(targetPosition, GetRepelTime(targetPosition))
            .SetEase(Ease.OutSine)
            .SetTimeDt(this, TimeChannel.Gameplay);
    }

    public void RepelToPosition(Vector3 targetPosition)
    {
        transform.DOKill();
        transform.DOMove(new Vector3(targetPosition.x, transform.position.y, 0), GetRepelTime(targetPosition))
            .SetEase(Ease.OutSine)
            .SetTimeDt(this, TimeChannel.Gameplay);
    }

    public void RepelToPosition(Transform targetPosition)
    {
        RepelToPosition(targetPosition.position);
    }

    private float GetRepelTime(Vector3 target)
    {
        float maxRepelTime = 0.55f;
        float repelTimeDistanceScale = 4f;

        float distance = Mathf.Abs(target.x - transform.position.x);
        float scale = Mathf.Max(0.0001f, repelTimeDistanceScale);

        // Asymptotic normalization: keeps increasing, but slower at larger distances.
        float normalizedDistance = distance / (distance + scale);

        // OutSine-like growth curve.
        return Mathf.Sin(normalizedDistance * Mathf.PI * 0.5f) * maxRepelTime;
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