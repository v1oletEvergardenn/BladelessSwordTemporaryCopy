using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.Universal;

namespace Water2D
{

    public enum LayerRendererType
    {
        spriteRendererSingle,
        spriteRendererMultiple,
        screenSingle,
        screenMultiple
    }

    [Serializable]
    public class LayerRenderer
    {
        [HideInInspector] [SerializeField] protected ModernWater2D water2D;
        [HideInInspector] [SerializeField] protected RenderTexture layerTexture;
        [HideInInspector] [SerializeField] protected LayerRendererType rendererType;
        [HideInInspector] [SerializeField] protected SpriteRenderer sr;
        [HideInInspector] [SerializeField] protected RenderTextureFormat format = RenderTextureFormat.ARGB32;
        [HideInInspector] [SerializeField] protected FilterMode fliterMode = FilterMode.Point;
        [HideInInspector] [SerializeField] protected float bitDepth = 0;
        [HideInInspector] [SerializeField] protected string layerName = "nl";
        [HideInInspector] [SerializeField] protected int layerMask = -1;
        [HideInInspector] [SerializeField] protected Camera mainCamera;

        [HideInInspector] [SerializeField] protected Transform holder;

        [HideInInspector] [SerializeField] [Range(0f, 1f)] protected float res;
        [HideInInspector] [SerializeField] [Range(1f, 2f)] protected Vector2 scale = new Vector2(1f, 1f);
        [HideInInspector] [SerializeField] int reflectionLayerIdx = -1;

        private bool _run;
        private bool _runDirty;
        [HideInInspector] [SerializeField] internal bool copyMainBackground = false;
        [HideInInspector] [SerializeField] float lastOrtographicSize = 0f;
        [HideInInspector] [SerializeField] float lastAspectRatio = 0f;

        private bool isSetupPending = false;
        private bool isSetupComplete = false;
        private Action pendingSetupAction;

        Camera mCamera => Camera.main;
        Camera CameraRenderingScene => Camera.main;
        Transform follow => Camera.main != null ? Camera.main.transform : null;

        // Only stores the desired state. Camera is enabled/disabled in Loop() or ApplyRunState()
        // so it happens once per frame after all water sources have voted via WaterFeatureLayerRenderer.
        public bool run
        {
            get { return _run; }
            set { _run = value; _runDirty = true; }
        }

        void ApplyRunState()
        {
            if (!_runDirty) return;
            _runDirty = false;

            if (mainCamera == null) return;

            if (_run)
            {
                mainCamera.aspect = lastAspectRatio;
                mainCamera.orthographicSize = lastOrtographicSize;
                mainCamera.enabled = true;
            }
            else if (CameraRenderingScene != null)
            {
                lastAspectRatio = CameraRenderingScene.aspect * (scale.x / scale.y);
                lastOrtographicSize = CameraRenderingScene.orthographicSize * scale.y;
                mainCamera.orthographicSize = 0;
                mainCamera.enabled = false;
            }
            else
            {
                mainCamera.orthographicSize = 0;
                mainCamera.enabled = false;
            }
        }

        public RenderTexture LayerTexture() { return layerTexture; }

        private IEnumerator WaitForMainCameraAndExecute(Action setupAction)
        {
            while (Camera.main == null)
                yield return null;

            yield return new WaitForEndOfFrame();

            setupAction?.Invoke();
            isSetupComplete = true;
            isSetupPending = false;
        }

        private void StartSetupCoroutine(MonoBehaviour context, Action setupAction)
        {
            if (context != null)
            {
                isSetupPending = true;
                isSetupComplete = false;
                pendingSetupAction = setupAction;
                context.StartCoroutine(WaitForMainCameraAndExecute(setupAction));
            }
            else
            {
                if (Camera.main != null)
                {
                    setupAction?.Invoke();
                    isSetupComplete = true;
                }
            }
        }

        public void TryCompletePendingSetup()
        {
            if (isSetupPending && Camera.main != null && pendingSetupAction != null)
            {
                pendingSetupAction.Invoke();
                isSetupComplete = true;
                isSetupPending = false;
                pendingSetupAction = null;
            }
        }

        public bool IsSetupComplete()
        {
            return isSetupComplete;
        }

        public void OnCameraSwapped()
        {
            if (isSetupComplete && pendingSetupAction != null)
            {
                if (Camera.main != null)
                    pendingSetupAction.Invoke();
            }
        }

