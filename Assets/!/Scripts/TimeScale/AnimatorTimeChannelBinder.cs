using UnityEngine;

[DisallowMultipleComponent]
public class AnimatorTimeChannelBinder : MonoBehaviour
{
    private Animator targetAnimator;
    [SerializeField] private TimeChannel channel = TimeChannel.Enemy;
    [SerializeField] private bool useUnscaledUpdateMode = true;

    private void Awake()
    {
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();

        if (targetAnimator == null) return;

        if (useUnscaledUpdateMode)
            targetAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void Update()
    {
        if (targetAnimator == null) return;

        float scale = TimeScaleManager.Delta(channel) / Mathf.Max(Time.unscaledDeltaTime, 0.00001f);
        targetAnimator.speed = Mathf.Max(0f, scale);
    }
}