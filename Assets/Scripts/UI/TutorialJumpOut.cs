using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialJumpOut : MonoBehaviour
{
    public GameManager.PlayerSkillsType playerSkillsType;
    public TutType tutType;
    public void JumpOutTut()
    {
        MenuManager.instance.TutJumpOut(tutType);
        GameManager.instance.LearnSkills(playerSkillsType);
    }
}
