using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 2f; // Velocità di movimento della telecamera
    public float rotationSpeed = 1f; // Velocità di rotazione della telecamera
    public float zoomSpeed = 5f; // Velocità di zoom della telecamera
    

    void Update()
    {
        // Movimento della telecamera
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // Calcola la rotazione della telecamera basata sulla posizione orizzontale del cursore del mouse
        float mouseX = Input.mousePosition.x / Screen.width; // Posizione orizzontale del cursore rispetto alla finestra di gioco (normalizzata)
        float rotationX = (mouseX - 0.5f) * 2; // Calcola la rotazione lungo l'asse X basata sulla posizione orizzontale del cursore (da -1 a 1)

        // Applica la rotazione della telecamera
        transform.Rotate(Vector3.up, rotationX * rotationSpeed);

        // Zoom della telecamera utilizzando la rotellina del mouse
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * scrollWheelInput * zoomSpeed);
    }
}
        
    



