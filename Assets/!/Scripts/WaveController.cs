using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public float waveDuration = 0.5f;

    private Coroutine waveCoroutine;
    public GameObject wave;
    public Material mat;

    private static int waveDistanceFromCenter = Shader.PropertyToID("_waveDistanceFromCenter");
    private static int waveStrength = Shader.PropertyToID("_waveStrength");
    private static int waveSize = Shader.PropertyToID("_size");

    private void Awake()
    {
        mat = new Material(Shader.Find("Shader Graphs/WaveShader"));
        wave.GetComponent<SpriteRenderer>().material = mat;
    }

    public void CallWave()
    {
        gameObject.SetActive(true);
        StartCoroutine(IEWave(-0.1f, 1f));
    }

    private IEnumerator IEWave(float startPos, float endPos)
    {
        mat.SetFloat(waveDistanceFromCenter, startPos);
        mat.SetFloat(waveStrength, 0.4f);
        mat.SetFloat(waveSize, 0.05f);
        float lerpAmount = 0f;
        float lerpStrength = 0.3f;
        float lerpSize = 0.05f;
        float elapsedTime = 0f;
        while (elapsedTime < waveDuration)
        {
            elapsedTime += Time.deltaTime;
            lerpAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / waveDuration));
            lerpStrength = Mathf.Lerp(0.3f, -0.1f, (elapsedTime / waveDuration));
            lerpSize = Mathf.Lerp(0.05f, 0.1f, (elapsedTime / waveDuration));
            mat.SetFloat(waveDistanceFromCenter, lerpAmount);
            mat.SetFloat(waveSize, lerpSize);
            mat.SetFloat(waveStrength, lerpStrength);
            yield return null;
        }

        gameObject.SetActive(false);
        yield return null;
    }
}