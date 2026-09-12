using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueCommandRuntimeItem
{
    public float delay;
    public bool useClipLength;
    public DialogueTimelineCommandType commandType;

    public string audioClip;
    public DialogueTimelineSubjectMode audioSubjectMode;
    public string audioSubjectName;
    public Transform audioSubjectTransform;
    public bool audioOneShot;

    public DialogueTimelineFadeDirection fadeDirection;
    public float fadeDuration;
    public Color fadeColor;

    public DialogueTimelineAnimatorCommandType animatorCommandType;
    public DialogueTimelineAnimatorSubjectMode animatorSubjectMode;
    public string animatorSubjectName;
    public Transform animatorSubjectTransform;
    public string animatorClipName;
    public string animatorResetTrigger;
    public bool animatorBoolValue;
    public int animatorIntValue;
    public float animatorFloatValue;
    public float animatorFloatDuration;

    public DialogueTimelineSubjectMode setActiveTargetMode;
    public string setActiveTargetName;
    public Transform setActiveTargetTransform;
    public DialogueTimelineToggleMode setActiveValue;

    public string setEnabledComponentName;
    public DialogueTimelineToggleMode setEnabledValue;
    public DialogueTimelineSubjectMode setEnabledSubjectMode;
    public string setEnabledSubjectName;
    public Transform setEnabledSubjectTransform;

    public bool sendMessageUpwards;
    public string sendMessageMethod;
    public string sendMessageArgument;
    public DialogueTimelineSubjectMode sendMessageSubjectMode;
    public string sendMessageSubjectName;
    public Transform sendMessageSubjectTransform;
    public bool sendMessageBroadcast;

    public string variableName;
    public string variableValue;

    public bool continueAll;
    public string continueMode;

    public DialogueTimelineTimelineCommandType timelineCommandType;
    public string timelineName;
}

public class DialogueCommandBehaviour : PlayableBehaviour
{
    public Transform listener;
    public List<DialogueCommandRuntimeItem> commands = new List<DialogueCommandRuntimeItem>();

    public bool ContainsTimelinePauseCommand()
    {
        for (int i = 0; i < commands.Count; i++)
        {
            DialogueCommandRuntimeItem item = commands[i];
            if (item == null)
                continue;

            if (item.commandType == DialogueTimelineCommandType.Timeline &&
                item.timelineCommandType == DialogueTimelineTimelineCommandType.PauseTimeline)
            {
                return true;
            }
        }

        return false;
    }

    public void Execute(Transform speaker, double clipDuration)
    {
        if (!Application.isPlaying || PixelCrushers.DialogueSystem.DialogueManager.instance == null)
            return;

        float duration = (float)clipDuration;
        for (int i = 0; i < commands.Count; i++)
        {
            DialogueCommandRuntimeItem item = commands[i];
            if (item == null)
                continue;

            string sequence = BuildSequence(item, duration);
            if (string.IsNullOrWhiteSpace(sequence))
                continue;

            PixelCrushers.DialogueSystem.DialogueManager.PlaySequence(sequence, speaker, listener);
        }
    }

    private static string BuildSequence(DialogueCommandRuntimeItem item, float clipDuration)
    {
        string command = BuildPrimaryCommand(item, clipDuration, out bool alreadyUsesDuration);
        if (string.IsNullOrWhiteSpace(command))
            return string.Empty;

        string sequence = command;

        if (item.useClipLength && !alreadyUsesDuration && clipDuration > 0f)
        {
            sequence += ";Delay(" + ToInvariant(clipDuration) + ")";
        }

        if (item.delay > 0f)
        {
            sequence = "Delay(" + ToInvariant(item.delay) + ");" + sequence;
        }

        return sequence;
    }

