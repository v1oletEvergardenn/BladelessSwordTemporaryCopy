using EditorAttributes;
using UnityEngine;

namespace EditorAttributesSamples
{
    [HelpURL("https://editorattributesdocs.readthedocs.io/en/latest/Attributes/MiscellaneousAttributes/readonly.html")]
    public class ReadOnlySample : MonoBehaviour
    {
        [Header("ReadOnly Attribute:")]
        [SerializeField, ReadOnly] private int readonlyField;

        [SerializeField, ReadOnly] private int[] readonlyArray;
    }
}