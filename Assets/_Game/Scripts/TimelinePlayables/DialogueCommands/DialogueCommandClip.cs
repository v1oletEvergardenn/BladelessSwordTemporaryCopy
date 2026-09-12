using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum DialogueTimelineCommandType
{
    Audio,
    Fade,
    Animator,
    SetActive,
    SetEnabled,
    SendMessage,
    SetVariable,
    Continue,
    SetContinueMode,
    Timeline
}

public enum DialogueTimelineSubjectMode
{
    Speaker,
    Listener,
    Name,
    Transform,
    Everyone,
    Tag
}

public enum DialogueTimelineAnimatorSubjectMode
{
    Player,
    Speaker,
    Listener,
    Name,
    Transform,
    Everyone,
    Tag
}

public enum DialogueTimelineToggleMode
{
    True,
    False,
    Flip
}

public enum DialogueTimelineAnimatorCommandType
{
    Play,
    Trigger,
    Bool,
    Int,
    Float
}

public enum DialogueTimelineFadeDirection
{
    In,
    Out,
    Stay,
    Unstay
}

public enum DialogueTimelineTimelineCommandType
{
    PauseTimeline,
    ResumeTimeLine,
    PlayTimeLine
}

[Serializable]
public class DialogueCommandItem
{
    [HorizontalGroup("Top"), LabelWidth(100)]
    [Min(0f)]
    [Tooltip("Delay from clip start before this command executes.")]
    public float delay;

    [HorizontalGroup("Top"), LabelWidth(120)]
    [Tooltip("Use clip duration where supported. Otherwise appends Delay(clipLength).")]
    public bool useClipLength = true;

    [LabelWidth(120)]
    public DialogueTimelineCommandType commandType = DialogueTimelineCommandType.Audio;

    [BoxGroup("Audio"), ShowIf(nameof(ShowAudio))]
    public string audioClip;

    [BoxGroup("Audio"), ShowIf(nameof(ShowAudio))]
    public DialogueTimelineSubjectMode audioSubjectMode = DialogueTimelineSubjectMode.Speaker;

    [BoxGroup("Audio"), ShowIf(nameof(ShowAudioSubjectNameField))]
    public string audioSubjectName;

    [BoxGroup("Audio"), ShowIf(nameof(ShowAudioSubjectTransformField))]
    public ExposedReference<Transform> audioSubjectTransform;

    [BoxGroup("Audio"), ShowIf(nameof(ShowAudio))]
    public bool audioOneShot;

    [BoxGroup("Fade"), ShowIf(nameof(ShowFade))]
    public DialogueTimelineFadeDirection fadeDirection = DialogueTimelineFadeDirection.Out;

    [BoxGroup("Fade"), ShowIf(nameof(ShowFadeDurationField)), Min(0f)]
    public float fadeDuration = 0.5f;

