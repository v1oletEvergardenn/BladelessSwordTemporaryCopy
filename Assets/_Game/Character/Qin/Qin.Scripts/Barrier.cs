using UnityEngine;

public class Barrier : MonoBehaviour
{
    public PlayerAttack playerAttack;
    public int lastHitNumbers = 0;

    private void OnEnable()
    {
        lastHitNumbers = 0;
    }

    private void Update()
    {
        if (playerAttack.isOnStorm)
        {
            CheckCounterAttackBarrier(playerAttack.storm_radius);
        }
    }

    public void CheckCounterAttackBarrier(float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius + 5);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<IProjectile>(out IProjectile i) && i.isHostileToPlayer)
            {
                float distance = Vector3.Distance(i.GetHitPos(), transform.position);
                if (distance <= radius) { CounterAttackBarrier(i); }
            }
        }
    }

    public void CounterAttackBarrier(IProjectile projectile)
    {
        Vector3 reverse = new Vector3(0, 0, -90);
        projectile.SetUp(reverse, this.gameObject).
            SetHostileToPlayer(false);
        projectile.NormalCounterAttack();
        lastHitNumbers++;
    }
}