        public void Setup(SpriteRenderer sr, Transform holder, string layerName, float resolution = 1, RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Point, float bitdepth = 0, MonoBehaviour context = null)
        {
            rendererType = LayerRendererType.spriteRendererSingle;
            mainCamera = holder.GetComponent<Camera>();

            this.holder = holder;
            this.layerName = layerName;
            this.format = format;
            this.fliterMode = filterMode;
            this.bitDepth = bitdepth;
            this.res = resolution;
            this.sr = sr;

            Action setupAction = () => ExecuteSpriteRendererSingleSetup(sr);

            if (Camera.main != null)
            {
                setupAction.Invoke();
                isSetupComplete = true;
            }
            else if (context != null)
                StartSetupCoroutine(context, setupAction);
            else
            {
                isSetupPending = true;
                pendingSetupAction = setupAction;
            }
        }

        private void ExecuteSpriteRendererSingleSetup(SpriteRenderer sr)
        {
            if (mCamera == null) return;

            mainCamera.aspect = sr.bounds.extents.x / sr.bounds.extents.y;

            StripCamera();
            mainCamera.orthographicSize = sr.bounds.size.y / 2f;
            mainCamera.backgroundColor = Color.clear;

            if (Screen.width == 0) return;
            if (layerTexture != null) layerTexture.Release();

            CreateRTSpriteRenderer(sr, mCamera);

#if UNITY_EDITOR
            if (!WaterLayers.LayerExists(layerName))
                WaterLayers.CreateLayer(layerName);
#endif
            reflectionLayerIdx = Obstructor.GetLayerIdx(layerName);

            mainCamera.depth = mCamera.depth - 1;
            if (reflectionLayerIdx != -1)
            {
                int cmask = (1 << reflectionLayerIdx);
                mainCamera.cullingMask = cmask;
                mainCamera.targetTexture = layerTexture;
                RemoveLayerFromMainCamera();
            }
        }

        public void Setup(SpriteRenderer sr, Transform holder, int layers, float resolution = 1, RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Point, float bitdepth = 0, MonoBehaviour context = null)
        {
            rendererType = LayerRendererType.spriteRendererMultiple;
            mainCamera = holder.GetComponent<Camera>();

            this.holder = holder;
            this.layerMask = layers;
            this.format = format;
            this.fliterMode = filterMode;
            this.bitDepth = bitdepth;
            this.res = resolution;
            this.sr = sr;

            Action setupAction = () => ExecuteSpriteRendererMultipleSetup(sr);

            if (Camera.main != null)
            {
                setupAction.Invoke();
                isSetupComplete = true;
            }
            else if (context != null)
                StartSetupCoroutine(context, setupAction);
            else
            {
                isSetupPending = true;
                pendingSetupAction = setupAction;
            }
        }

        private void ExecuteSpriteRendererMultipleSetup(SpriteRenderer sr)
        {
            if (mCamera == null) return;

            mainCamera.aspect = sr.bounds.extents.x / sr.bounds.extents.y;

            StripCamera();
            mainCamera.orthographicSize = sr.bounds.size.y / 2f;
            mainCamera.backgroundColor = Color.clear;

            if (Screen.width == 0) return;
            if (layerTexture != null) layerTexture.Release();

            CreateRTSpriteRenderer(sr, mCamera);

            mainCamera.cullingMask = layerMask;
            mainCamera.targetTexture = layerTexture;
            mainCamera.depth = mCamera.depth - 1;
        }

        private void CreateRT(SpriteRenderer sr, Camera mCamera, LayerRendererType type)
        {
            if (mCamera == null) return;

            switch (type)
            {
                case LayerRendererType.spriteRendererSingle:
                case LayerRendererType.spriteRendererMultiple:
                    CreateRTSpriteRenderer(sr, mCamera);
                    break;
                case LayerRendererType.screenSingle:
                case LayerRendererType.screenMultiple:
                    CreateRTCamera(mCamera);
                    break;
            }
        }

