using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_AnimTrigger : MonoBehaviour
{
    private Animator anim;
    public QianXiaoAI bossAI;
    public GameObject hitbox_slash;
    public GameObject hitbox_dashing_slash;
    public GameObject hitbox_land;
    private QX_add_land_attack add_Land_Attack;
    private QX_jump_attack jump_Attack;
    private QX_add_teleport_up_attack add_Teleport_Up;

    // Start is called before the first frame update
    private void Start()
    {
        anim = GetComponent<Animator>();
        add_Land_Attack = bossAI.GetComponent<QX_add_land_attack>();
        jump_Attack = bossAI.GetComponent<QX_jump_attack>();
        add_Teleport_Up = bossAI.GetComponent<QX_add_teleport_up_attack>();
    }

    public void TurnOffFlyEngine()
    {
        bossAI.SetFlyEngine(false);
    }

    public void Outline_Flash()
    {
        bossAI.OutLineFlash();
    }

    public void Outline_MeleeActivate(int i)
    {
        bossAI.OutLine_Activate(i);
    }

    public void Hit_Box_Slash_set(int i)
    {
        if (i == 0)
        {
            hitbox_slash.SetActive(false);
        }
        else
        {
            hitbox_slash.SetActive(true);
        }
    }

    public void Hit_Box_DashingSlash_set(int i)
    {
        if (i == 0)
        {
            hitbox_dashing_slash.SetActive(false);
        }
        else
        {
            hitbox_dashing_slash.SetActive(true);
        }
    }

    public void Hit_Box_Land_set(int i)
    {
        if (i == 0)
        {
            hitbox_land.SetActive(false);
        }
        else
        {
            hitbox_land.SetActive(true);
        }
    }

    public void jumpAttack_Shoot()
    {
        jump_Attack.ShootSwordBullet();
    }

    public void Land_slash_effect()
    {
        add_Land_Attack.Land_slash_effect();
    }

    public void teleportAttack_Shoot()
    {
        add_Teleport_Up.ShootBullet();
    }
}