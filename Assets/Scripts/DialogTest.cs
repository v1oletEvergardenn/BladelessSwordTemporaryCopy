using Doublsb.Dialog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Characters
{
    Null,
    Qin,
    QinTest,
    Sword,
    Qianxiao,
    Caiyu
}

[System.Serializable]
public class DialogInformation
{
    [HideInInspector] public string sentenceIndex;
    public Characters firstCharacter;
    public Characters secondCharacter;
    public string emotion = "Normal";
    public bool isSkippable = true;
    [TextArea(3, 10)] public string sentence;
    public UnityEvent callback;
}

public class DialogTest : MonoBehaviour
{
    public int initialSize;
    public float initialSpeed;

    [SerializeField] public List<DialogInformation> dialogs = new List<DialogInformation>();

    private List<DialogData> text;

    private void Awake()
    {
        var dialogTexts = new List<DialogData>();
        for (int i = 0; i < dialogs.Count; i++)
        {
            dialogTexts.Add(new DialogData($"/size:{initialSize}//speed:{initialSpeed}/{dialogs[i].sentence}", FindCharacter(dialogs[i].firstCharacter), FindCharacter(dialogs[i].secondCharacter), dialogs[i].callback, dialogs[i].isSkippable));
        }
        dialogTexts[dialogs.Count - 1].Callback.AddListener(endText);
        text = dialogTexts;
    }

    public string FindCharacter(Characters i)
    {
        if (i == Characters.Null)
        {
            return "";
        }
        else if (i == Characters.Qin)
        {
            return "Qin";
        }
        else if (i == Characters.QinTest)
        {
            return "Qin_test";
        }
        else if (i == Characters.Sword)
        {
            return "Sword";
        }
        else if (i == Characters.Qianxiao)
        {
            return "Qianxiao";
        }
        else if (i == Characters.Caiyu)
        {
            return "Caiyu";
        }
        return "";
    }

    public void startText()
    {
        DialogManager.instance.Show(text);
        GameManager.instance.isInDialog = true;
        PlayerAttack.instance.anim.SetBool("isRunning", false);
    }

    public void endText()
    {
        GameManager.instance.isInDialog = false;
    }
}