        private void CreateRTSpriteRenderer(SpriteRenderer sr, Camera mCamera)
        {
            if (mCamera == null) return;

            float perX = sr.bounds.size.x / (mCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - mCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x);
            float perY = sr.bounds.size.y / (mCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - mCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y);
            if (layerTexture != null) layerTexture.Release();
            if (layerTexture == null) layerTexture = new RenderTexture((int)(mCamera.scaledPixelWidth * res * perX * Mathf.Max(1f, scale.x)), (int)(mCamera.scaledPixelHeight * res * perY * Mathf.Max(1f, scale.y)), 32, format, 0);
            else
            {
                layerTexture.width = (int)(mCamera.scaledPixelWidth * res * perX * Mathf.Max(1f, scale.x));
                layerTexture.height = (int)(mCamera.scaledPixelHeight * res * perY * Mathf.Max(1f, scale.y));
            }
            layerTexture.depthStencilFormat = GraphicsFormat.D32_SFloat;
            layerTexture.filterMode = FilterMode.Bilinear;
            layerTexture.enableRandomWrite = false;
            if (layerTexture == null) layerTexture.Create();
            mainCamera.targetTexture = layerTexture;
        }

        private void CreateRTCamera(Camera mCamera)
        {
            if (mCamera == null) return;

            int baseWidth;
            int baseHeight;

            PixelPerfectCamera ppc = mCamera.GetComponent<PixelPerfectCamera>();

            if (ppc != null)
            {
                float scaleX = (float)Screen.width / ppc.refResolutionX;
                float scaleY = (float)Screen.height / ppc.refResolutionY;

                float pixelScale = ppc.stretchFill
                    ? Mathf.Max(scaleX, scaleY)
                    : Mathf.Min(scaleX, scaleY);

                baseWidth = Mathf.RoundToInt(ppc.refResolutionX * pixelScale);
                baseHeight = Mathf.RoundToInt(ppc.refResolutionY * pixelScale);
            }
            else
            {
                baseWidth = mCamera.scaledPixelWidth;
                baseHeight = mCamera.scaledPixelHeight;
            }

            int finalWidth = Mathf.Max(1, Mathf.RoundToInt(baseWidth * res * Mathf.Max(1f, scale.x)));
            int finalHeight = Mathf.Max(1, Mathf.RoundToInt(baseHeight * res * Mathf.Max(1f, scale.y)));

            if (layerTexture == null)
                layerTexture = new RenderTexture(finalWidth, finalHeight, 32, format, 0);
            else if (layerTexture.width != finalWidth || layerTexture.height != finalHeight)
            {
                layerTexture.Release();
                layerTexture.width = finalWidth;
                layerTexture.height = finalHeight;
            }

            layerTexture.depthStencilFormat = GraphicsFormat.D32_SFloat;
            layerTexture.filterMode = FilterMode.Bilinear;
            layerTexture.enableRandomWrite = false;

            if (!layerTexture.IsCreated())
                layerTexture.Create();

            mainCamera.targetTexture = layerTexture;
        }

        void StripCamera()
        {
            mainCamera.depthTextureMode = DepthTextureMode.None;
        }

        public void Setup(Transform holder, string layerName, Vector2 scale, float resolution = 1, RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Point, float bitdepth = 0, MonoBehaviour context = null)
        {
            rendererType = LayerRendererType.screenSingle;
            mainCamera = holder.GetComponent<Camera>();

            StripCamera();

            this.holder = holder;
            this.layerName = layerName;
            this.format = format;
            this.fliterMode = filterMode;
            this.bitDepth = bitdepth;
            this.res = resolution;
            this.scale = scale;

            Action setupAction = () => ExecuteScreenSingleSetup();

            if (Camera.main != null)
            {
                setupAction.Invoke();
                isSetupComplete = true;
            }
            else if (context != null)
                StartSetupCoroutine(context, setupAction);
            else
            {
                isSetupPending = true;
                pendingSetupAction = setupAction;
            }
        }

        private void ExecuteScreenSingleSetup()
        {
            if (mCamera == null) return;

            mainCamera.CopyFrom(mCamera);
            mainCamera.depth = mCamera.depth - 1;
            mainCamera.aspect = mCamera.aspect * (scale.x / scale.y);
            mainCamera.orthographicSize = mCamera.orthographicSize * this.scale.y;
            lastOrtographicSize = mainCamera.orthographicSize;
            lastAspectRatio = mainCamera.aspect;
            if (!copyMainBackground) mainCamera.backgroundColor = Color.clear;
            else mainCamera.backgroundColor = mCamera.backgroundColor;

            RTSetup();
        }