    private static string BuildPrimaryCommand(DialogueCommandRuntimeItem item, float clipDuration, out bool alreadyUsesDuration)
    {
        alreadyUsesDuration = false;

        switch (item.commandType)
        {
            case DialogueTimelineCommandType.Audio:
                {
                    if (string.IsNullOrWhiteSpace(item.audioClip))
                        return string.Empty;

                    string subject = BuildSubjectSpecifier(item.audioSubjectMode, item.audioSubjectName, item.audioSubjectTransform);
                    return item.audioOneShot
                        ? "Audio(" + item.audioClip + "," + subject + ",oneshot)"
                        : "Audio(" + item.audioClip + "," + subject + ")";
                }

            case DialogueTimelineCommandType.Fade:
                {
                    float duration = item.useClipLength ? Mathf.Max(0f, clipDuration) : Mathf.Max(0f, item.fadeDuration);
                    alreadyUsesDuration = true;
                    return "Fade(" + FadeDirectionToToken(item.fadeDirection) + "," + ToInvariant(duration) + "," + ColorToWebHex(item.fadeColor) + ")";
                }

            case DialogueTimelineCommandType.Animator:
                return BuildAnimatorCommand(item, clipDuration, ref alreadyUsesDuration);

            case DialogueTimelineCommandType.SetActive:
                {
                    string target = BuildSubjectSpecifier(item.setActiveTargetMode, item.setActiveTargetName, item.setActiveTargetTransform);
                    return "SetActive(" + target + "," + ToggleToToken(item.setActiveValue) + ")";
                }

            case DialogueTimelineCommandType.SetEnabled:
                {
                    if (string.IsNullOrWhiteSpace(item.setEnabledComponentName))
                        return string.Empty;

                    string subject = BuildSubjectSpecifier(item.setEnabledSubjectMode, item.setEnabledSubjectName, item.setEnabledSubjectTransform);
                    return "SetEnabled(" + item.setEnabledComponentName + "," + ToggleToToken(item.setEnabledValue) + "," + subject + ")";
                }

            case DialogueTimelineCommandType.SendMessage:
                {
                    if (string.IsNullOrWhiteSpace(item.sendMessageMethod))
                        return string.Empty;

                    string subject = BuildSubjectSpecifier(item.sendMessageSubjectMode, item.sendMessageSubjectName, item.sendMessageSubjectTransform);
                    string arg = string.IsNullOrEmpty(item.sendMessageArgument) ? "\"\"" : item.sendMessageArgument;
                    string commandName = item.sendMessageUpwards ? "SendMessageUpwards" : "SendMessage";

                    if (!item.sendMessageUpwards && item.sendMessageBroadcast)
                        return commandName + "(" + item.sendMessageMethod + "," + arg + "," + subject + ",broadcast)";

                    return commandName + "(" + item.sendMessageMethod + "," + arg + "," + subject + ")";
                }

            case DialogueTimelineCommandType.SetVariable:
                {
                    if (string.IsNullOrWhiteSpace(item.variableName))
                        return string.Empty;

                    return "SetVariable(" + item.variableName + "," + item.variableValue + ")";
                }

            case DialogueTimelineCommandType.Continue:
                return item.continueAll ? "Continue(all)" : "Continue()";

            case DialogueTimelineCommandType.SetContinueMode:
                {
                    string mode = string.IsNullOrWhiteSpace(item.continueMode) ? "true" : item.continueMode.Trim();
                    return "SetContinueMode(" + mode + ")";
                }

            case DialogueTimelineCommandType.Timeline:
                return BuildTimelineCommand(item);

            default:
                return string.Empty;
        }
    }

    private static string BuildTimelineCommand(DialogueCommandRuntimeItem item)
    {
        switch (item.timelineCommandType)
        {
            case DialogueTimelineTimelineCommandType.PauseTimeline:
                return "PauseTimeline()";

            case DialogueTimelineTimelineCommandType.ResumeTimeLine:
                return "ResumeTimeLine()";

            case DialogueTimelineTimelineCommandType.PlayTimeLine:
                {
                    if (string.IsNullOrWhiteSpace(item.timelineName))
                        return string.Empty;

                    return "PlayTimeLine(" + item.timelineName.Trim() + ")";
                }

            default:
                return string.Empty;
        }
    }

