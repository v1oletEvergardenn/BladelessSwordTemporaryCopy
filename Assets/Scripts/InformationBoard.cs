using UnityEngine;

public class InformationBoard : EventObject
{
    // Start is called before the first frame update
    private GameManager gameManager;

    public GameManager.PlayerSkillsType playerSkillsType;
    public GameObject informationUI;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public override void InteractEvent()
    {
        _Event?.Invoke();
        informationUI.SetActive(true);
        gameManager.LearnSkills(playerSkillsType);
    }
}