        public void Setup(Transform holder, int layers, Vector2 scale, float resolution = 1, RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Point, float bitdepth = 0, MonoBehaviour context = null)
        {
            rendererType = LayerRendererType.screenMultiple;
            mainCamera = holder.GetComponent<Camera>();

            StripCamera();

            this.holder = holder;
            this.layerMask = layers;
            this.format = format;
            this.fliterMode = filterMode;
            this.bitDepth = bitdepth;
            this.res = resolution;
            this.scale = scale;

            Action setupAction = () => ExecuteScreenMultipleSetup();

            if (Camera.main != null)
            {
                setupAction.Invoke();
                isSetupComplete = true;
            }
            else if (context != null)
                StartSetupCoroutine(context, setupAction);
            else
            {
                isSetupPending = true;
                pendingSetupAction = setupAction;
            }
        }

        private void ExecuteScreenMultipleSetup()
        {
            if (mCamera == null) return;

            mainCamera.CopyFrom(mCamera);
            mainCamera.depth = mCamera.depth - 1;
            mainCamera.aspect = mCamera.aspect * (scale.x / scale.y);
            mainCamera.orthographicSize = mCamera.orthographicSize * this.scale.y;
            lastOrtographicSize = mainCamera.orthographicSize;
            lastAspectRatio = mainCamera.aspect;
            if (!copyMainBackground) mainCamera.backgroundColor = Color.clear;
            else mainCamera.backgroundColor = mCamera.backgroundColor;

            RTSetupExtended();
        }

        private void RTSetupExtended()
        {
            if (Screen.width == 0) return;
            if (layerTexture != null) layerTexture.Release();

            CreateRTCamera(mainCamera);
            mainCamera.cullingMask = layerMask;
            mainCamera.targetTexture = layerTexture;
        }

        private void RTSetup()
        {
            if (Screen.width == 0) return;
            if (layerTexture != null) layerTexture.Release();
            CreateRTCamera(mainCamera);

#if UNITY_EDITOR
            if (!WaterLayers.LayerExists(layerName))
                WaterLayers.CreateLayer(layerName);
#endif

            reflectionLayerIdx = Obstructor.GetLayerIdx(layerName);

            if (reflectionLayerIdx != -1)
            {
                int cmask = (1 << reflectionLayerIdx);
                if (follow != null) mainCamera.rect = follow.GetComponent<Camera>().rect;
                mainCamera.cullingMask = cmask;
                mainCamera.targetTexture = layerTexture;
                RemoveLayerFromMainCamera();
            }
        }

        public void Loop()
        {
            TryCompletePendingSetup();

            // Apply the run state once per frame here, after WaterFeatureLayerRenderer has
            // tallied all votes from every water source and written the final value to run.
            ApplyRunState();

            if (!isSetupComplete) return;

            RemoveLayerFromMainCamera();
            FollowCamera();
            UpdateCameraSize();
        }

        private void UpdateCameraSize()
        {
            if (CameraRenderingScene == null) return;

            if (Mathf.Abs(mainCamera.orthographicSize - CameraRenderingScene.orthographicSize * scale.y) > 0.01f) { lastOrtographicSize = mainCamera.orthographicSize = CameraRenderingScene.orthographicSize * scale.y; CreateRT(sr, CameraRenderingScene, rendererType); }
            if (Mathf.Abs(mainCamera.aspect - CameraRenderingScene.aspect * (scale.x / scale.y)) > 0.01f) { lastAspectRatio = mainCamera.aspect = CameraRenderingScene.aspect * (scale.x / scale.y); CreateRT(sr, CameraRenderingScene, rendererType); }
            if (follow != null && !Rect.Equals(mainCamera.rect, follow.GetComponent<Camera>().rect)) mainCamera.rect = follow.GetComponent<Camera>().rect;
        }

        private void RemoveLayerFromMainCamera()
        {
            if (follow == null) return;
            Camera followCamera = follow.GetComponent<Camera>();
            if (followCamera == null) return;
            if ((followCamera.cullingMask & (1 << reflectionLayerIdx)) != 0) followCamera.cullingMask &= ~(1 << reflectionLayerIdx);
        }

        private void FollowCamera()
        {
            if (follow == null) return;
           
            holder.position = follow.position;
            holder.rotation = follow.rotation;
            holder.SetGlobalScale(follow.lossyScale);
        }

        public void Release()
        {
            mainCamera.targetTexture = null;
            if (layerTexture != null) layerTexture.Release();
            isSetupComplete = false;
            isSetupPending = false;
            pendingSetupAction = null;
        }
    }

}