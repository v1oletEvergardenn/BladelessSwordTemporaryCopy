using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamHitBox : MonoBehaviour
{
    private BoxCollider2D col;
    private SpriteRenderer spriteRenderer;

    private float damageTimer = 0f;
    public float damageTimeGap = 0.5f;
    public int damageAmount;
    private VFXManager vfx;
    private bool inrange = false;

    // Start is called before the first frame update
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        vfx = VFXManager.instance;
    }

    // Update is called once per frame
    private void Update()
    {
        col.size = spriteRenderer.bounds.size;
        if (inrange) { damageTimer += Time.deltaTime; }
        if (damageTimer >= damageTimeGap)
        {
            DealDamage();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == GameManager.instance.player)
        {
            inrange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == GameManager.instance.player)
        {
            inrange = false;
        }
    }

    public void DealDamage()
    {
        damageTimer = 0f;
        if (!GameManager.instance.player.GetComponent<PlayerAttack>().isDefending)
        {
            GameManager.instance.playerhealth.Damage(damageAmount, null, 0.2f);
        }
        vfx.SpawnSlashEffect(GameManager.instance.playerhealth.GetHitPos());
    }
}