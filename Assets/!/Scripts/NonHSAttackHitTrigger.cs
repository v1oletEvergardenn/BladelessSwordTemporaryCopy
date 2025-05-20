using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NonHSAttackHitTrigger : MonoBehaviour
{
    public bool hitEffect = true;
    public Hit_Effect effectType;
    public bool isRed = false;
    private VFXManager vfx;
    [Header("effects")][SerializeField] public float rumbleDuration = 0.1f;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float freezeTimeDuration = 0.2f;
    public UnityEvent onHit;

    private void Start()
    {
        vfx = VFXManager.instance;
    }

    public void Trigger()
    {
        if (hitEffect)
        {
            vfx.SpawnEffectWithEnum(effectType, transform.position, isRed);
            vfx.CameraShake(0.2f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        onHit?.Invoke();
    }
}