using UnityEngine;
using System.Collections.Generic;

namespace Mobsoft.PixelStyleWaterShader
{
    public class FootstepsParticleSystemController : MonoBehaviour
    {
        [System.Serializable]
        public class ParticleSystemData
        {
            public GameObject prefab;
            public float yOffset = 0f;
            [HideInInspector] public ParticleSystem instance;
        }

        public Transform followTransform; // Player's transform to follow
        public List<ParticleSystemData> particleSystems = new List<ParticleSystemData>(); // List of particle systems to instantiate

        private void Start()
        {
            InstantiateFootstepsParticleSystems();
        }

        private void Update()
        {
            if (followTransform != null)
            {
                // Update positions of all particle systems
                foreach (var psData in particleSystems)
                {
                    if (psData.instance != null)
                    {
                        Vector3 newPosition = followTransform.position;
                        newPosition.y += psData.yOffset; // Add the Y-axis offset
                        psData.instance.transform.position = newPosition;
                    }
                }
            }
        }

        private void InstantiateFootstepsParticleSystems()
        {
            foreach (var psData in particleSystems)
            {
                if (psData.prefab != null)
                {
                    // Instantiate the particle system prefab as a GameObject
                    GameObject particleSystemObject = Instantiate(psData.prefab, transform.position, Quaternion.identity);

                    // Get the ParticleSystem component from the instantiated GameObject
                    psData.instance = particleSystemObject.GetComponent<ParticleSystem>();

                    // Ensure the ParticleSystem component is valid
                    if (psData.instance == null)
                    {
                        Debug.LogError("The prefab must contain a ParticleSystem component.");
                        continue;
                    }

                    // Set the parent transform for the instantiated particle system
                    psData.instance.transform.SetParent(transform, false);

                    // Adjust the local position to apply the Y-axis offset
                    Vector3 newPosition = psData.instance.transform.localPosition;
                    newPosition.y = psData.yOffset;
                    psData.instance.transform.localPosition = newPosition;

                    // Set the start size of the particle system to 1.0
                    var mainModule = psData.instance.main;
                    mainModule.startSize = 1.0f;

                    // Debug log to verify the start size
                    Debug.Log("Particle system start size: " + mainModule.startSize.constant);
                }
                else
                {
                    Debug.LogError("Particle system prefab is not assigned.");
                }
            }
        }
    }
}