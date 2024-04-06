using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UMAController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private bool isMoving = false;

    void Start()
    {
        // Ottenere i riferimenti al NavMeshAgent e all'Animator
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Imposta il parametro "Speed" a 0 all'avvio
        animator.SetFloat("Speed", 0);
    }

    void Update()
    {
        // Se viene cliccato il tasto sinistro del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Lanciare un raggio dal punto in cui è stato cliccato
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Se il raggio colpisce un oggetto sulla NavMesh
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, NavMesh.AllAreas))
            {
                // Imposta la destinazione per il NavMeshAgent
                navMeshAgent.SetDestination(hit.point);
                isMoving = true; // Imposta il flag per indicare che l'UMA sta per muoversi

                // Attiva l'animazione di corsa
                animator.SetFloat("Speed", 1);
            }
        }

        // Se l'UMA è in movimento e ha raggiunto la destinazione
        if (isMoving && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            // Se ha raggiunto la destinazione, ferma l'UMA e reimposta il flag
            animator.SetFloat("Speed", 0);
            isMoving = false;
        }
    }
}






