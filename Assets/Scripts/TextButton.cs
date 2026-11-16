using TMPro;
using UnityEngine;

public class TextButton : MonoBehaviour
{
    public TextMeshPro textButton;
    public void ExtractName(int nameNumber)
    {
        textButton.text = "+ " + nameNumber.ToString();
    }
}
