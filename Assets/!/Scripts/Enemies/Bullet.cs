using UnityEngine;

public class Bullet : IProjectile
{
    public bool dodged = false;

    public override void SetUp(Vector3 dir, GameObject _owner, float additionSpeed = 0f, bool _followTarget = false,
        IDamagable _target = null, bool _isHostileToPlayer = true,
        float _damage = 0, float _speed = -1, float gravityScale = 0, float _stunValue = 0)
    {
        ResetAttributes();
        owner = _owner;
        followTarget = _followTarget;
        target = _target;
        isHostileToPlayer = _isHostileToPlayer;
        if (_damage != 0)
        {
            damage = _damage;
        }
        transform.eulerAngles = dir;// rotate to given direction
        if (target != null) { transform.rotation = CalculateWantedRotation(target.GetHitPos()); } //rotate to face target
        if (_speed != -1) { speed = _speed; originalSpeed = _speed; }
        speed = originalSpeed + additionSpeed;
        isPerfect = false;
        lifeTimer = 0f;
        dodged = false;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();
        if (target != null && collision.gameObject != owner && !collided)
        {
            if (collision.gameObject == gameManager.player && collision.gameObject.layer == 14 && !dodged)
            {
                owner.GetComponent<Shooter>().dodgedTimes += 1;
                dodged = true;
                return;
            }
            if (collision.gameObject.layer == 14) { return; }
            vfx.SpawnHitEffect(false, GetPivot());
            target.Damage(damage, transform, stunDuration);
            this.gameObject.SetActive(false);
            if (collision.gameObject == gameManager.player && gameManager.player.GetComponent<PlayerAttack>().isDefending && owner.GetComponent<Shooter>().defendTest)
            {
                owner.GetComponent<ShooterOnHIt>().Die?.Invoke();
            }
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            Die();
            collided = true;
        }
    }
}