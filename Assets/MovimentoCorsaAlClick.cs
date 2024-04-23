using System.Collections.Generic;
using UMA;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.UI.CanvasScaler;

public class MovimentoCorsaAlClick : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar;
    float minSpeedWalk = 0.22f; // Modificato per un rallentamento più marcato
    float maxSpeedWalk = 0.33f;
    float medSpeedWalk = 0.28f;
    float detectionRadius = 3f;
    private float maxTimeToDestination = 10f; // Tempo massimo consentito per raggiungere la destinazione
    private float currentTimeToDestination = 0f; // Timer per tenere traccia del tempo trascorso

    void Start()
    {

    }

    void Update()
    {
        UpdateUMAAnimations();
        UpdateUMADestinations();
    }

    void UpdateUMAAnimations()
    {
        // Lista per memorizzare tutti gli UMA e le loro posizioni
        List<Transform> allUMATransforms = new List<Transform>();

        // Popola la lista con tutte le posizioni degli UMA
        foreach (Transform child in umaRandomAvatar.transform)
        {
            allUMATransforms.Add(child);
        }

        // Per ogni UMA, calcola la distanza con tutti gli altri UMA
        foreach (Transform umaTransform in allUMATransforms)
        {
            Animator animator = umaTransform.GetComponent<Animator>();
            // Lista per memorizzare le distanze tra l'UMA corrente e gli altri UMA
            List<Transform> nearbyUMAs = new List<Transform>();

            foreach (Transform otherUMATransform in allUMATransforms)
            {
                if (otherUMATransform != umaTransform)
                {
                    float distance = Vector3.Distance(umaTransform.position, otherUMATransform.position);

                    //Se la distanza è inferiore a 2 unità di lunghezza(2f), significa che l'altro UMA si trova entro un raggio di 2 unità di lunghezza rispetto all'UMA corrente.
                    //In tal caso, aggiungiamo l'altro UMA alla lista nearbyUMAs, indicando che è vicino all'UMA corrente.
                    if (distance < detectionRadius)
                    {
                        nearbyUMAs.Add(otherUMATransform);
                    }
                }
            }

            // Conta il numero di UMA nelle vicinanze, escludendo l'UMA stesso
            int nearbyUMACount = nearbyUMAs.Count; // Sottrai 1 per escludere l'UMA corrente

            NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();

            // Se non ci sono UMA nelle vicinanze o al massimo 3, incrementa progressivamente la velocità fino a 0.40
            if (nearbyUMACount >= 0 && nearbyUMACount <= 2)
            {

                float targetSpeed = Mathf.Lerp(0f, maxSpeedWalk, Mathf.Clamp01(Time.time)); // Modifica il 5f con la durata desiderata per raggiungere la velocità massima
                animator.SetFloat("Speed", targetSpeed);
                navMeshAgent.speed = targetSpeed * 3;
            }
            else if (nearbyUMACount > 2 && nearbyUMACount <= 5)
            {
                // Rallenta progressivamente da 0.40 a 0.30
                float targetSpeed = Mathf.Lerp(maxSpeedWalk, medSpeedWalk, Mathf.Clamp01((nearbyUMACount - 2) / 10f));
                // Applica il nuovo speed all'animator
                animator.SetFloat("Speed", targetSpeed);
                navMeshAgent.speed = targetSpeed * 3;
            }
            else
            {
                // Rallenta progressivamente da 0.30 a 0.22
                float targetSpeed = Mathf.Lerp(medSpeedWalk, minSpeedWalk, Mathf.Clamp01((nearbyUMACount - 6) / 10f));
                // Applica il nuovo speed all'animator
                animator.SetFloat("Speed", targetSpeed);
                navMeshAgent.speed = targetSpeed * 3;
            }
        }
    }

    void UpdateUMADestinations()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();

            // Imposta il raggio e la distanza di sosta del NavMeshAgent
            navMeshAgent.radius = 0.1f;
            navMeshAgent.acceleration = 3f;
            navMeshAgent.angularSpeed = 180f;
            // Genera un livello di priorità random tra 1 e 100
            navMeshAgent.avoidancePriority = Random.Range(1, 100);

            // Calcola la distanza rimanente alla destinazione
            float distanceToDestination = navMeshAgent.remainingDistance;
            // Se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && distanceToDestination < 2f)
            {
                // Aggiorna il timer
                currentTimeToDestination += Time.deltaTime;

                // Controlla se il tempo massimo è stato superato
                if (currentTimeToDestination >= maxTimeToDestination)
                {
                    Vector3 randomDestination = GetRandomNavmeshLocation();
                    navMeshAgent.SetDestination(randomDestination);
                    // Resetta il timer
                    currentTimeToDestination = 0f;
                }
            }
            else
            {
                // Ottieni una nuova destinazione casuale
                Vector3 randomDestination = GetRandomNavmeshLocation();
                navMeshAgent.SetDestination(randomDestination);
                // Controlla se l'agente sta ancora cercando di raggiungere la destinazione
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



