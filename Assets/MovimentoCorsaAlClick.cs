using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UMA;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class MovimentoCorsaAlClick : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar; // Riferimento a UMARandomAvatar
    private bool isRunning = false; // Flag per controllare se gli UMAs stanno correndo
    private float modificaDest = 15f;
    

    void Start()
    {
        
    }

    void Update()
    {
        if (isRunning == false)
        {
            foreach (Transform child in umaRandomAvatar.transform)
            {
                NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
                navMeshAgent.speed = 2.2f;
                navMeshAgent.acceleration = 1f;
                navMeshAgent.angularSpeed = 180f;
                navMeshAgent.radius = 0.2f;
                navMeshAgent.stoppingDistance = 1f;

                Animator animator = navMeshAgent.GetComponent<Animator>();
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;


                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
                {
                    Vector3 randomDestination = GetRandomNavmeshLocation();
                    navMeshAgent.SetDestination(randomDestination);
                }
            }
        }

        // Se viene cliccato il tasto sinistro del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Lanciare un raggio dal punto in cui � stato cliccato
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Se il raggio colpisce un oggetto sulla NavMesh
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, NavMesh.AllAreas))
            {

                // Ottieni le coordinate del punto colpito
                Vector3 clickedPoint = hit.point;
                Debug.Log("Coordinate del punto colpito: " + clickedPoint);

                // Itera attraverso ogni UMA generato
                foreach (Transform child in umaRandomAvatar.transform)
                {
                    isRunning = true;

                    // Ottieni il componente NavMeshAgent dell'UMA
                    NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
                    Animator animator = navMeshAgent.GetComponent<Animator>();

                    //animator.SetTrigger("CrouchTrigger");
                    // Ottieni le coordinate dell'UMA
                    Vector3 umaPosition = child.position;

                    // Calcola la distanza tra l'UMA e il punto cliccato
                    float distance = Vector3.Distance(umaPosition, clickedPoint);

                    // Definisci il raggio dell'area
                    float radius = 1.8f;

                    if (distance <= radius)
                    {
                        navMeshAgent.speed = 0f;
                        animator.SetTrigger("DeathTrigger");
                        animator.SetBool("IsDeath",true);
                    }
                    else
                    {
                        // Se clicco in alto, gli uma andranno in basso
                        if (clickedPoint.z >= umaPosition.z && animator.GetBool("IsDeath")==false)
                        {
                            if (clickedPoint.x < umaPosition.x)
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione
                                Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                            else
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione 
                                Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                        }
                        // se clicco in basso, gli uma andranno in alto
                        if (clickedPoint.z <= umaPosition.z && animator.GetBool("IsDeath") == false)
                        {
                            if (clickedPoint.x < umaPosition.x)
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione
                                Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                            else
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione 
                                Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                        }
                        // se clicco a destra, gli uma andranno a sinistra
                        if (clickedPoint.x >= umaPosition.x && animator.GetBool("IsDeath") == false)
                        {
                            if (clickedPoint.z < umaPosition.z)
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione    
                                Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                            else
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione 
                                Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                        }
                        //se clicco a sinistra, gli uma andranno a destra 
                        if (clickedPoint.x <= umaPosition.x && animator.GetBool("IsDeath") == false)
                        {
                            if (clickedPoint.z < umaPosition.z)
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione 
                                Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                            else
                            {
                                animator.SetTrigger("RunTrigger");
                                // Calcola la nuova destinazione 
                                Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                                navMeshAgent.SetDestination(newDestination);
                            }
                        }
                    }
                }
            }
        }
        foreach (Transform child in umaRandomAvatar.transform)
        {

            // Ottieni il componente NavMeshAgent dell'UMA
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            Animator animator = navMeshAgent.GetComponent<Animator>();

            // Controlla se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 1f)
            {
                animator.SetTrigger("IdleTrigger");
            }
        }
    }

    // Genera una posizione casuale all'interno del navmesh
    Vector3 GetRandomNavmeshLocation()
    {
        NavMeshHit navHit;
        Vector3 randomPoint = Vector3.zero;
        if (NavMesh.SamplePosition(transform.position + Random.insideUnitSphere * 13f, out navHit, 10f, NavMesh.AllAreas))
        {
            randomPoint = navHit.position;
        }
        return randomPoint;
    }
}