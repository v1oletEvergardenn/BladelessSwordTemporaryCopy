using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeInputSprite : MonoBehaviour
{
    public Image targetImage;
    public InputKeyType inputKeyType;

    // Start is called before the first frame update
    private void Start()
    {
    }

    public void OnEnable()
    {
        targetImage.sprite = InputMaster.instance.icons.gamePadicons.GetSprite(inputKeyType);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}