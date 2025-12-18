using EditorAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Void = EditorAttributes.Void;

[ExecuteAlways]
public class ParallaxManager : MonoBehaviour
{
    private Camera cam;

    [ButtonField(nameof(RecreateAllLayerCopies), "Clear & Recreate All Layer Copies")] public Void holder;

    public List<layer> layers = new List<layer>();

    [HideInInspector] public Transform bigCopiesHolder; // Holds all layer copy holders

    public void RecreateAllLayerCopies()
    {
        foreach (layer i in layers)
        {
            if (i.copies != null)
            {
                foreach (var copy in i.copies)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        if (copy != null) DestroyImmediate(copy?.gameObject);
                    }
                    else
                        if (copy != null) Destroy(copy?.gameObject);
#else
                Destroy(copy?.gameObject);
#endif
                }
                i.copies.Clear();
            }
            // Destroy layer's holder if it exists
            if (i.copiesHolder != null)
            {
#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(i.copiesHolder.gameObject);
                else
                    Destroy(i.copiesHolder.gameObject);
#else
                Destroy(i.copiesHolder.gameObject);
#endif
                i.copiesHolder = null;
            }
        }
        // Destroy big holder if it exists
        if (bigCopiesHolder != null)
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(bigCopiesHolder.gameObject);
            else
                Destroy(bigCopiesHolder.gameObject);
#else
            Destroy(bigCopiesHolder.gameObject);
