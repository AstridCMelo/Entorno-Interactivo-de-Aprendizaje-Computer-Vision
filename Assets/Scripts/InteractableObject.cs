using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class InteractableObject : MonoBehaviour
{
    // Identifica los objetos que pueden ser agarrados.
    public SpawnerContainer SOButton;
    public int value = 1;
    private Renderer rendererCube;

    public void Awake()
    {
        rendererCube = GetComponentInChildren<Renderer>();
        ChangeMat();
    }

    public void ChangeMat()
    {
        if (SOButton != null)
        {
            value = SOButton.value;
            rendererCube.material = SOButton.Normalmaterial;
        }
    }
}
