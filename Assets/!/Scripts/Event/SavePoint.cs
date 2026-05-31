using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : IDamagable
{
    private float hitTimer = 0f;
    private int hitCount = 0;
    public float comboTimeout = 1f; // seconds to wait before triggering combo
    public CinemachineVirtualCamera cam;

    public void Update()
    {
        if (hitCount > 0)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= comboTimeout)
            {
                switch (hitCount)
                {
                    case 1:
                        Save();
                        break;

                    case 2:
                        OpenMenu();
                        break;
                }
                hitCount = 0;
                hitTimer = 0f;
            }
        }
    }

    public override int Damage(float damageAmount, Transform sender = null, float stunDuration = 0f, bool damageFlash = true, float stunValue = 0)
    {
        if (sender.transform != PlayerAttack.instance.transform) return 0;
        hitCount++;
        if (hitCount >= 3)
        {
            GiveTokens();
            hitCount = 0;
            hitTimer = 0f;
        }
        return 1;
    }

    //first hit saves game
    public void Save()
    {
        SaveSystem.Save();
    }

    //second hit opens menu
    public void OpenMenu()
    {
        MenuManager.instance.OpenSavePointCanvas();
        CharacterController2D.instance.RunToPosition(transform.position.x + 1f, null);
        CameraManager.SwitchCamera(cam);
    }

    //third hit gives tokens
    public void GiveTokens()
    {
        print("Give Tokens");
    }
}