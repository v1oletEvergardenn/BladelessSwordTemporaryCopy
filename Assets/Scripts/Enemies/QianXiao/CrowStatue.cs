using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowStatue : AdvancedShooter
{
    // Start is called before the first frame update

    public override IEnumerator IEShoot()
    {
        isShooting = true;
        anim.Play("Crow_shoot");
        shootPos.gameObject.SetActive(true);

        yield return new WaitForSeconds(roundCD);
        vfx.SpawnSlashEffect(shootPos.position, true);
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < bulletAmountInOneRound; i++)
        {
            yield return new WaitForSeconds(bulletCDInOneRound);
            Shoot();
        }
        isShooting = false;
        anim.Play("CrowStatue");
        shootPos.gameObject.SetActive(false);
        afterRoundShooting.Invoke();
    }
}