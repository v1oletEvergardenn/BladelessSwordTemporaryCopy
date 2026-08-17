// Recompile at 17/08/2026 13:10:24
#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class SetQuestStateClip : PlayableAsset, ITimelineClipAsset
    {
        public SetQuestStateBehaviour template = new SetQuestStateBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SetQuestStateBehaviour>.Create(graph, template);
            //SetQuestStateBehaviour clone = playable.GetBehaviour();
            return playable;
        }
    }
}
#endif
#endif
