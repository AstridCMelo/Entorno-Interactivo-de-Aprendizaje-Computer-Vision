using UnityEngine;

public class Container2 : MonoBehaviour
{
    public QuatityManager uiManager;
    public Testing grid;
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
        var piece = other.GetComponent<Pieces>();
        if (piece.valor == value)
        {
            quantity++;
            uiManager.UpdateQuantity(quantity);
            Vector3 newPosition = grid.setFullGrid();
            other.gameObject.transform.position = new Vector3(newPosition.x, other.gameObject.transform.position.y, newPosition.z);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        quantity--;
        uiManager.UpdateQuantity(quantity);
        grid.setEmptyGrid();
    }

}
 