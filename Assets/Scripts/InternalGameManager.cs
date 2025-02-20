using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InternalGameManager : MonoBehaviour
{
    // Start is called before the first frame update

    public UnityEvent DiedFromQianXiao;
    public UnityEvent ReachedRoom;
    public UnityEvent StartGameEvent;
    public UnityEvent LaunchStoryScene;

    private void Start()
    {
        GameManager.instance.UnpauseGame();
        StartGameEvent?.Invoke();
    }

    public void SwitchMusic(int i)
    {
        SoundManager.SwitchMusic(i);
    }

    public void Update()
    {
    }

    public void ReachedQianxiaoRoom()
    {
        Health.instance.DEATH = DiedFromQianXiao;
        ReachedRoom?.Invoke();
    }

    public void DiefromQianXiao()
    {
        DiedFromQianXiao?.Invoke();
    }

    public void Story_2_0_Event()
    {
        StartCoroutine(IE_Story_2_0_Event());
    }

    public IEnumerator IE_Story_2_0_Event()
    {
        QianXiaoAI.instance.anim.Play("S1_thrust");
        yield return new WaitForSeconds(1.4f);
        InputPlayer.instance.anim.Play("attack0");
        yield return new WaitForSeconds(0.1f);
        VFXManager vfx = VFXManager.instance;
        vfx.SpawnSlashEffect(Health.instance.GetHitPos());
        Health.instance.Repel(20, transform.right);
        vfx.CameraShake(0.2f);
        vfx.RumblePulse(0.3f * 2, 0.4f * 2, 0.1f * 2);
        vfx.SlowTimeForSeconds(0.2f, 0f);
        yield return null;
    }

    public void LaunchStory_Scene()
    {
        StartCoroutine(IE_LauchStoryScene());
    }

    public IEnumerator IE_LauchStoryScene()
    {
        yield return new WaitForSeconds(2f);
        LaunchStoryScene?.Invoke();
        yield return null;
    }
}