    private static string BuildAnimatorCommand(DialogueCommandRuntimeItem item, float clipDuration, ref bool alreadyUsesDuration)
    {
        string subject = BuildAnimatorSubjectSpecifier(item.animatorSubjectMode, item.animatorSubjectName, item.animatorSubjectTransform);

        switch (item.animatorCommandType)
        {
            case DialogueTimelineAnimatorCommandType.Play:
                {
                    if (string.IsNullOrWhiteSpace(item.animatorClipName))
                        return string.Empty;

                    return "AnimatorPlay(" + item.animatorClipName + "," + subject + ",0,-1)";
                }

            case DialogueTimelineAnimatorCommandType.Trigger:
                {
                    if (string.IsNullOrWhiteSpace(item.animatorClipName))
                        return string.Empty;

                    if (string.IsNullOrWhiteSpace(item.animatorResetTrigger))
                        return "AnimatorTrigger(" + item.animatorClipName + "," + subject + ")";

                    return "AnimatorTrigger(" + item.animatorClipName + "," + subject + "," + item.animatorResetTrigger + ")";
                }

            case DialogueTimelineAnimatorCommandType.Bool:
                {
                    if (string.IsNullOrWhiteSpace(item.animatorClipName))
                        return string.Empty;

                    return "AnimatorBool(" + item.animatorClipName + "," + (item.animatorBoolValue ? "true" : "false") + "," + subject + ")";
                }

            case DialogueTimelineAnimatorCommandType.Int:
                {
                    if (string.IsNullOrWhiteSpace(item.animatorClipName))
                        return string.Empty;

                    return "AnimatorInt(" + item.animatorClipName + "," + item.animatorIntValue + "," + subject + ")";
                }

            case DialogueTimelineAnimatorCommandType.Float:
                {
                    if (string.IsNullOrWhiteSpace(item.animatorClipName))
                        return string.Empty;

                    float duration = item.useClipLength ? Mathf.Max(0f, clipDuration) : Mathf.Max(0f, item.animatorFloatDuration);
                    alreadyUsesDuration = duration > 0f;
                    return "AnimatorFloat(" + item.animatorClipName + "," + ToInvariant(item.animatorFloatValue) + "," + subject + "," + ToInvariant(duration) + ")";
                }

            default:
                return string.Empty;
        }
    }

    private static string BuildSubjectSpecifier(DialogueTimelineSubjectMode mode, string name, Transform transformRef)
    {
        switch (mode)
        {
            case DialogueTimelineSubjectMode.Speaker: return "speaker";
            case DialogueTimelineSubjectMode.Listener: return "listener";
            case DialogueTimelineSubjectMode.Name: return string.IsNullOrWhiteSpace(name) ? "speaker" : name.Trim();
            case DialogueTimelineSubjectMode.Transform: return transformRef != null ? transformRef.name : "speaker";
            case DialogueTimelineSubjectMode.Everyone: return "everyone";
            case DialogueTimelineSubjectMode.Tag: return string.IsNullOrWhiteSpace(name) ? "speaker" : "tag=" + name.Trim();
            default: return "speaker";
        }
    }

    private static string BuildAnimatorSubjectSpecifier(DialogueTimelineAnimatorSubjectMode mode, string name, Transform transformRef)
    {
        switch (mode)
        {
            case DialogueTimelineAnimatorSubjectMode.Player:
                {
                    PlayerControl player = PlayerControl.instance;
                    if (player == null)
                        player = Object.FindObjectOfType<PlayerControl>();
                    return player != null ? player.transform.name : "speaker";
                }

            case DialogueTimelineAnimatorSubjectMode.Speaker: return "speaker";
            case DialogueTimelineAnimatorSubjectMode.Listener: return "listener";
            case DialogueTimelineAnimatorSubjectMode.Name: return string.IsNullOrWhiteSpace(name) ? "speaker" : name.Trim();
            case DialogueTimelineAnimatorSubjectMode.Transform: return transformRef != null ? transformRef.name : "speaker";
            case DialogueTimelineAnimatorSubjectMode.Everyone: return "everyone";
            case DialogueTimelineAnimatorSubjectMode.Tag: return string.IsNullOrWhiteSpace(name) ? "speaker" : "tag=" + name.Trim();
            default: return "speaker";
        }
    }

    private static string FadeDirectionToToken(DialogueTimelineFadeDirection direction)
    {
        switch (direction)
        {
            case DialogueTimelineFadeDirection.In: return "in";
            case DialogueTimelineFadeDirection.Out: return "out";
            case DialogueTimelineFadeDirection.Stay: return "stay";
            case DialogueTimelineFadeDirection.Unstay: return "unstay";
            default: return "out";
        }
    }

    private static string ToggleToToken(DialogueTimelineToggleMode mode)
    {
        switch (mode)
        {
            case DialogueTimelineToggleMode.True: return "true";
            case DialogueTimelineToggleMode.False: return "false";
            case DialogueTimelineToggleMode.Flip: return "flip";
            default: return "true";
        }
    }

    private static string ToInvariant(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string ColorToWebHex(Color color)
    {
        Color32 c = color;
        return "#" + c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");
    }
}