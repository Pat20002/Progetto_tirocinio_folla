using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UMA;

public class CamminataCasuale : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar; // Riferimento a UMARandomAvatar
    public Collider boundaryCollider; // Collider sferico che delimita lo spazio navigabile per gli UMAs
    public float avoidanceRadius = 2f; // Raggio di evitamento per la collisione tra UMAs
    public float changeDirectionInterval = 5f; // Intervallo di tempo dopo il quale cambia la direzione

    void Start()
    {
        StartWalking();
        InvokeRepeating("ChangeDirection", changeDirectionInterval, changeDirectionInterval);
    }

    void Update()
    {
        // Controlla se gli UMAs hanno raggiunto il bordo del collider o hanno colpito un ostacolo e, in tal caso, cambia direzione
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
                {
                    Vector3 randomDestination = GetRandomNavmeshLocationInCircle(boundaryCollider.transform.position, boundaryCollider.bounds.extents.x);
                    navMeshAgent.SetDestination(randomDestination);
                }
            }
        }
    }

    void StartWalking()
    {
        // Imposta la velocità e la destinazione per ogni UMA
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = 1f;
                Vector3 randomDestination = GetRandomNavmeshLocationInCircle(boundaryCollider.transform.position, boundaryCollider.bounds.extents.x);
                navMeshAgent.SetDestination(randomDestination);
                navMeshAgent.autoBraking = false; // Disabilita l'auto-frenata per permettere agli UMAs di continuare a muoversi anche dopo aver colpito un ostacolo
            }

            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetFloat("Speed", 1);
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }
        }
    }

    // Genera una posizione casuale all'interno della circonferenza
    Vector3 GetRandomNavmeshLocationInCircle(Vector3 center, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized * radius;
        Vector3 randomDestination = center + new Vector3(randomDirection.x, 0f, randomDirection.y);
        return randomDestination;
    }

    // Funzione per il cambio di direzione
    void ChangeDirection()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                Vector3 randomDestination = GetRandomNavmeshLocationInCircle(boundaryCollider.transform.position, boundaryCollider.bounds.extents.x);
                navMeshAgent.SetDestination(randomDestination);
            }
        }
    }
}

