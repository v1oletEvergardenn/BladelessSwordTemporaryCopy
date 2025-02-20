using UnityEngine;

public class AnimSetBool : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator bladeAnim;
    public PlayerAttack playerAttack;
    public GameObject player;
    public Rigidbody2D rb;

    public static AnimSetBool instance;
    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
    }

    #region MOVEMENT BOOL SETTINGS

    public void Anim_Reset()
    {
        controller.canMove = true;
        controller.canFlip = true;
        controller.canJump = true;
        playerAttack.canAttack = true;
        playerAttack.canAttack = true;
        playerAttack.canLaunchBoomerang = true;
        playerAttack.canStorm = true;
        playerAttack.canDefend = true;
    }

    #endregion MOVEMENT BOOL SETTINGS

    #region COMBAT BOOL SETTINGS

    public void Anim_Defend(int i)
    {
        bool b = i == 1 ? true : false;

        controller.canMove = b;
        if (!b) { controller.StopMovement(); }
        controller.canFlip = b;
        controller.canJump = b;
        playerAttack.canAttack = b;
        playerAttack.canLaunchBoomerang = b;
        playerAttack.canStorm = b;
    }

    public void Anim_Teleport(int i)
    {
        bool b = i == 1 ? true : false;
        controller.canMove = b;
        if (!b) { controller.StopMovement(); }
        controller.canFlip = b;
        controller.canJump = b;
        playerAttack.canAttack = b;
        playerAttack.canLaunchBoomerang = b;
        playerAttack.canStorm = b;
    }

    public void Anim_Hit(int i)
    {
        bool b = i == 1 ? true : false;

        controller.canMove = b;
        if (!b) { controller.StopMovement(); }
        controller.canFlip = b;
        controller.canJump = b;
        playerAttack.canAttack = b;
        playerAttack.canLaunchBoomerang = b;
        playerAttack.canStorm = b;
        playerAttack.canDefend = b;
    }

    public void Anim_Attack(int i)
    {
        bool b = i == 1 ? true : false;
        if (i == 2)
        {
            playerAttack.EndAttack();
            return;
        }
        controller.canMove = b;
        if (!b) { controller.StopMovement(); }
        controller.canFlip = b;
        controller.canJump = b;
        playerAttack.canAttack = b;
        playerAttack.canLaunchBoomerang = b;
        playerAttack.canStorm = b;
        playerAttack.canDefend = b;
        playerAttack.isAttacking = !b;
    }

    public void TranslateDistance(float x)
    {
        if (controller.m_FacingRight)
        {
            player.transform.position += new Vector3(x, 0, 0);
        }
        else
        {
            player.transform.position -= new Vector3(x, 0, 0);
        }
    }

    public void TeleportToBoomerang()
    {
        playerAttack.TeleportToSword();
    }

    #endregion COMBAT BOOL SETTINGS
}