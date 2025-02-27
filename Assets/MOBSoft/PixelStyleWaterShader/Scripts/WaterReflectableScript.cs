using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mobsoft.PixelStyleWaterShader
{
    public class WaterReflectableScript : MonoBehaviour
    {
        public Vector3 localPosition = new Vector3(0, -0.25f, 0);
        public Vector3 localRotation = new Vector3(0, 0, -180);
        public Sprite sprite;
        public string spriteLayer = "Default";
        public int spriteLayerOrder = -5;
        public Material spriteMaterial;
        public SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        private SpriteRenderer spriteSource;
        private SpriteRenderer spriteRenderer;


        void Awake()
        {
            GameObject reflectGo = new GameObject("Water Reflect");
            reflectGo.transform.parent = transform;
            reflectGo.transform.localPosition = localPosition;
            reflectGo.transform.localRotation = Quaternion.Euler(localRotation);
            reflectGo.transform.localScale = new Vector3(reflectGo.transform.localScale.x, reflectGo.transform.localScale.y, reflectGo.transform.localScale.z);

            spriteRenderer = reflectGo.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = spriteLayer;
            spriteRenderer.sortingOrder = spriteLayerOrder;
            spriteRenderer.material = spriteMaterial; // Assign the material
            spriteRenderer.maskInteraction = maskInteraction;

            spriteSource = GetComponent<SpriteRenderer>();
            reflectGo.transform.localScale = Vector3.one;
        }

        void OnDestroy()
        {
            if (spriteRenderer != null)
            {
                Destroy(spriteRenderer.gameObject);
            }
        }

        void LateUpdate()
        {
            if (spriteSource != null)
            {
                if (sprite == null)
                {
                    spriteRenderer.sprite = spriteSource.sprite;
                }
                else
                {
                    spriteRenderer.sprite = sprite;
                }
                spriteRenderer.flipX = spriteSource.flipX;
                spriteRenderer.flipY = spriteSource.flipY;
                spriteRenderer.color = spriteSource.color;
            }

            // Check if material is assigned and update the spriteRenderer's material
            if (spriteRenderer != null && spriteRenderer.material != spriteMaterial)
            {
                spriteRenderer.material = spriteMaterial;
            }

            // Update mask interaction property
            if (spriteRenderer != null && spriteRenderer.maskInteraction != maskInteraction)
            {
                spriteRenderer.maskInteraction = maskInteraction;
            }
        }
    }
}