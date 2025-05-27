using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening.Core.Easing;
using DG.Tweening;

/// <summary>
/// Types of hit effects available for spawning.
/// </summary>
public enum Hit_Effect
{
    slash,
    largeSlash,
}

/// <summary>
/// Manages visual and feedback effects such as camera shake, time manipulation, controller rumble, and spawning VFX.
/// </summary>
public class VFXManager : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// Singleton instance of the VFXManager.
    /// </summary>
    public static VFXManager instance;

    #endregion Singleton

    #region Fields

    private GameManager gameManager;
    private bool Camera_isShaking = false;
    private Gamepad gamePad;
    private ObjectPooler objectPooler;
    public Volume breakEffect;
    public Light2D globalLight;
    public Light2D light_player_enemy;

    #endregion Fields

    #region Time

    public static float globalTimeScale = 1f;
    public static bool isInBulletTime = false;

    #endregion Time

    #region Unity Lifecycle

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
        globalTimeScale = 1f;
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        objectPooler = ObjectPooler.instance;
    }

    #endregion Unity Lifecycle

    #region MainMethods

    public void MeleeAttackEffect(MeleeAttack melee, IDamagable target, bool left)
    {
        SpawnHitEffect(true, target.GetHitPos());
        target.Repel(melee.repel, left);
        CameraShake(melee.cameraShake);
        RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
        SlowTimeForSeconds(melee.freezeTime, 0f);
    }

    public void MeleeAttackEffect(MeleeAttack melee, IDamagable target)
    {
        SpawnHitEffect(true, target.GetHitPos());
        CameraShake(melee.cameraShake);
        RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
        SlowTimeForSeconds(melee.freezeTime, 0f);
    }

    #endregion MainMethods

    #region Rumble

    /// <summary>
    /// Triggers a short rumble pulse on the current gamepad.
    /// </summary>
    /// <param name="lowFrequency">Low frequency motor speed (0.0 to 1.0).</param>
    /// <param name="highFrequency">High frequency motor speed (0.0 to 1.0).</param>
    /// <param name="duration">Duration of the rumble in seconds.</param>
    public void RumblePulse(float lowFrequency, float highFrequency, float duration)
    {
        gamePad = Gamepad.current;
        if (gamePad != null)
        {
            //start rumble
            gamePad.SetMotorSpeeds(lowFrequency, highFrequency);
            //stop rumble
            StartCoroutine(StopRumble(duration, gamePad));
        }
    }

    /// <summary>
    /// Starts continuous rumble on the current gamepad.
    /// </summary>
    /// <param name="lowFrequency">Low frequency motor speed (0.0 to 1.0).</param>
    /// <param name="highFrequency">High frequency motor speed (0.0 to 1.0).</param>
    public void Rumble(float lowFrequency, float highFrequency)
    {
        gamePad = Gamepad.current;
        if (gamePad != null)
        {
            //start rumble
            gamePad.SetMotorSpeeds(lowFrequency, highFrequency);
        }
    }

    /// <summary>
    /// Stops all rumble on the current gamepad.
    /// </summary>
    public void StopRumble()
    {
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

    /// <summary>
    /// Coroutine to stop rumble after a specified duration.
    /// </summary>
    private IEnumerator StopRumble(float duration, Gamepad pad)
    {
        yield return new WaitForSecondsRealtime(duration);

        if (CharacterController2D.instance.resetRumbleJump)
        {
            pad.SetMotorSpeeds(CharacterController2D.instance.floatingRumblingSpeed.x, 0);
        }
        else
        {
            pad.SetMotorSpeeds(0, 0);
        }
        yield return null;
    }

    #endregion Rumble

    #region Time Manipulation

    /// <summary>
    /// Temporarily slows down time for a given duration.
    /// </summary>
    /// <param name="duration">Duration of the slow effect in seconds.</param>
    /// <param name="timeSpeed">Time scale to set during the effect.</param>
    public void SlowTimeForSeconds(float duration, float timeSpeed)
    {
        StartCoroutine(IESlowTime(duration, timeSpeed));
    }

    /// <summary>
    /// Coroutine to handle time slow effect.
    /// </summary>
    private IEnumerator IESlowTime(float duration, float timeSpeed)
    {
        Time.timeScale = timeSpeed;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = globalTimeScale;
    }

    /// <summary>
    /// Freezes the game time (sets time scale to 0).
    /// </summary>
    public void FreezeTime()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Unfreezes the game time (sets time scale to 1).
    /// </summary>
    public void UnFreezeTime()
    {
        Time.timeScale = 1f;
    }

    public void BulletTime()
    {
        //stop all
        StartBossBreakEffect();
        Time.timeScale = 0.1f;
        globalTimeScale = 0.1f;
        isInBulletTime = true;
        PlayerAttack.instance.anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        Time.fixedDeltaTime = Time.deltaTime * 0.02f;
    }

    public void UnBulletTime()
    {
        EndBossBreakEffect();
        Time.timeScale = 1f;
        globalTimeScale = 1f;
        isInBulletTime = false;
        PlayerAttack.instance.anim.updateMode = AnimatorUpdateMode.Normal;
        Time.fixedDeltaTime = 0.02f;
    }

    #endregion Time Manipulation

    #region Camera Effects

    /// <summary>
    /// Triggers a camera shake effect with the specified force.
    /// </summary>
    /// <param name="force">Force of the camera shake.</param>
    public void CameraShake(float force)
    {
        if (!Camera_isShaking)
        {
            Camera_isShaking = true;
            gameManager.impulseSource.GenerateImpulseWithForce(force);
            Camera_isShaking = false;
        }
    }

    public static void StartBossBreakEffect()
    {
        instance.StartCoroutine(instance.BossBreakEffectCoroutine(true));
    }

    public static void EndBossBreakEffect()
    {
        instance.StartCoroutine(instance.BossBreakEffectCoroutine(false));
    }

    public IEnumerator BossBreakEffectCoroutine(bool start)
    {
        breakEffect.enabled = true;
        float duration = 0.4f;
        float elapsed = 0f;
        float from_breakValue = start ? 0f : 1f;
        float to_breakValue = start ? 1f : 0f;

        float from_lightIntensity = start ? 1f : 0.5f;
        float to_lightIntensity = start ? 0.5f : 1f;

        float from_lightIntensity2 = start ? 0f : 0.8f;
        float to_lightIntensity2 = start ? 0.8f : 0f;

        Ease easeType = Ease.OutCubic;

        // Set initial value
        breakEffect.weight = from_breakValue;
        globalLight.intensity = from_lightIntensity;
        light_player_enemy.intensity = from_lightIntensity2;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Use DOTween's DOVirtual.EasedValue for easing
            float breakValue = DOVirtual.EasedValue(from_breakValue, to_breakValue, t, easeType);
            float lightIntensity = DOVirtual.EasedValue(from_lightIntensity, to_lightIntensity, t, easeType);
            float lightIntensity2 = DOVirtual.EasedValue(from_lightIntensity2, to_lightIntensity2, t, easeType);
            breakEffect.weight = breakValue;
            light_player_enemy.intensity = lightIntensity2;
            globalLight.intensity = lightIntensity;
            yield return null;
        }

        //elapsed = 0f;
        //while (elapsed < 0.2f)
        //{
        //    elapsed += Time.unscaledDeltaTime;
        //    float t = Mathf.Clamp01(elapsed / 0.2f);

        //    float breakValue2 = DOVirtual.EasedValue(1, 0.8f, t, easeType);
        //    breakEffect.weight = breakValue2;
        //    yield return null;
        //}

        // Optionally disable the effect when finished decreasing
        if (!start)
            breakEffect.enabled = false;
    }

    #endregion Camera Effects

    #region VFX Spawning

    /// <summary>
    /// Spawns a hit effect at the given position.
    /// </summary>
    /// <param name="isPerfect">Whether the hit is perfect or normal.</param>
    /// <param name="position">World position to spawn the effect.</param>
    /// <returns>The spawned HitEffect component.</returns>
    public HitEffect SpawnHitEffect(bool isPerfect, Vector3 position)
    {
        HitEffect i = objectPooler.SpawnFromPool("hit_effect", position, true).GetComponent<HitEffect>();
        if (isPerfect) { i.Perfect(); }
        else { i.Normal(); }
        return i;
    }

    /// <summary>
    /// Spawns a slash effect at the given position.
    /// </summary>
    /// <param name="position">World position to spawn the effect.</param>
    /// <param name="isRed">If true, the effect is red; otherwise, white.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnSlashEffect(Vector3 position, bool isRed = false)
    {
        GameObject i = objectPooler.SpawnFromPool("slash_effect", position, false);
        if (isRed) { i.GetComponent<SpriteRenderer>().color = Color.red; }
        else { i.GetComponent<SpriteRenderer>().color = Color.white; }
        return i;
    }

    /// <summary>
    /// Spawns a large slash effect at the given position.
    /// </summary>
    /// <param name="position">World position to spawn the effect.</param>
    /// <param name="isRed">If true, the effect is red; otherwise, white.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnLargeSlashEffect(Vector3 position, bool isRed = false)
    {
        GameObject i = objectPooler.SpawnFromPool("large_slash_effect", position, false);
        if (isRed) { i.GetComponent<SpriteRenderer>().color = Color.red; }
        else { i.GetComponent<SpriteRenderer>().color = Color.white; }
        return i;
    }

    /// <summary>
    /// Spawns a dash effect at the given position and rotation.
    /// </summary>
    /// <param name="position">World position to spawn the effect.</param>
    /// <param name="rot">Rotation to apply to the effect.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnDashEffect(Vector3 position, Quaternion rot)
    {
        GameObject i = objectPooler.SpawnFromPool("dash_effect", position, false);
        i.transform.rotation = rot;
        return i;
    }

    /// <summary>
    /// Spawns a slash or large slash effect based on the provided enum.
    /// </summary>
    /// <param name="i">Type of hit effect to spawn.</param>
    /// <param name="position">World position to spawn the effect.</param>
    /// <param name="isRed">If true, the effect is red; otherwise, white.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnEffectWithEnum(Hit_Effect i, Vector3 position, bool isRed = false)
    {
        if (i == Hit_Effect.slash)
        {
            return SpawnSlashEffect(position, isRed);
        }
        else if (i == Hit_Effect.largeSlash)
        {
            return SpawnLargeSlashEffect(position, isRed);
        }

        return null;
    }

    private Coroutine co_failedAction;

    public void FailedToDoAction()
    {
        Energy.instance.failedToDoActionSymbol.localPosition = new Vector3(0, 1.9f, 0);
        if (co_failedAction != null) StopCoroutine(co_failedAction);
        co_failedAction = StartCoroutine(IE_FailedToDoAction());
    }

    public IEnumerator IE_FailedToDoAction()
    {
        var symbol = Energy.instance.failedToDoActionSymbol;
        if (symbol == null) yield break;

        // Get or add SpriteRenderer for alpha control
        SpriteRenderer sr = symbol.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        // Reset state

        Color color = sr.color;
        color.a = 0f;
        sr.color = color;
        Vector3 startPos = new Vector3(0, 1.9f, 0);
        symbol.gameObject.SetActive(true);
        Vector3 endPos = startPos + new Vector3(0, 0.5f, 0); // Move up 0.5 units

        // Fade in and move up
        sr.DOFade(1f, 0.2f).SetUpdate(true);
        symbol.DOLocalMove(endPos, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.5f);

        // Fade out
        sr.DOFade(0f, 0.2f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.2f);

        symbol.gameObject.SetActive(false);
        symbol.localPosition = startPos; // Reset position for next time
    }

    #endregion VFX Spawning
}