    [BoxGroup("Fade"), ShowIf(nameof(ShowFade))]
    public Color fadeColor = Color.black;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimator))]
    public DialogueTimelineAnimatorCommandType animatorCommandType = DialogueTimelineAnimatorCommandType.Play;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimator))]
    public DialogueTimelineAnimatorSubjectMode animatorSubjectMode = DialogueTimelineAnimatorSubjectMode.Player;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorSubjectNameField))]
    public string animatorSubjectName;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorSubjectTransformField))]
    public ExposedReference<Transform> animatorSubjectTransform;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorClipNameField))]
    [LabelText("Clip Name")]
    public string animatorClipName;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorResetTriggerField))]
    public string animatorResetTrigger;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorBoolField))]
    public bool animatorBoolValue = true;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorIntField))]
    public int animatorIntValue = 1;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorFloatValueField))]
    public float animatorFloatValue = 1f;

    [BoxGroup("Animator"), ShowIf(nameof(ShowAnimatorFloatDurationField)), Min(0f)]
    public float animatorFloatDuration;

    [BoxGroup("SetActive"), ShowIf(nameof(ShowSetActive))]
    public DialogueTimelineSubjectMode setActiveTargetMode = DialogueTimelineSubjectMode.Name;

    [BoxGroup("SetActive"), ShowIf(nameof(ShowSetActiveTargetNameField))]
    public string setActiveTargetName;

    [BoxGroup("SetActive"), ShowIf(nameof(ShowSetActiveTargetTransformField))]
    public ExposedReference<Transform> setActiveTargetTransform;

    [BoxGroup("SetActive"), ShowIf(nameof(ShowSetActive))]
    public DialogueTimelineToggleMode setActiveValue = DialogueTimelineToggleMode.True;

    [BoxGroup("SetEnabled"), ShowIf(nameof(ShowSetEnabled))]
    public string setEnabledComponentName;

    [BoxGroup("SetEnabled"), ShowIf(nameof(ShowSetEnabled))]
    public DialogueTimelineToggleMode setEnabledValue = DialogueTimelineToggleMode.True;

    [BoxGroup("SetEnabled"), ShowIf(nameof(ShowSetEnabled))]
    public DialogueTimelineSubjectMode setEnabledSubjectMode = DialogueTimelineSubjectMode.Speaker;

    [BoxGroup("SetEnabled"), ShowIf(nameof(ShowSetEnabledSubjectNameField))]
    public string setEnabledSubjectName;

    [BoxGroup("SetEnabled"), ShowIf(nameof(ShowSetEnabledSubjectTransformField))]
    public ExposedReference<Transform> setEnabledSubjectTransform;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessage))]
    public bool sendMessageUpwards;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessage))]
    public string sendMessageMethod;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessage))]
    public string sendMessageArgument;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessage))]
    public DialogueTimelineSubjectMode sendMessageSubjectMode = DialogueTimelineSubjectMode.Speaker;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessageSubjectNameField))]
    public string sendMessageSubjectName;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessageSubjectTransformField))]
    public ExposedReference<Transform> sendMessageSubjectTransform;

    [BoxGroup("SendMessage"), ShowIf(nameof(ShowSendMessageBroadcastField))]
    public bool sendMessageBroadcast;

    [BoxGroup("SetVariable"), ShowIf(nameof(ShowSetVariable))]
    public string variableName;

    [BoxGroup("SetVariable"), ShowIf(nameof(ShowSetVariable))]
    public string variableValue = "true";

    [BoxGroup("Continue"), ShowIf(nameof(ShowContinue))]
    public bool continueAll;

    [BoxGroup("SetContinueMode"), ShowIf(nameof(ShowSetContinueMode))]
    [Tooltip("Examples: true, false, optional, always, never, original")]
    public string continueMode = "true";

    [BoxGroup("Timeline"), ShowIf(nameof(ShowTimeline))]
    public DialogueTimelineTimelineCommandType timelineCommandType = DialogueTimelineTimelineCommandType.PlayTimeLine;

    [BoxGroup("Timeline"), ShowIf(nameof(ShowTimelineNameField))]
    [LabelText("Timeline Name")]
    public string timelineName;

    public DialogueCommandRuntimeItem Resolve(IExposedPropertyTable resolver)
    {
        return new DialogueCommandRuntimeItem
        {
            delay = delay,
            useClipLength = useClipLength,
            commandType = commandType,

            audioClip = audioClip,
            audioSubjectMode = audioSubjectMode,
            audioSubjectName = audioSubjectName,
            audioSubjectTransform = audioSubjectTransform.Resolve(resolver),
            audioOneShot = audioOneShot,

            fadeDirection = fadeDirection,
            fadeDuration = fadeDuration,
            fadeColor = fadeColor,

            animatorCommandType = animatorCommandType,
            animatorSubjectMode = animatorSubjectMode,
            animatorSubjectName = animatorSubjectName,
            animatorSubjectTransform = animatorSubjectTransform.Resolve(resolver),
            animatorClipName = animatorClipName,
            animatorResetTrigger = animatorResetTrigger,
            animatorBoolValue = animatorBoolValue,
            animatorIntValue = animatorIntValue,
            animatorFloatValue = animatorFloatValue,
            animatorFloatDuration = animatorFloatDuration,

            setActiveTargetMode = setActiveTargetMode,
            setActiveTargetName = setActiveTargetName,
            setActiveTargetTransform = setActiveTargetTransform.Resolve(resolver),
            setActiveValue = setActiveValue,

            setEnabledComponentName = setEnabledComponentName,
            setEnabledValue = setEnabledValue,
            setEnabledSubjectMode = setEnabledSubjectMode,
            setEnabledSubjectName = setEnabledSubjectName,
            setEnabledSubjectTransform = setEnabledSubjectTransform.Resolve(resolver),

            sendMessageUpwards = sendMessageUpwards,
            sendMessageMethod = sendMessageMethod,
            sendMessageArgument = sendMessageArgument,
            sendMessageSubjectMode = sendMessageSubjectMode,
            sendMessageSubjectName = sendMessageSubjectName,
            sendMessageSubjectTransform = sendMessageSubjectTransform.Resolve(resolver),
            sendMessageBroadcast = sendMessageBroadcast,

            variableName = variableName,
            variableValue = variableValue,

            continueAll = continueAll,
            continueMode = continueMode,

            timelineCommandType = timelineCommandType,
            timelineName = timelineName
        };
    }

    private bool ShowAudio => commandType == DialogueTimelineCommandType.Audio;
    private bool ShowFade => commandType == DialogueTimelineCommandType.Fade;
    private bool ShowAnimator => commandType == DialogueTimelineCommandType.Animator;
    private bool ShowSetActive => commandType == DialogueTimelineCommandType.SetActive;
    private bool ShowSetEnabled => commandType == DialogueTimelineCommandType.SetEnabled;
    private bool ShowSendMessage => commandType == DialogueTimelineCommandType.SendMessage;
    private bool ShowSetVariable => commandType == DialogueTimelineCommandType.SetVariable;
    private bool ShowContinue => commandType == DialogueTimelineCommandType.Continue;
    private bool ShowSetContinueMode => commandType == DialogueTimelineCommandType.SetContinueMode;
    private bool ShowTimeline => commandType == DialogueTimelineCommandType.Timeline;

    private bool ShowFadeDurationField => ShowFade && !useClipLength;

    private bool ShowAudioSubjectNameField => ShowAudio && (audioSubjectMode == DialogueTimelineSubjectMode.Name || audioSubjectMode == DialogueTimelineSubjectMode.Tag);
    private bool ShowAudioSubjectTransformField => ShowAudio && audioSubjectMode == DialogueTimelineSubjectMode.Transform;

    private bool ShowAnimatorSubjectNameField => ShowAnimator && (animatorSubjectMode == DialogueTimelineAnimatorSubjectMode.Name || animatorSubjectMode == DialogueTimelineAnimatorSubjectMode.Tag);
    private bool ShowAnimatorSubjectTransformField => ShowAnimator && animatorSubjectMode == DialogueTimelineAnimatorSubjectMode.Transform;
    private bool ShowAnimatorClipNameField => ShowAnimator;
    private bool ShowAnimatorResetTriggerField => ShowAnimator && animatorCommandType == DialogueTimelineAnimatorCommandType.Trigger;
    private bool ShowAnimatorBoolField => ShowAnimator && animatorCommandType == DialogueTimelineAnimatorCommandType.Bool;
    private bool ShowAnimatorIntField => ShowAnimator && animatorCommandType == DialogueTimelineAnimatorCommandType.Int;
    private bool ShowAnimatorFloatValueField => ShowAnimator && animatorCommandType == DialogueTimelineAnimatorCommandType.Float;
    private bool ShowAnimatorFloatDurationField => ShowAnimator && animatorCommandType == DialogueTimelineAnimatorCommandType.Float && !useClipLength;

    private bool ShowSetActiveTargetNameField => ShowSetActive && (setActiveTargetMode == DialogueTimelineSubjectMode.Name || setActiveTargetMode == DialogueTimelineSubjectMode.Tag);
    private bool ShowSetActiveTargetTransformField => ShowSetActive && setActiveTargetMode == DialogueTimelineSubjectMode.Transform;

    private bool ShowSetEnabledSubjectNameField => ShowSetEnabled && (setEnabledSubjectMode == DialogueTimelineSubjectMode.Name || setEnabledSubjectMode == DialogueTimelineSubjectMode.Tag);
    private bool ShowSetEnabledSubjectTransformField => ShowSetEnabled && setEnabledSubjectMode == DialogueTimelineSubjectMode.Transform;

    private bool ShowSendMessageSubjectNameField => ShowSendMessage && (sendMessageSubjectMode == DialogueTimelineSubjectMode.Name || sendMessageSubjectMode == DialogueTimelineSubjectMode.Tag);
    private bool ShowSendMessageSubjectTransformField => ShowSendMessage && sendMessageSubjectMode == DialogueTimelineSubjectMode.Transform;
    private bool ShowSendMessageBroadcastField => ShowSendMessage && !sendMessageUpwards;

    private bool ShowTimelineNameField => ShowTimeline && timelineCommandType == DialogueTimelineTimelineCommandType.PlayTimeLine;
}

[Serializable]
public class DialogueCommandClip : PlayableAsset, ITimelineClipAsset
{
    [Tooltip("Optional listener override used for speaker/listener subject resolution in sequence commands.")]
    public ExposedReference<Transform> listener;

    [ListDrawerSettings(Expanded = true, DraggableItems = true, ShowPaging = false, NumberOfItemsPerPage = 20)]
    public List<DialogueCommandItem> commands = new List<DialogueCommandItem> { new DialogueCommandItem() };

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<DialogueCommandBehaviour> playable = ScriptPlayable<DialogueCommandBehaviour>.Create(graph);
        DialogueCommandBehaviour behaviour = playable.GetBehaviour();

        behaviour.listener = listener.Resolve(graph.GetResolver());
        behaviour.commands.Clear();

        for (int i = 0; i < commands.Count; i++)
        {
            DialogueCommandItem item = commands[i];
            if (item == null)
                continue;

            behaviour.commands.Add(item.Resolve(graph.GetResolver()));
        }

        return playable;
    }
}