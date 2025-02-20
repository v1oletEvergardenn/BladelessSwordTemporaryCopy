using Doublsb.Dialog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogForStoryDemo : MonoBehaviour
{
    public DialogManager DialogManager;

    public DialogData DialogData1;
    // Start is called before the first frame update
    private void Awake()
    {
        Chapter1_0();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Chapter1_0()
    {
        var dialogTexts = new List<DialogData>();
        dialogTexts.Add(new DialogData("/size:init/We¡¯re almost at Fushui Mountain.", "Qin"));
        dialogTexts.Add(new DialogData("/size:init//size:up/Aha!/wait:0.5//size:init/\nfinally going to find a scapegoat.", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/Let¡¯s try negotiating first,", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/if that doesn¡¯t work then...", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/Negotiating?", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/You still think you can solve this through talking, Qin?", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/Killing is always the last resort.", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/Little girl,/wait:0.5/ \nthat¡¯s exactly why you swing me but never take even a life.", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/At least I wil stick to my principles.", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/Don¡¯t forget, your life is on a countdown.", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/Last 3 years.../wait:0.5/\nand one year has already passed.", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/And that championship the knowledge-holders created is start in next month.", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/Is championship the only way to survive...?", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/(Should I kill others just to live?)", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/By the way, using a bird as transportation is pretty ridiculous.", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/...", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/(Isn¡¯t this one of your spells?)", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/( I¡¯d prefer to be more low-key.)", "Qin"));
        dialogTexts.Add(new DialogData("/size:init/You do realize that the connection of our minds is part of this shared life bond, right?", "Sword"));
        dialogTexts.Add(new DialogData("/size:init/Also, I don¡¯t talk to ridiculous birds.", "Sword"));
        DialogManager.Show(dialogTexts);

    }
}