#endif
            bigCopiesHolder = null;
        }
        UpdateLayerCopies();
    }

    public void Start()
    {
        cam = Camera.main;
        UpdateLayerCopies();
        foreach (layer i in layers)
        {
            i.sprite = i.trans.GetComponent<SpriteRenderer>();
            i.spriteLength = i.sprite.bounds.size.x;
            i.lastLerpPosition = i.trans.position; // Initialize
        }
    }

    private float GetCameraWidth()
    {
        if (cam == null) return 20f; // fallback
        return cam.orthographicSize * 2f * cam.aspect;
    }

    private List<Transform> GetAllTiles(layer i)
    {
        var all = new List<Transform>();
        all.Add(i.trans);
        if (i.copies != null)
            all.AddRange(i.copies);
        return all;
    }

    public void Update()
    {
        float camX = cam != null ? cam.transform.position.x : 0f;
        float camY = cam != null ? cam.transform.position.y : 0f;
        float camWidth = GetCameraWidth();

        foreach (layer i in layers)
        {
            if (i.sprite == null) continue;

            float scaledSpriteLength = i.sprite.bounds.size.x * i.trans.lossyScale.x;
            float parallaxX = camX * i.movingSpeed;
            float parallaxY = camY * i.movingSpeedY;
            float baseZ = i.trans.position.z;

            var allTiles = GetAllTiles(i);

            for (int t = 0; t < allTiles.Count; t++)
            {
                var tile = allTiles[t];
                if (tile == null) continue;
                int offset = 0;
                if (tile != i.trans)
                {
                    string name = tile.name;
                    if (name.Contains("_copy_right_"))
                        offset = int.Parse(name.Substring(name.LastIndexOf("_") + 1));
                    else if (name.Contains("_copy_left_"))
                        offset = -int.Parse(name.Substring(name.LastIndexOf("_") + 1));
                }
                tile.position = new Vector3(
                    parallaxX + scaledSpriteLength * offset + i.offsetX,
                    parallaxY + i.offsetY,
                    baseZ
                );
            }

            // Only X-axis repeat/snapping
            if (!i.disableSnapping)
            {
                float totalWidth = scaledSpriteLength * (i.copyCount * 2 + 1);

                foreach (var tile in allTiles)
                {
                    if (tile == null) continue;
                    float tileX = tile.position.x;

                    if (tileX - camX > scaledSpriteLength * i.copyCount)
                    {
                        tile.position -= new Vector3(totalWidth, 0, 0);
                    }
                    else if (tileX - camX < -scaledSpriteLength * i.copyCount)
                    {
                        tile.position += new Vector3(totalWidth, 0, 0);
                    }
                }
            }
        }
    }

    private void UpdateLayerCopies()
    {
        // Create or find the big holder if any layer repeats
        bool anyRepeat = false;
        foreach (layer i in layers)
            if (i.repeat) { anyRepeat = true; break; }

        if (anyRepeat)
        {
            if (bigCopiesHolder == null)
            {
                var bigHolderObj = GameObject.Find("ParallaxCopiesHolder") ?? new GameObject("ParallaxCopiesHolder");
                bigCopiesHolder = bigHolderObj.transform;
                bigCopiesHolder.SetParent(transform, false);
                bigCopiesHolder.localPosition = Vector3.zero;
            }
        }
        else
        {
            if (bigCopiesHolder != null)
            {
#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(bigCopiesHolder.gameObject);
                else
                    Destroy(bigCopiesHolder.gameObject);
#else
                Destroy(bigCopiesHolder.gameObject);
#endif
                bigCopiesHolder = null;
            }
        }

        foreach (layer i in layers)
        {
            if (i.trans == null) continue;

            i.sprite = i.trans.GetComponent<SpriteRenderer>();
            if (i.sprite == null) continue;
            i.spriteLength = i.sprite.bounds.size.x;

            // Only create or maintain the holder if repeat is true
            if (i.repeat)
            {
                if (i.copiesHolder == null)
                {
                    var holderObj = GameObject.Find(i.trans.name + "_CopiesHolder") ??
                                    new GameObject(i.trans.name + "_CopiesHolder");
                    i.copiesHolder = holderObj.transform;
                }
                i.copiesHolder.SetParent(bigCopiesHolder, false);
                i.copiesHolder.localPosition = Vector3.zero;

                // Remove old children if any
                for (int c = i.copiesHolder.childCount - 1; c >= 0; c--)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(i.copiesHolder.GetChild(c).gameObject);
                    else
                        Destroy(i.copiesHolder.GetChild(c).gameObject);
#else
                    Destroy(i.copiesHolder.GetChild(c).gameObject);
#endif
                }
            }
            else
            {
                // If repeat is false, destroy the holder if it exists
                if (i.copiesHolder != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(i.copiesHolder.gameObject);
                    else
                        Destroy(i.copiesHolder.gameObject);
#else
                    Destroy(i.copiesHolder.gameObject);
#endif
                    i.copiesHolder = null;
                }
            }

            if (i.copies == null)
                i.copies = new List<Transform>();
            else
                i.copies.Clear();

            int desiredCopies = i.repeat && i.copyCount > 0 ? i.copyCount * 2 : 0;

            // Add new copies if increasing copyCount
            if (i.repeat)
            {
                float scaledSpriteLength = i.sprite.bounds.size.x * i.trans.lossyScale.x;
                for (int c = 1; c <= i.copyCount; c++)
                {
                    if (i.copyCount <= 0) break;

                    // Right copy
                    Transform copy = CreateSpriteOnlyCopy(
                        i.trans,
                        i.copiesHolder,
                        i.trans.position + new Vector3(scaledSpriteLength * c, 0, 0),
                        i.trans.name + "_copy_right_" + c
                    );
                    i.copies.Add(copy);

                    // Left copy
                    Transform leftCopy = CreateSpriteOnlyCopy(
                        i.trans,
                        i.copiesHolder,
                        i.trans.position + new Vector3(-scaledSpriteLength * c, 0, 0),
                        i.trans.name + "_copy_left_" + c
                    );
                    i.copies.Add(leftCopy);
                }
            }
        }
    }

    private Transform CreateSpriteOnlyCopy(Transform original, Transform parent, Vector3 worldPosition, string copyName)
    {
        GameObject newObj = new GameObject(copyName);
        newObj.transform.SetParent(parent, false);
        newObj.transform.position = worldPosition;
        newObj.transform.localRotation = original.localRotation;
        newObj.transform.localScale = original.localScale;

        // Copy SpriteRenderer if present
        var origRenderer = original.GetComponent<SpriteRenderer>();
        if (origRenderer != null)
        {
            var newRenderer = newObj.AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (newRenderer != null)
                    {
                        newRenderer.sprite = origRenderer.sprite;
                        newRenderer.color = origRenderer.color;
                        newRenderer.sortingLayerID = origRenderer.sortingLayerID;
                        newRenderer.sortingOrder = origRenderer.sortingOrder;
                        newRenderer.flipX = origRenderer.flipX;
                        newRenderer.flipY = origRenderer.flipY;
                        newRenderer.material = origRenderer.sharedMaterial;
                        newRenderer.drawMode = origRenderer.drawMode;
                        newRenderer.size = origRenderer.size;
                        newRenderer.tileMode = origRenderer.tileMode;
                    }
                };
            }
            else
