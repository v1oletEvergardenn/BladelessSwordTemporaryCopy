using UnityEngine;

[CreateAssetMenu(fileName = "IDamagbleActionKey", menuName = "QuestSystem/IDamagble Action Key")]
public class IDamagbleActionKey : QuestActionKey
{
    public string getHit => ActionID("getHit");
    public string dead => ActionID("dead");
}