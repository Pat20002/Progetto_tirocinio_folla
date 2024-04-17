using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UMA;

public class SimulazionePericolo : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar; // Riferimento a UMARandomAvatar
    public float runSpeed = 3f; // Velocità di corsa degli UMAs quando si allontanano
    public Animator animator; // Riferimento all'animator degli UMAs
    public string runTrigger = "RunTrigger"; // Nome del trigger per attivare l'animazione di corsa

    void Update()
    {
        // Gestisci l'input del mouse
        if (Input.GetMouseButtonDown(1)) // Tasto destro del mouse
        {
            // Ottieni la posizione del clic del mouse nel mondo
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Imposta la destinazione per gli UMAs per allontanarsi dal punto cliccato
                foreach (Transform child in umaRandomAvatar.transform)
                {
                    // Ottieni il componente Animator dell'UMA
                    Animator animator = child.GetComponent<Animator>();

                    // Attiva l'animazione di corsa per gli UMAs
                    animator.SetTrigger(runTrigger);

                    NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
                    if (navMeshAgent != null)
                    {
                        Vector3 directionToTarget = (child.position - hit.point).normalized;
                        Vector3 runDestination = child.position + directionToTarget * 10f; // Allontanati di 10 metri dal punto cliccato
                        navMeshAgent.SetDestination(runDestination);
                    }
                }
            }
        }
    }
}

