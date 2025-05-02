using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace KabreetGames.ParallaxSystem
{
    [Serializable]
    public sealed class Plane
    {
        [Tooltip("Set this to true if this the Plane the player moves on")]
        public bool isMain;
        
        [Tooltip("The originalPlane Game Object")] 
        public GameObject originalPlane;
        
        [Tooltip("How far the Plane form the camera based on camera clip planes (0 is near, 1 is far).")]
        public float distanceFromCamera = 1f;
        
        [Tooltip("Set this to true if you want this Plane to keep it's X Y position and not move to (offset x, offset y, distance)")]
        public bool saveXyPos;

        [Tooltip("Set this to true if this Plane will not repeat on screen")]
        public bool doNotRepeat;

       [Tooltip("Set this to true the distance between the repeated layers Will be the same as plane 0 if false the layers will be stacked next to each other use it's own local bound width")]
        public bool useBackgroundLengthToRepeat;

        [Tooltip("Set this to true if this Plane will Parallax only on X axis")]
        public bool isStaticY;
        
        [Tooltip("Repeat vertical")]
        public bool repeatY;
        
        [HideInInspector] public bool showMore;
        [HideInInspector] public float length;
        [HideInInspector] public Transform parent;
        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public float distanceFromMainPlane;
        [HideInInspector] public float parallaxFactor;
        [HideInInspector] public GameObject supPlane0;
        [HideInInspector] public GameObject supPlane1;
        [HideInInspector] public GameObject supPlane2;
        
    }
}