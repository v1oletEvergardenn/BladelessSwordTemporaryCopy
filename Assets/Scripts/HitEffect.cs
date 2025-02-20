using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public Animator anim;

    public void Perfect()
    {
        randomRot();
        anim.Play("perfect_effect");
    }

    public void Normal()
    {
        randomRot();
        anim.Play("normal_effect");
    }

    private void randomRot()
    {
        float i = Random.Range(0f, 360f);
        transform.eulerAngles = new Vector3(0, 0, i);
    }

    public void End()
    {
        this.gameObject.SetActive(false);
    }
}