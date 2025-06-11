using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [HideInInspector] public Material flashMaterial;
    [ColorUsage(true, true)] public Color _color = Color.white;
    public float flashTime = 0.2f;
    public SpriteRenderer sprite;
    private Material originalMat;
    public AnimationCurve flashCurve = AnimationCurve.Linear(0f, 1f, 0.2f, 0f);
    private Coroutine co_damageFlash;

    private void Start()
    {
        originalMat = sprite.material;
        flashMaterial = GameManager.instance.FlashEffectMat;
    }

    public void OnDamageFlash(SpriteRenderer spriteInput = null)
    {
        if (co_damageFlash != null) { StopCoroutine(co_damageFlash); }
        if (spriteInput != null)
        {
            co_damageFlash = StartCoroutine(IEDamageFlasher(spriteInput));
        }
        else
        {
            co_damageFlash = StartCoroutine(IEDamageFlasher(sprite));
        }
    }

    private IEnumerator IEDamageFlasher(SpriteRenderer sprite)
    {
        sprite.material = flashMaterial;
        sprite.material.SetColor("_FlashColor", _color);

        float currentFlashAmount = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = Mathf.Lerp(1f, flashCurve.Evaluate(elapsedTime), elapsedTime / flashTime);
            sprite.material.SetFloat("_FlashAmount", currentFlashAmount);
            yield return null;
        }
        sprite.material = originalMat;
    }
}