using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Shooter : MonoBehaviour
{
    public Transform shootPosition;
    private bool isShooting = false;
    private Transform target;
    public bool isFacingRight;
    private ObjectPooler pooler;
    public bool canShoot = true;
    public bool canTurn = false;
    public bool aimPlayer = false;
    public bool follow_player = false;
    public int Damage = 10;

    public float bulletSpeed = 200;
    public float bulletRotationSpeed = 200;
    public int bulletAmountInOneRound = 1;
    public float bulletCDInOneRound = 0.2f;
    public float roundCD;
    private float CDtimer = 0f;
    public bool NeedTobeINRange = false;
    public float shootRange = 20f;

    [Header("mutiple bullet path")]
    public int shootPath = 1;

    public float pathGap = 0.2f;
    public bool shootSameTime = false;

    [Header("dodgeTest")]
    public bool enableDashTest = false;

    public int dodgedTimes = 0;
    public UnityEvent dashTutComplete;
    public bool defendTest;

    // Start is called before the first frame update
    private void Start()
    {
        target = PlayerAttack.instance.gameObject.transform;
        pooler = ObjectPooler.instance;
    }

    // Update is called once per frame
    private void Update()
    {
        if (enableDashTest && dodgedTimes >= 3)
        {
            dashTutComplete.Invoke();
        }
        if (NeedTobeINRange)
        {
            if (Vector3.Distance(transform.position, target.transform.position) > shootRange)
            {
                return;
            }
        }
        if (!isShooting)
        {
            CDtimer += Time.deltaTime;
            if (CDtimer >= roundCD)
            {
                StartCoroutine(Foo());
                CDtimer = 0f;
            }
        }
        Vector3 targetDir = (target.position - transform.position).normalized;
        Vector3 forward = transform.right;
        //print(Vector3.SignedAngle(targetDir, forward, transform.up));
        if (target.position.x >= this.transform.position.x && !isFacingRight) { Flip(); }//face right
        else if (target.position.x < this.transform.position.x && isFacingRight) { Flip(); } //face left
    }

    public void Flip()
    {
        if (canTurn)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(new Vector3(0, 1, 0), 180);
            shootPosition.Rotate(new Vector3(1, 0, 0), 180);
        }
    }

    public void OnShoot(Vector3 pos)
    {
        if (!canShoot) { return; }
        IProjectile _bullet = pooler.SpawnFromPool("bullet", pos, shootPosition.rotation).GetComponent<IProjectile>();
        _bullet.rotationSpeed = bulletRotationSpeed;
        if (aimPlayer)
        {
            _bullet.SetUp(shootPosition.eulerAngles, this.gameObject, 0, follow_player, GameManager.instance.Player.GetComponent<IDamagable>(), _damage: Damage, _speed: bulletSpeed);
        }
        else
        {
            _bullet.SetUp(shootPosition.eulerAngles, this.gameObject, 0, false, _damage: Damage, _speed: bulletSpeed);
        }
    }

    private IEnumerator Foo()
    {
        isShooting = true;
        for (int i = 0; i < bulletAmountInOneRound; i++)
        {
            OnShoot(shootPosition.position);
            for (int j = 1; j < shootPath; j++)
            {
                if (!shootSameTime) { yield return new WaitForSeconds(bulletCDInOneRound); }
                OnShoot(shootPosition.position - new Vector3(0, pathGap, 0));
                i++;
            }

            yield return new WaitForSeconds(bulletCDInOneRound);
        }
        isShooting = false;
    }

    public void Activate()
    {
        canShoot = true;
    }

    public void Deactivate()
    {
        canShoot = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}