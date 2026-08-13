using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime debug overlay for channel scales and active modifiers.
/// Toggle with F9.
/// </summary>
public class TimeScaleDebugOverlay : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.F9;
    [SerializeField] private bool visibleOnStart = false;
    [SerializeField] private int fontSize = 14;

    private bool visible;

    private void Awake()
    {
        visible = visibleOnStart;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
        }
    }

    private void OnGUI()
    {
        if (!visible) return;
        if (TimeScaleManager.instance == null) return;

        GUI.skin.label.fontSize = fontSize;

        List<TimeScaleManager.ChannelDebugSnapshot> snapshots = TimeScaleManager.instance.CreateDebugSnapshot();

        GUILayout.BeginArea(new Rect(12f, 12f, 740f, Screen.height - 24f), GUI.skin.box);
        GUILayout.Label("TIME SCALE DEBUG");
        GUILayout.Space(4f);

        for (int i = 0; i < snapshots.Count; i++)
        {
            TimeScaleManager.ChannelDebugSnapshot s = snapshots[i];
            GUILayout.Label(
                s.channel +
                " | base: " + s.baseScale.ToString("0.###") +
                " | local: " + s.localScale.ToString("0.###") +
                " | final: " + s.compositeScale.ToString("0.###") +
                " | modifiers: " + s.modifiers.Count);

            for (int m = 0; m < s.modifiers.Count; m++)
            {
                TimeModifier mod = s.modifiers[m];
                string ttl = mod.IsTimed ? mod.remaining.ToString("0.###") + "s" : "inf";
                GUILayout.Label("   - [" + mod.id + "] " + mod.source + " x" + mod.multiplier.ToString("0.###") + " (ttl: " + ttl + ")");
            }

            GUILayout.Space(4f);
        }

        GUILayout.EndArea();
    }
}