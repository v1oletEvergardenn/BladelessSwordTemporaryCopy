using System;
using UnityEngine.Timeline;

[Serializable]
public class BpmBarMarker : Marker
{
    [UnityEngine.SerializeField] private int barIndex;

    public int barIndexValue => barIndex;

#if UNITY_EDITOR

    public void SetData(int bar)
    {
        barIndex = bar;
    }

#endif
}