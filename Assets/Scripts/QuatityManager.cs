using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuatityManager : MonoBehaviour
{
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI nameContainer;

    Dictionary<int, string> namesContainer = new Dictionary<int, string>()
    {
    { 1000, "Mil" },
    { 100, "Centenas" },
    { 10, "Decenas" },
    { 1, "Unidades" }
    };

    public void ExtractName(int nameNumber)
    {
        namesContainer.TryGetValue(nameNumber, out string dictionaryName);
        nameContainer.text = dictionaryName;
    }

    public void UpdateQuantity(int quantity)
    {
        quantityText.text = quantity.ToString();
    }

}
