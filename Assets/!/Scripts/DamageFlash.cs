using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [HideInInspector] public Material flashMaterial;
    [ColorUsage(true, true)] public Color _color = Color.white;
    public float flashTime = 0.2f;
    public SpriteRenderer sprite;
    private Material originalMat;
    public AnimationCurve flashCurve = AnimationCurve.Linear(0f, 1f, 0.2f, 0f);

    private void Start()
    {
        originalMat = sprite.material;
        flashMaterial = GameManager.instance.FlashEffectMat;
    }

    public void OnDamageFlash(SpriteRenderer spriteInput = null)
    {
        if (spriteInput != null)
        {
            StartCoroutine(IEDamageFlasher(spriteInput));
        }
        else
        {
            StartCoroutine(IEDamageFlasher(sprite));
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