using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AdvancedShooter : MonoBehaviour
{
    [Header("references")] public GameObject senderOfProjectile;
    public Transform shootPos;
    public string projectileName = "bullet";
    [Header("stats")] public int Damage = 1;
    public float bulletSpeed = 200;
    public float bulletRotationSpeed = 200;
    public int bulletAmountInOneRound = 1;
    public float bulletCDInOneRound = 0.2f;
    public float roundCD = 3f;

    [Header("settings")] public bool isFacingRight = false;
    public bool canShoot = true;
    public bool canFlip = false;
    public bool aimPlayer = false;
    public bool follow_player = false;

    public UnityEvent afterRoundShooting;

    [HideInInspector] public ObjectPooler pooler;
    [HideInInspector] public float CDtimer = 0f;
    [HideInInspector] public IDamagable playerIDamagable;
    [HideInInspector] public bool isShooting = false;
    [HideInInspector] public Animator anim;
    [HideInInspector] public VFXManager vfx;

    public virtual void Start()
    {
        playerIDamagable = GameManager.instance.playerhealth;
        pooler = ObjectPooler.instance;
        anim = GetComponent<Animator>();
        vfx = VFXManager.instance;
        if (senderOfProjectile == null)
        {
            senderOfProjectile = this.gameObject;
        }
    }

    public virtual void Update()
    {
        if (!isShooting && canShoot)
        {
            CDtimer += Time.deltaTime;
            if (CDtimer >= roundCD)
            {
                StartCoroutine(IEShoot());
                CDtimer = 0f;
            }
        }
        if (canFlip)
        {
            Vector3 targetDir = (playerIDamagable.GetHitPos() - transform.position).normalized;
            Vector3 forward = transform.right;
            //print(Vector3.SignedAngle(targetDir, forward, transform.up));
            if (playerIDamagable.GetHitPos().x >= this.transform.position.x && !isFacingRight) { Flip(); }//face right
            else if (playerIDamagable.GetHitPos().x < this.transform.position.x && isFacingRight) { Flip(); } //face left
        }
    }

    public virtual void Shoot()
    {
        if (!canShoot) { return; }
        IProjectile _bullet = pooler.SpawnFromPool(projectileName, shootPos.position, shootPos.rotation).GetComponent<IProjectile>();
        _bullet.rotationSpeed = bulletRotationSpeed;
        if (aimPlayer)
        {
            _bullet.SetUp(shootPos.eulerAngles, senderOfProjectile, 0, follow_player, GameManager.instance.player.GetComponent<IDamagable>(), _damage: Damage, _speed: bulletSpeed);
        }
        else
        {
            _bullet.SetUp(shootPos.eulerAngles, senderOfProjectile, 0, false, _damage: Damage, _speed: bulletSpeed);
        }
    }

    public virtual IEnumerator IEShoot()
    {
        isShooting = true;
        for (int i = 0; i < bulletAmountInOneRound; i++)
        {
            yield return new WaitForSeconds(bulletCDInOneRound);
            Shoot();
        }
        isShooting = false;
        afterRoundShooting.Invoke();
    }

    public virtual void Flip()
    {
        if (canFlip)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(new Vector3(0, 1, 0), 180);
            shootPos.Rotate(new Vector3(1, 0, 0), 180);
        }
    }

    public virtual void Activate()
    {
        canShoot = true;
    }

    public virtual void Deactivate()
    {
        canShoot = false;
    }
}