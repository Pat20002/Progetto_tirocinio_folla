using UMA;
using UnityEngine;
using UnityEngine.AI;

public class MovimentoCorsaAlClick : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar;
    float minSpeedWalk = 0.25f; // Modificato per un rallentamento più marcato
    float maxSpeedWalk = 0.45f;

    void Update()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            Animator animator = child.GetComponent<Animator>();

            // Imposta il raggio e la distanza di sosta del NavMeshAgent
            navMeshAgent.radius = 0.2f;
            navMeshAgent.stoppingDistance = 1f;
            navMeshAgent.speed = 1f;
            navMeshAgent.angularSpeed = 180f;

            // Calcola la distanza rimanente alla destinazione
            float distanceToDestination = navMeshAgent.remainingDistance;

            // Se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && distanceToDestination < 2.5)
            {
                // Rallenta l'UMA
                animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), minSpeedWalk, Time.deltaTime * 4f));
            }
            else
            {
                // Se l'UMA non è vicino alla destinazione, aumenta la velocità
                animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), maxSpeedWalk, Time.deltaTime * 4f));
            }

            // Se l'UMA ha completato il percorso e non ha un percorso in sospeso, assegna una nuova destinazione
            if (!navMeshAgent.pathPending && distanceToDestination < navMeshAgent.stoppingDistance)
            {
                // Ottieni una nuova destinazione casuale
                Vector3 randomDestination = GetRandomNavmeshLocation();
                navMeshAgent.SetDestination(randomDestination);
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

