using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnimation : MonoBehaviour
{
    public Image m_Image;

    public Sprite[] m_SpriteArray;
    public float m_Speed = .02f;

    private int m_IndexSprite;
    private Coroutine m_CorotineAnim;
    private float animTimer;

    private void Update()
    {
        animTimer += TimeScaleManager.UIDt;
        if (animTimer >= m_Speed)
        {
            animTimer = 0;
            m_Image.sprite = m_SpriteArray[m_IndexSprite];
            m_IndexSprite = (m_IndexSprite + 1) % m_SpriteArray.Length;
        }
    }
}