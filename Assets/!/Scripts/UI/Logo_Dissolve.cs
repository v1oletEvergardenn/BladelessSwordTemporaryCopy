using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logo_Dissolve : MonoBehaviour
{
    public GameObject particle;
    public Material mat;
    public GameObject mainmenu;

    // Start is called before the first frame update
    private void Start()
    {
        particle.SetActive(false);
        mat.SetFloat("_DissolveAmount", 0f);
    }

    public void StartDissolve()
    {
        StartCoroutine(Dissolve());
    }

    public IEnumerator Dissolve()
    {
        float duration = 1.5f;
        float elapsed = 0f;

        particle.SetActive(true);
        while (elapsed < duration)
        {
            float dissolveAmount = Mathf.Lerp(0f, 2f, elapsed / duration);
            mat.SetFloat("_DissolveAmount", dissolveAmount);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainmenu.SetActive(true);
        this.gameObject.SetActive(false);

        yield return null;
    }
}