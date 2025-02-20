using System.Collections.Generic;
using UnityEngine;

namespace KabreetGames.ParallaxSystem
{
    [DefaultExecutionOrder(1000), DisallowMultipleComponent]
    public sealed class ParallaxManager : MonoBehaviour
    {
        public Vector2 xyOffset = Vector2.zero;
        public int backgroundLayer;
        public int backgroundSortLayer;
        public int midgroundLayer;
        public int midgroundSortLayer;
        public int foregroundLayer;
        public int foregroundSortLayer;
        public List<Plane> planes = new();
        public GameObject planesParent;
        public Camera cameraObject;
        public bool sorted;
        private Vector3 lastCameraPos;
        private Quaternion lastCameraRotation;
        public GameObject planesHolder;
        public Vector3 cameraStartPos;
        public Quaternion cameraStartRotation;

        private void Awake()
        {
            cameraObject = cameraObject != null ? cameraObject : Camera.main;
        }

        private void LateUpdate()
        {
            if (planes.Count == 0 || !cameraObject) return;

            var cameraTransform = cameraObject.transform;
            var cameraPos = cameraTransform.TransformPoint(0, 0, cameraObject.farClipPlane);
            var cameraRotation = cameraTransform.rotation;

            if (cameraPos == lastCameraPos && cameraRotation == lastCameraRotation) return;

            var cameraMovement = cameraPos - cameraStartPos;
            var relativeRotation = Quaternion.Inverse(cameraStartRotation) * cameraRotation;
            var zDistanceFactor = cameraPos.z / cameraStartPos.z;

            foreach (var plane in planes)
            {
                UpdatePosition(cameraMovement, relativeRotation, zDistanceFactor, plane);
                UpdateRotation(cameraRotation, plane);
                HandleRepeat(cameraMovement, plane);
            }

            lastCameraPos = cameraPos;
            lastCameraRotation = cameraRotation;
        }

        private void HandleRepeat(Vector3 cameraMovement, Plane plane)
        {
            if (plane.doNotRepeat || Mathf.Approximately(plane.parallaxFactor, 1)) return;

            if (plane.repeatY)
            {
                var temp = cameraMovement.y * (1 - plane.parallaxFactor);
                var offsetY = plane.startPos.y - cameraStartPos.y;
                if (temp > offsetY + plane.length) plane.startPos.y += plane.length;
                else if (temp < offsetY - plane.length) plane.startPos.y -= plane.length;
            }
            else
            {
                var temp = cameraMovement.x * (1 - plane.parallaxFactor);
                var offsetX = plane.startPos.x - cameraStartPos.x;
                if (temp > offsetX + plane.length) plane.startPos.x += plane.length;
                else if (temp < offsetX - plane.length) plane.startPos.x -= plane.length;
            }
        }

        private void UpdatePosition(Vector3 cameraMovement, Quaternion relativeRotation, float zDistanceFactor,
            Plane plane)
        {
            var distanceThatCameraMovedWithParallax = cameraMovement * plane.parallaxFactor;
            var rotatedParallaxOffset = relativeRotation * distanceThatCameraMovedWithParallax;

            var newPos = new Vector3(
                plane.startPos.x + rotatedParallaxOffset.x - xyOffset.x,
                plane.isStaticY
                    ? cameraMovement.y + cameraStartPos.y
                    : plane.startPos.y + rotatedParallaxOffset.y - xyOffset.y,
                plane.startPos.z * zDistanceFactor
            );

            plane.parent.transform.position = newPos;
        }

        private static void UpdateRotation(Quaternion cameraRotation, Plane plane)
        {
            plane.supPlane0.transform.rotation = cameraRotation;
            if (!plane.supPlane1) return;
            plane.supPlane1.transform.rotation = cameraRotation;
            plane.supPlane2.transform.rotation = cameraRotation;
        }
        

#if UNITY_EDITOR
        
        [ContextMenu("Rate Package",false, int.MaxValue)]
        private void RatePackage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/214502");
        }
        private void OnDrawGizmos()
        {
            if (cameraObject == null) return;
            var cameraPos = cameraObject.transform.TransformPoint(0, 0, cameraObject.farClipPlane);
            UnityEditor.Handles.DrawDottedLine(cameraPos, cameraStartPos, 1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(cameraStartPos, Vector3.one * 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(cameraPos, Vector3.one * 0.2f);
        }
#endif
    }
}