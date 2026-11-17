using UnityEngine;

public class Container2 : MonoBehaviour
{
    public QuatityManager uiManager;
    private int quantity = 0;

    [SerializeField]
    private int value = 1;

    void Start()
    {
        quantity = 0;
        uiManager.ExtractName(value);
        uiManager.UpdateQuantity(quantity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractCube"))
        {

        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        quantity--;
        uiManager.UpdateQuantity(quantity);
    }

}
 