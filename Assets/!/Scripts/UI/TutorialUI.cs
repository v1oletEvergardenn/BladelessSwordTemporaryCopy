using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    private ControllerInput INPUTcontrol;
    private ControllerInput.GameplayActions control;
    private GameManager gameManager;

    public List<GameObject> pages = new List<GameObject>();
    private int currentIndex = 0;

    private float pageTimer = 0f;

    public GameObject leftArrow;
    public GameObject rightArrow;

    // Start is called before the first frame update
    private void OnEnable()
    {
        INPUTcontrol = new ControllerInput();
        control = INPUTcontrol.Gameplay;
        control.Enable();
        gameManager = GameManager.instance;
        gameManager.isInInformationEvent = true;
        VFXManager.instance.FreezeTime();
        currentIndex = 0;
        Page();
    }

    // Update is called once per frame
    private void Update()
    {
        float x = control.Event_flip_page.ReadValue<Vector2>().x;
        if (control.EventKey.WasPressedThisFrame() && currentIndex == pages.Count - 1)
        {
            //next page or finish the tutorial.
            gameObject.SetActive(false);
            gameManager.isInInformationEvent = false;
            VFXManager.instance.UnFreezeTime();
        }
        else if (x < -0.5 && Time.realtimeSinceStartup - pageTimer >= 0.3f)
        {
            //page before
            currentIndex -= 1;
            Page();
        }
        else if (x > 0.5 && Time.realtimeSinceStartup - pageTimer >= 0.3f)
        {
            //page next
            currentIndex += 1;
            Page();
        }
    }

    private void Page()
    {
        {
            if (currentIndex <= 0) { currentIndex = 0; }
            if (currentIndex > pages.Count - 1) { currentIndex = pages.Count - 1; }
            foreach (GameObject i in pages)
            {
                i.SetActive(false);
            }
            pages[currentIndex].SetActive(true);
            rightArrow.SetActive(true);
            leftArrow.SetActive(true);
            if (currentIndex - 1 < 0)
            {
                leftArrow.SetActive(false);
            }
            if (currentIndex + 1 == pages.Count)
            {
                rightArrow.SetActive(false);
            }
            pageTimer = Time.realtimeSinceStartup;
        }
    }
}