using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class InteractableObject : MonoBehaviour
{
    // Identifica los objetos que pueden ser agarrados.
    public SpawnerContainer SOButton;
    public int value = 1;
    private Renderer rendererCube;


    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Transform padreInicial;
    private Rigidbody rb;

    public bool generadoDuranteJuego = false;

private void Start()
{
    //posicionInicial = transform.position;
    //rotacionInicial = transform.rotation;
    //padreInicial = transform.parent;
    //rb = GetComponent<Rigidbody>();
}

public void ReiniciarObjeto()
{
    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
    }

    transform.SetParent(padreInicial);
    transform.localPosition = posicionInicial;
    transform.localRotation = rotacionInicial;

    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}

    public void Awake()
    {
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;
        padreInicial = transform.parent;
        rb = GetComponent<Rigidbody>();
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
