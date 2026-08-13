using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIconGenerator : MonoBehaviour
{
    public MapIconType iconType;
    public bool mapIconEnabled = false;
    public bool CreateOnStart = false;

    public void Start()
    {
        if (CreateOnStart) CreateIcon();
    }

    public void CreateIcon()
    {
        if (mapIconEnabled) return;
        mapIconEnabled = true;
        MapManager.instance.CreateMapIcon(iconType, this.gameObject);
    }
}