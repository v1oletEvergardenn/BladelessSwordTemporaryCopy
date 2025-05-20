using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Boomerang : MonoBehaviour
{
    private Rigidbody2D rb;
    private CharacterController2D controller;
    public CinemachineVirtualCamera cam;
    private VFXManager vfx;
    private Animator anim;
    private float speed = 50f;
    private float startDecreaseTime = 1f;
    private float decreaseSpeed = 30f;
    private float rotationSpeed = 100f;
    private float speedMultiplier = 1f;
    [HideInInspector] public float flyingTimer = 0f;

    private Transform player;
    private bool flipped = false;
    public bool facingRight = false;
    public bool stickedInToWall = false;
    public bool retrieved = true;
    [HideInInspector] public bool isQTE = false;
    public bool isBouncing = false;
    [HideInInspector] public GameObject BounceQTEUI;
    private int bounceDirection = 0;
    public float bounceQTEtime = 4f;
    private float bounceQTEtimer = 0f;

    public LayerMask colliderLayers;
    private GameObject lastHitHardwall;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        flyingTimer = 0f;
        player = GameManager.instance.Player.transform;
        BounceQTEUI = PlayerAttack.instance.BounceUI;
        controller = player.GetComponent<CharacterController2D>();
        anim = GetComponentInChildren<Animator>();
        vfx = VFXManager.instance;
    }

    public void SetUp(float _speed, float _startDecreaseTime, float _decreaseSpeed, float _rotationSpeed)
    {
        speed = _speed;
        startDecreaseTime = _startDecreaseTime;
        decreaseSpeed = _decreaseSpeed;
        rotationSpeed = _rotationSpeed;
        speedMultiplier = 1f;
        bounceQTEtimer = 0f;
        retrieved = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (stickedInToWall) { return; }
        if (!isQTE)
        {
            rb.velocity = transform.right * speed * speedMultiplier;
        }
        if (!isBouncing)
        {
            Vector3 targetPos = player.GetComponent<Health>().GetHitPos();
            flyingTimer += Time.deltaTime;
            if (!flipped && flyingTimer >= startDecreaseTime)
            {
                speedMultiplier -= (decreaseSpeed / speed) * Time.deltaTime;
                //speedMultiplier = Mathf.Clamp01(speedMultiplier);
            }//before flip
            if (speedMultiplier <= 0f)
            {
                Flip(targetPos);//flip
            }//check flip
            if (flipped)
            {
                if (speedMultiplier < 1)
                {
                    speedMultiplier += (decreaseSpeed / speed) * Time.deltaTime;
                    speedMultiplier = Mathf.Clamp01(speedMultiplier);
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, CalculateWantedRotation(targetPos), rotationSpeed * Time.deltaTime);//follow player's position
            }//after flip
        }
        else if (isBouncing && isQTE)
        {
            BounceQTE();
        }
    }

    private void Flip(Vector3 targetPos)
    {
        if (!flipped)
        {
            flipped = true;
            if (!facingRight && transform.position.x < targetPos.x) { transform.Rotate(new Vector3(0, 0, 1), 180f); }
            else if (facingRight && transform.position.x > targetPos.x) { transform.Rotate(new Vector3(0, 0, 1), 180f); }
        }
    }//flip direction of the boomerang

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    private void BounceQTE()
    {
        if (bounceDirection == 0)
        {
            bounceQTEtimer += Time.deltaTime;
            CheckBounceDirection();
            //if not decide over time, cancel.
            if (bounceQTEtimer >= bounceQTEtime)
            {
                SetBool(true);
                player.GetComponent<PlayerAttack>().EndBoomerang();
                Time.timeScale = 1f;
                bounceQTEtimer = 0f;
                _End();
            }
        }
    }//player decide the bounce direction QTE

    public void CheckBounceDirection()
    {
        SetBool(false);
        float y = Gamepad.current.rightStick.value.y;
        rb.velocity = Vector3.zero;
        if (y != 0)
        {
            bounceDirection = y < 0 ? 2 : 1;
            SetBool(true);
            EndQTE();
        }
    }

    public void EndQTE()
    {
        BounceQTEUI.SetActive(false);
        Bounce();
        Time.timeScale = 1f;
        isQTE = false;
        if (CameraManager.IsActiveCamera(cam))
        {
            if (CameraManager.beforeActiveCam == cam | CameraManager.beforeActiveCam == null)
            {
                CameraManager.instance.SwtichToNormalCam();
            }
            else
            {
                CameraManager.SwitchBounceQTECamera(CameraManager.beforeActiveCam);
            }
        }
    }

    public void SetBool(bool i)
    {
        controller.canMove = i;
        controller.canFlip = i;
        controller.canJump = i;
        controller.canDoubleJump = i;
        player.GetComponent<PlayerAttack>().canAttack = i;
        player.GetComponent<PlayerAttack>().canDefend = i;
    }

    private void Bounce()
    {
        //hitPos = Vector2.zero;
        //if (bounceDirection != 0)
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0.6f), Vector2.up, 1.2f, colliderLayers);
        //    hitPos = hit.point;
        //}
        //float x = transform.position.x;
        if (bounceDirection == 1)
        {
            if (facingRight)
            {
                if (lastHitHardwall.tag == "hard_wall_vertical") { transform.eulerAngles = new Vector3(0, 0, 135); facingRight = !facingRight; }
                else if (lastHitHardwall.tag == "hard_wall_horizontal") { transform.eulerAngles = new Vector3(0, 0, -45); bounceDirection = 2; }
            }
            else
            {
                if (lastHitHardwall.tag == "hard_wall_vertical") { transform.eulerAngles = new Vector3(0, 0, 45); facingRight = !facingRight; }
                else if (lastHitHardwall.tag == "hard_wall_horizontal") { transform.eulerAngles = new Vector3(0, 0, 225); bounceDirection = 2; }
            }
        }//bounce up
        else if (bounceDirection == 2)
        {
            if (facingRight)
            {
                if (lastHitHardwall.tag == "hard_wall_vertical") { transform.eulerAngles = new Vector3(0, 0, 225); facingRight = !facingRight; }
                else if (lastHitHardwall.tag == "hard_wall_horizontal") { transform.eulerAngles = new Vector3(0, 0, 45); bounceDirection = 1; }
            }
            else
            {
                if (lastHitHardwall.tag == "hard_wall_vertical") { transform.eulerAngles = new Vector3(0, 0, -45); facingRight = !facingRight; }
                else if (lastHitHardwall.tag == "hard_wall_horizontal") { transform.eulerAngles = new Vector3(0, 0, 135); bounceDirection = 1; }
            }
        }//bounce down
        SoundManager.PlaySound("hard_wall");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y - 0.6f), new Vector2(transform.position.x, transform.position.y - 0.6f) + Vector2.up * 1.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == GameManager.instance.Player && flipped && !stickedInToWall)
        {
            player.GetComponent<PlayerAttack>().EndBoomerang();
            _End();
        }
        else if (collision.gameObject.layer == 10 && !retrieved)//soft wall
        {
            //stick into
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            GetComponent<BoxCollider2D>().isTrigger = false;
            anim.Play("boomerang_stickintoWall");
            SoundManager.PlaySound("soft_wall");
            stickedInToWall = true;
            gameObject.layer = 7;
            if (!facingRight)
            {
                transform.eulerAngles = new Vector3(0, 0, 180);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }//soft wall
        else if (collision.gameObject.layer == 11 && !retrieved)//hard wall
        {
            vfx.SpawnHitEffect(false, transform.position);
            lastHitHardwall = collision.gameObject;
            //qte for first time
            if (!isBouncing)
            { BounceQTEUI.SetActive(true); Time.timeScale = 0.1f; isBouncing = true; isQTE = true; CameraManager.SwitchBounceQTECamera(cam); rb.velocity = Vector3.zero; speedMultiplier = 1f; }
            else
            {
                Bounce();
            }
        }//hard wall
        else if (collision.gameObject.layer == 7 | collision.gameObject.layer == 18)//ground
        {
            vfx.SpawnHitEffect(false, transform.position);
            retrieved = true;
            //retrieve
            player.GetComponent<PlayerAttack>().RetreiveBoomerang();
            _End();
            //anim of retrieve
        }//ground
    }

    public void _End()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        anim.Play("boomerang_spin");
        bounceDirection = 0;
        gameObject.SetActive(false);
        stickedInToWall = false;
        isBouncing = false;
        flipped = false;
        flyingTimer = 0f;
        bounceQTEtimer = -1f;
        EndQTE();
    }
}