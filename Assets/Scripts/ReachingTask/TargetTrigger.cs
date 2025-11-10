using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TargetTrigger : MonoBehaviour
{
    private TrajectoryTrackingManager manager;

    void Start()
    {
        // Busca automáticamente al gestor en la escena
        manager = FindObjectOfType<TrajectoryTrackingManager>();

        // Asegúrate de que el collider es un trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto que entró es el cursor
        if (other.CompareTag("HapticCursor"))
        {
            manager.OnCursorEnterTarget(this.gameObject);
        }
    }
}