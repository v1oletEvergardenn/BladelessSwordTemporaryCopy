using System;
using UnityEngine.Timeline;

[Serializable]
public class BpmBeatMarker : Marker
{
    [UnityEngine.SerializeField] private int beatIndex;
    [UnityEngine.SerializeField] private int subdivisionIndex;
    [UnityEngine.SerializeField] private int barIndex;
    [UnityEngine.SerializeField] private int beatInBar;
    [UnityEngine.SerializeField] private bool isBarStart;

    public int beatIndexValue => beatIndex;
    public int subdivisionIndexValue => subdivisionIndex;
    public int barIndexValue => barIndex;
    public int beatInBarValue => beatInBar;
    public bool isBarStartValue => isBarStart;

#if UNITY_EDITOR

    public void SetData(int beat, int subdivision, int bar, int beatInsideBar, bool barStart)
    {
        beatIndex = beat;
        subdivisionIndex = subdivision;
        barIndex = bar;
        beatInBar = beatInsideBar;
        isBarStart = barStart;
    }

#endif
}