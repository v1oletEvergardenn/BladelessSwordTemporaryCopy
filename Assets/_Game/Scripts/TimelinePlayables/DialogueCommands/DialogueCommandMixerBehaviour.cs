using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueCommandMixerBehaviour : PlayableBehaviour
{
    private const float ActiveWeightThreshold = 0.001f;
    private const double RewindEpsilon = 0.0001d;

    private readonly HashSet<int> _played = new HashSet<int>();
    private double _lastRootTime = double.NegativeInfinity;
    private double _pendingResumeJumpRootTime = double.NegativeInfinity;
    private bool _resumeJumpArmed;
    private bool _pauseObservedAfterArm;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Playable rootPlayable = playable.GetGraph().GetRootPlayable(0);
        double rootTime = rootPlayable.IsValid() ? rootPlayable.GetTime() : playable.GetTime();

        if (_lastRootTime != double.NegativeInfinity && rootTime + RewindEpsilon < _lastRootTime)
        {
            _played.Clear();
            ClearPendingResumeJump();
        }

        _lastRootTime = rootTime;

        if (_resumeJumpArmed)
        {
            if (info.effectivePlayState == PlayState.Paused)
            {
                _pauseObservedAfterArm = true;
            }
            else if (_pauseObservedAfterArm &&
                     info.effectivePlayState == PlayState.Playing &&
                     _pendingResumeJumpRootTime != double.NegativeInfinity)
            {
                if (rootPlayable.IsValid() && _pendingResumeJumpRootTime > rootTime + RewindEpsilon)
                {
                    rootPlayable.SetTime(_pendingResumeJumpRootTime);
                    _lastRootTime = _pendingResumeJumpRootTime;
                }

                ClearPendingResumeJump();
            }
        }

        GameObject trackBinding = playerData as GameObject;
        Transform speaker = trackBinding != null ? trackBinding.transform : null;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float weight = playable.GetInputWeight(i);
            if (weight > ActiveWeightThreshold && !_played.Contains(i))
            {
                _played.Add(i);

                ScriptPlayable<DialogueCommandBehaviour> inputPlayable = (ScriptPlayable<DialogueCommandBehaviour>)playable.GetInput(i);
                DialogueCommandBehaviour behaviour = inputPlayable.GetBehaviour();

                bool hasPauseCommand = ContainsTimelinePauseCommand(behaviour);

                if (hasPauseCommand)
                {
                    double remainingClipTime = inputPlayable.GetDuration() - inputPlayable.GetTime();
                    double clipEndRootTime = rootTime + remainingClipTime;

                    if (_pendingResumeJumpRootTime == double.NegativeInfinity || clipEndRootTime > _pendingResumeJumpRootTime)
                    {
                        _pendingResumeJumpRootTime = clipEndRootTime;
                    }

                    _resumeJumpArmed = true;
                    _pauseObservedAfterArm = false;
                }

                if (behaviour != null)
                {
                    behaviour.Execute(speaker, inputPlayable.GetDuration());
                }
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);

        if (_resumeJumpArmed)
        {
            _pauseObservedAfterArm = true;
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        base.OnGraphStart(playable);

        // Keep pending jump state across graph restart during pause/resume.
        _lastRootTime = double.NegativeInfinity;
    }

    public override void OnGraphStop(Playable playable)
    {
        base.OnGraphStop(playable);

        if (_resumeJumpArmed && _pendingResumeJumpRootTime != double.NegativeInfinity)
        {
            Playable rootPlayable = playable.GetGraph().GetRootPlayable(0);
            if (rootPlayable.IsValid())
            {
                double rootTime = rootPlayable.GetTime();
                if (_pendingResumeJumpRootTime > rootTime + RewindEpsilon)
                {
                    rootPlayable.SetTime(_pendingResumeJumpRootTime);
                    _lastRootTime = _pendingResumeJumpRootTime;
                }
            }
        }
    }

    private static bool ContainsTimelinePauseCommand(DialogueCommandBehaviour behaviour)
    {
        if (behaviour == null || behaviour.commands == null)
            return false;

        for (int i = 0; i < behaviour.commands.Count; i++)
        {
            DialogueCommandRuntimeItem item = behaviour.commands[i];
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

    private void ClearPendingResumeJump()
    {
        _resumeJumpArmed = false;
        _pauseObservedAfterArm = false;
        _pendingResumeJumpRootTime = double.NegativeInfinity;
    }
}