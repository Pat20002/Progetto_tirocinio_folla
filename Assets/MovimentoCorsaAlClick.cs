using System.Collections.Generic;
using UMA;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
public class MovimentoCorsaAlClick : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar;
    float minSpeedWalk = 0.20f; // Modificato per un rallentamento più marcato
    float maxSpeedWalk = 0.33f;
    float detectionRadiusWalk = 1.5f;
    private float maxTimeToDestination = 10f; // Tempo massimo consentito per raggiungere la destinazione
    private float currentTimeToDestination = 0f; // Timer per tenere traccia del tempo trascorso
    bool CliccatoTastoDestro = false;
    float maxSpeedRun = 0.80f;
    float medSpeedRun = 0.65f;
    float minSpeedRun = 0.45f;
    float detectionRadiusRun = 1.0f;
    float counterInciampo = 0f;


    void Start()
    {

    }

    void Update()
    {
        CaratteristicheUMA();
        counterInciampo += Time.deltaTime * 5f;

        if (CliccatoTastoDestro == false)
        {
            UpdateUMAAnimations();
            UpdateUMADestinations();
        }
        else
        {
            
            RaggiungimentoDestinazione();
            Inciampo();
            MovimentoPerico();
        }

        // Se viene cliccato il tasto destro del mouse
        if (Input.GetMouseButtonDown(1))
        {
            counterInciampo = 0f;
            DeathAnimation();
        }
    }

    void CaratteristicheUMA()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            Animator animator = child.GetComponent<Animator>();
            child.tag = "Player";

            // Imposta il raggio e la distanza di sosta del NavMeshAgent
            navMeshAgent.radius = 0.1f;
            navMeshAgent.acceleration = 3f;
            navMeshAgent.angularSpeed = 120f;
            // Genera un livello di priorità random tra 1 e 100
            navMeshAgent.avoidancePriority = UnityEngine.Random.Range(1, 100);
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
    }

    void UpdateUMAAnimations()
    {

        foreach (Transform umaTransform in umaRandomAvatar.transform)
        {
            Animator animator = umaTransform.GetComponent<Animator>();
            NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();

            // Lista per memorizzare le distanze dagli altri UMA
            List<float> distancesToOtherUMAs = new List<float>();

            // Calcola la distanza dagli altri UMA
            foreach (Transform otherUMATransform in umaRandomAvatar.transform)
            {
                if (otherUMATransform != umaTransform)
                {
                    float distance = Vector3.Distance(umaTransform.position, otherUMATransform.position);
                    distancesToOtherUMAs.Add(distance);
                }
            }

            // Calcola la distanza minima dagli altri UMA
            float minDistanceToOtherUMAs = distancesToOtherUMAs.Count > 0 ? distancesToOtherUMAs.Min() : Mathf.Infinity;

            // Se la distanza minima è inferiore a un certo valore, rallenta l'UMA
            if (minDistanceToOtherUMAs < detectionRadiusWalk) // Imposta il valore appropriato
            {
                float targetSpeed = Mathf.Lerp(minSpeedWalk, maxSpeedWalk, minDistanceToOtherUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = targetSpeed * 3; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
            }
            else
            {
                navMeshAgent.speed = 1f; // Ripristina la velocità predefinita se non ci sono altri UMA nelle vicinanze
                animator.SetFloat("Speed", maxSpeedWalk); // Imposta la velocità predefinita dell'animator
            }
        }
    }

    void UpdateUMADestinations()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();

            // Calcola la distanza rimanente alla destinazione
            float distanceToDestination = navMeshAgent.remainingDistance;
            // Se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && distanceToDestination > 2f)
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


    void DeathAnimation()
    {
        CliccatoTastoDestro = true;
        float modificaDest = 20f;
        // Lanciare un raggio dal punto in cui è stato cliccato
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Se il raggio colpisce un oggetto sulla NavMesh
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {

            // Ottieni le coordinate del punto colpito
            Vector3 clickedPoint = hit.point;

            // Crea un collider sferico intorno al punto cliccato
            Collider[] colliders = Physics.OverlapSphere(clickedPoint, 2.5f); // 2.0f è il raggio dell'area


            // Itera attraverso gli UMA all'interno dell'area
            foreach (Collider collider in colliders)
            {
                NavMeshAgent navMeshAgent = collider.GetComponent<NavMeshAgent>();
                Animator animator = collider.GetComponent<Animator>();
                // Verifica se il collider appartiene a un UMA
                if (collider.CompareTag("Player"))
                {
                    animator.SetBool("IsDeath", true);
                    animator.SetTrigger("TriggerDeathF");

                }

            }
            foreach (Transform child in umaRandomAvatar.transform)
            {
                NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();

                // Verifica se l'UMA ha colliso con altri oggetti

                Vector3 umaPosition = child.position;

                // Se clicco in alto, gli uma andranno in basso
                if (clickedPoint.z >= umaPosition.z)
                {
                    if (clickedPoint.x <= umaPosition.x)
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione
                        Vector3 newDestination = new Vector3(umaPosition.x - modificaDest + UnityEngine.Random.Range(-10,10), umaPosition.y, umaPosition.z - modificaDest + UnityEngine.Random.Range(-10,10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                    else
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione 
                        Vector3 newDestination = new Vector3(umaPosition.x + modificaDest + UnityEngine.Random.Range(-10, 10), umaPosition.y, umaPosition.z - modificaDest + UnityEngine.Random.Range(-10, 10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                }
                // se clicco in basso, gli uma andranno in alto
                if (clickedPoint.z <= umaPosition.z)
                {
                    if (clickedPoint.x <= umaPosition.x)
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione
                        Vector3 newDestination = new Vector3(umaPosition.x + modificaDest + UnityEngine.Random.Range(-10,10), umaPosition.y, umaPosition.z + modificaDest + UnityEngine.Random.Range(-10,10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                    else
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione 
                        Vector3 newDestination = new Vector3(umaPosition.x - modificaDest + UnityEngine.Random.Range(-10, 10), umaPosition.y, umaPosition.z + modificaDest + UnityEngine.Random.Range(-10, 10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                }
                // se clicco a destra, gli uma andranno a sinistra
                if (clickedPoint.x >= umaPosition.x)
                {
                    if (clickedPoint.z <= umaPosition.z)
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione    
                        Vector3 newDestination = new Vector3(umaPosition.x - modificaDest + UnityEngine.Random.Range(-10, 10), umaPosition.y, umaPosition.z + modificaDest + UnityEngine.Random.Range(-10, 10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                    else
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione 
                        Vector3 newDestination = new Vector3(umaPosition.x - modificaDest + UnityEngine.Random.Range(-10, 10), umaPosition.y, umaPosition.z - modificaDest + UnityEngine.Random.Range(-10, 10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                }
                //se clicco a sinistra, gli uma andranno a destra 
                if (clickedPoint.x <= umaPosition.x)
                {
                    if (clickedPoint.z <= umaPosition.z)
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione 
                        Vector3 newDestination = new Vector3(umaPosition.x + modificaDest + UnityEngine.Random.Range(-10, 10), umaPosition.y, umaPosition.z + modificaDest + UnityEngine.Random.Range(-10, 10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                    else
                    {
                        MovimentoPerico();
                        // Calcola la nuova destinazione 
                        Vector3 newDestination = new Vector3(umaPosition.x + modificaDest + UnityEngine.Random.Range(-10, 10), umaPosition.y, umaPosition.z - modificaDest + UnityEngine.Random.Range(-10, 10));
                        navMeshAgent.SetDestination(newDestination);
                    }
                }
            }
        }
    }


    void MovimentoPerico()
    {
            foreach (Transform umaTransform in umaRandomAvatar.transform)
            {
                Animator animator = umaTransform.GetComponent<Animator>();
                NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();

                // Calcola la direzione del raggio di percezione in base alla rotazione dell'UMA
                Vector3 forwardDirection = umaTransform.forward;

                // Lunghezza massima del raggio di percezione
                float perceptionDistance = 2f;

                // Lancio del raggio di percezione
                RaycastHit hit;
                if (Physics.Raycast(umaTransform.position, forwardDirection, out hit, perceptionDistance))
                {
                    // Controlla se l'oggetto colpito è un UMA
                    if (hit.collider.CompareTag("Player") && animator.GetBool("IsInjury") == false)
                    {
                        // Calcola la distanza tra l'UMA corrente e l'UMA colpito dal raggio
                        float distanceToOtherUMA = Vector3.Distance(umaTransform.position, hit.collider.transform.position);

                    float targetSpeed = Mathf.Lerp(maxSpeedWalk, medSpeedRun, distanceToOtherUMA / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                    navMeshAgent.speed = targetSpeed * 3; // Imposta la velocità dell'UMA
                    animator.SetFloat("Speed", targetSpeed);
                    }
                }
                else if(animator.GetBool("IsInjury") == false)
            {
                    // Se il raggio non colpisce nulla, ripristina la velocità dell'UMA
                    animator.SetFloat("Speed", maxSpeedRun);
                    navMeshAgent.speed = maxSpeedRun * 2;// Assumi che originalSpeed sia la velocità originale dell'UMA prima di applicare eventuali rallentamenti
                }

            }
    }


    void RaggiungimentoDestinazione()
    {               
        foreach (Transform child in umaRandomAvatar.transform)
        {

            // Ottieni il componente NavMeshAgent dell'UMA
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            Animator animator = navMeshAgent.GetComponent<Animator>();

            if (animator.GetBool("IsDeath") == true)
            {
                navMeshAgent.speed = 0f;
                navMeshAgent.ResetPath();
            }

            // Controlla se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 2f && animator.GetBool("IsDeath") == false && animator.GetBool("IsStumple") == false )
            {
                animator.SetTrigger("InjuryWalkToIdle");
                animator.SetTrigger("InjuryRunToIdle");
                animator.SetTrigger("IdleTrigger");
                navMeshAgent.speed = 0f;                                      
            }
        }           
    }

    bool PermessoInciampo = false;
    void Inciampo()
    {
        if ( PermessoInciampo == false && counterInciampo > 13f)
        {
            // Lista per memorizzare tutti gli UMA e le loro posizioni
            List<Transform> allUMATransforms = new List<Transform>();

            // Popola la lista con tutte le posizioni degli UMA
            foreach (Transform child in umaRandomAvatar.transform)
            {
                Animator animator = child.GetComponent<Animator>();
                if (animator.GetBool("IsDeath") == false)
                {
                    allUMATransforms.Add(child);
                }

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
                        if (distance < 1.2f)
                        {
                            nearbyUMAs.Add(otherUMATransform);
                        }
                    }
                }

                // Conta il numero di UMA nelle vicinanze, escludendo l'UMA stesso
                int nearbyUMACount = nearbyUMAs.Count; // Sottrai 1 per escludere l'UMA corrente

                NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();

                // Se non ci sono UMA nelle vicinanze o al massimo 3, incrementa fino a 1
                if (nearbyUMACount >= 6 )
                {
                    // Genera un numero casuale da 1 a 5
                    int randomNumber = UnityEngine.Random.Range(1, 4);
                    // Verifica se il numero casuale è uguale a 4
                    if (randomNumber == 2 && counterInciampo > 13f)
                    {
                        animator.SetTrigger("StumbleTrigger");
                        navMeshAgent.speed = 0f;
                        animator.SetBool("IsStumple", true);
                        navMeshAgent.ResetPath();
                        PermessoInciampo = true;
                    }
                    if (randomNumber == 1)
                    {
                        int randomNumber1 = UnityEngine.Random.Range(1, 3);
                        if(randomNumber1 == 1)
                        {
                            animator.SetTrigger("InjuryRun");
                            animator.SetBool("IsInjury", true);
                            animator.SetFloat("Speed", 0.3f);
                            navMeshAgent.speed = 0.35f * 3;
                        }
                        else
                        {
                            animator.SetTrigger("InjuryWalk");
                            animator.SetBool("IsInjury", true);
                            animator.SetFloat("Speed", 0.3f);
                            navMeshAgent.speed = 0.35f * 3;
                        }
                    }
                }
            }
        }
    }

    // Genera una posizione casuale all'interno del navmesh
    Vector3 GetRandomNavmeshLocation()
    {
        NavMeshHit navHit;
        Vector3 randomPoint = Vector3.zero;
        if (NavMesh.SamplePosition(transform.position + UnityEngine.Random.insideUnitSphere * 13f, out navHit, 10f, NavMesh.AllAreas))
        {
            randomPoint = navHit.position;
        }
        return randomPoint;
    }
}