#endif
            {
                newRenderer.sprite = origRenderer.sprite;
                newRenderer.color = origRenderer.color;
                newRenderer.sortingLayerID = origRenderer.sortingLayerID;
                newRenderer.sortingOrder = origRenderer.sortingOrder;
                newRenderer.flipX = origRenderer.flipX;
                newRenderer.flipY = origRenderer.flipY;
                newRenderer.material = origRenderer.sharedMaterial;
                newRenderer.drawMode = origRenderer.drawMode;
                newRenderer.size = origRenderer.size;
                newRenderer.tileMode = origRenderer.tileMode;
            }
        }

        // Copy Animator if present
        var origAnimator = original.GetComponent<Animator>();
        if (origAnimator != null)
        {
            var newAnimator = newObj.AddComponent<Animator>();
            newAnimator.runtimeAnimatorController = origAnimator.runtimeAnimatorController;
            newAnimator.avatar = origAnimator.avatar;
            newAnimator.applyRootMotion = origAnimator.applyRootMotion;
            newAnimator.updateMode = origAnimator.updateMode;
            newAnimator.cullingMode = origAnimator.cullingMode;
        }

        // Copy Animation (legacy) if present
        var origAnimation = original.GetComponent<Animation>();
        if (origAnimation != null)
        {
            var newAnimation = newObj.AddComponent<Animation>();
            newAnimation.playAutomatically = origAnimation.playAutomatically;
            newAnimation.animatePhysics = origAnimation.animatePhysics;
            newAnimation.cullingType = origAnimation.cullingType;
            foreach (AnimationState state in origAnimation)
            {
                newAnimation.AddClip(state.clip, state.name);
            }
            if (origAnimation.clip != null)
                newAnimation.clip = origAnimation.clip;
        }

        // Recursively copy children (for tiled or composite objects)
        for (int i = 0; i < original.childCount; i++)
        {
            Transform child = original.GetChild(i);
            Transform childCopy = CreateSpriteOnlyCopy(
                child,
                newObj.transform,
                child.position,
                child.name
            );
            childCopy.localPosition = child.localPosition;
            childCopy.localRotation = child.localRotation;
            childCopy.localScale = child.localScale;
        }

        return newObj.transform;
    }
}

[Serializable]
public class layer
{
    public Transform trans;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public float spriteLength;
    public float offsetX; // X-axis offset for this layer
    public float offsetY; // Y-axis offset for this layer
    [Range(0, 1)] public float movingSpeed;
    [Range(0, 1)] public float movingSpeedY;
    public bool repeat;
    [ShowField(nameof(repeat))][Range(0, 10)] public int copyCount = 1;
    [HideInInspector] public List<Transform> copies;
    [NonSerialized] public Vector3 lastLerpPosition;
    public bool disableSnapping = false;
    [HideInInspector] public Transform copiesHolder;
}