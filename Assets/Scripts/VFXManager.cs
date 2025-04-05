using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public enum Hit_Effect
{
    slash,
    largeSlash,
}

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;
    private GameManager gameManager;
    private bool Camera_isShaking = false;
    private Gamepad gamePad;
    private ObjectPooler objectPooler;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        objectPooler = ObjectPooler.instance;
    }

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

    public void Rumble(float lowFrequency, float highFrequency)
    {
        gamePad = Gamepad.current;
        if (gamePad != null)
        {
            //start rumble
            gamePad.SetMotorSpeeds(lowFrequency, highFrequency);
        }
    }

    public void StopRumble()
    {
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

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

    public void SlowTimeForSeconds(float duration, float timeSpeed)
    {
        StartCoroutine(IESlowTime(duration, timeSpeed));
    }

    private IEnumerator IESlowTime(float duration, float timeSpeed)
    {
        Time.timeScale = timeSpeed;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    public void FreezeTime()
    {
        Time.timeScale = 0f;
    }

    public void UnFreezeTime()
    {
        Time.timeScale = 1f;
    }

    public void CameraShake(float force)
    {
        if (!Camera_isShaking)
        {
            Camera_isShaking = true;
            gameManager.impulseSource.GenerateImpulseWithForce(force);
            Camera_isShaking = false;
        }
    }

    public HitEffect SpawnHitEffect(bool isPerfect, Vector3 position)
    {
        HitEffect i = objectPooler.SpawnFromPool("hit_effect", position, true).GetComponent<HitEffect>();
        if (isPerfect) { i.Perfect(); }
        else { i.Normal(); }
        return i;
    }

    public GameObject SpawnSlashEffect(Vector3 position, bool isRed = false)
    {
        GameObject i = objectPooler.SpawnFromPool("slash_effect", position, false);
        if (isRed) { i.GetComponent<SpriteRenderer>().color = Color.red; }
        else { i.GetComponent<SpriteRenderer>().color = Color.white; }
        return i;
    }

    public GameObject SpawnLargeSlashEffect(Vector3 position, bool isRed = false)
    {
        GameObject i = objectPooler.SpawnFromPool("large_slash_effect", position, false);
        if (isRed) { i.GetComponent<SpriteRenderer>().color = Color.red; }
        else { i.GetComponent<SpriteRenderer>().color = Color.white; }
        return i;
    }

    public GameObject SpawnDashEffect(Vector3 position, Quaternion rot)
    {
        GameObject i = objectPooler.SpawnFromPool("dash_effect", position, false);
        i.transform.rotation = rot;
        return i;
    }

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
}