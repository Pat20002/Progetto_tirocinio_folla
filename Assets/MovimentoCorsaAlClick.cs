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
    float detectionRadius = 2.5f;
    private float maxTimeToDestination = 10f; // Tempo massimo consentito per raggiungere la destinazione
    private float currentTimeToDestination = 0f; // Timer per tenere traccia del tempo trascorso
    bool CliccatoTastoSinistro = false;
    private float modificaDest = 15f;
    float maxSpeedRun = 1f;
    float medSpeedRun = 0.80f;
    float minSpeedRun = 0.65f;
    float detectionRadiusRun = 2.5f;

    void Start()
    {

    }

    void Update()
    {
        CaratteristicheUMA();
        

        if (CliccatoTastoSinistro == false)
        {
            UpdateUMAAnimations();
            UpdateUMADestinations();
        }

        // Se viene cliccato il tasto destro del mouse
        if (Input.GetMouseButtonDown(1))
        {
            DeathAnimation();
        }
        RaggiungimentoDestinazione();

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

    void CaratteristicheUMA()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            child.tag = "Player";

            // Imposta il raggio e la distanza di sosta del NavMeshAgent
            navMeshAgent.radius = 0.1f;
            navMeshAgent.acceleration = 3f;
            navMeshAgent.angularSpeed = 180f;

            // Genera un livello di priorità random tra 1 e 100
            navMeshAgent.avoidancePriority = Random.Range(1, 100);
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
        CliccatoTastoSinistro = true;

        // Lanciare un raggio dal punto in cui è stato cliccato
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Se il raggio colpisce un oggetto sulla NavMesh
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {

            // Ottieni le coordinate del punto colpito
            Vector3 clickedPoint = hit.point;
            Debug.Log("Coordinate del punto colpito: " + clickedPoint);
            
            // Crea un collider sferico intorno al punto cliccato
            Collider[] colliders = Physics.OverlapSphere(clickedPoint, 1.0f); // 2.0f è il raggio dell'area

            // Itera attraverso gli UMA all'interno dell'area
            foreach (Collider collider in colliders)
            {
                NavMeshAgent navMeshAgent = collider.GetComponent<NavMeshAgent>();
                Animator animator = collider.GetComponent<Animator>();
                // Verifica se il collider appartiene a un UMA
                if (collider.CompareTag("Player"))
                {
                    // Attiva l'animazione di morte
                    animator.SetTrigger("TriggerDeathF");
                    navMeshAgent.ResetPath();
                    navMeshAgent.speed = 0f;
                    animator.SetBool("IsDeath", true);
                    
                    //}
                    //}
                    // Visualizza l'oggetto visivo (sfera) intorno al punto cliccato per rappresentare il collider sferico
                    //GameObject sphereVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphereVisual.transform.position = clickedPoint;
                    //sphereVisual.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f); // Dimensiona la sfera visiva
                    //Renderer sphereRenderer = sphereVisual.GetComponent<Renderer>();
                    //sphereRenderer.material.color = Color.blue; // Imposta il colore della sfera visiva

                }
                    
            }
            foreach (Transform child in umaRandomAvatar.transform)
            {
                NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
                Animator animator = navMeshAgent.GetComponent<Animator>();
                if (animator.GetBool("IsDeath") == false)
                {
                    Vector3 umaPosition = child.position;

                    // Se clicco in alto, gli uma andranno in basso
                    if (clickedPoint.z >= umaPosition.z && animator.GetBool("IsDeath") == false)
                    {
                        if (clickedPoint.x <= umaPosition.x)
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione
                            Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                        else
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione 
                            Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                    }
                    // se clicco in basso, gli uma andranno in alto
                    if (clickedPoint.z <= umaPosition.z && animator.GetBool("IsDeath") == false)
                    {
                        if (clickedPoint.x <= umaPosition.x)
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione
                            Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                        else
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione 
                            Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                    }
                    // se clicco a destra, gli uma andranno a sinistra
                    if (clickedPoint.x >= umaPosition.x && animator.GetBool("IsDeath") == false)
                    {
                        if (clickedPoint.z <= umaPosition.z)
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione    
                            Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                        else
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione 
                            Vector3 newDestination = new Vector3(umaPosition.x - modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                    }
                    //se clicco a sinistra, gli uma andranno a destra 
                    if (clickedPoint.x <= umaPosition.x && animator.GetBool("IsDeath") == false)
                    {
                        if (clickedPoint.z <= umaPosition.z)
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione 
                            Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z + modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                        else
                        {
                            MovimentoPerico();
                            // Calcola la nuova destinazione 
                            Vector3 newDestination = new Vector3(umaPosition.x + modificaDest, umaPosition.y, umaPosition.z - modificaDest);
                            navMeshAgent.SetDestination(newDestination);
                        }
                    }
                }

            }

        }

    }

    void MovimentoPerico()
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
                    if (distance < detectionRadiusRun)
                    {
                        nearbyUMAs.Add(otherUMATransform);
                    }
                }
            }

            // Conta il numero di UMA nelle vicinanze, escludendo l'UMA stesso
            int nearbyUMACount = nearbyUMAs.Count; // Sottrai 1 per escludere l'UMA corrente

            NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();

            // Se non ci sono UMA nelle vicinanze o al massimo 3, incrementa fino a 1
            if (nearbyUMACount >= 0 && nearbyUMACount <= 3)
            {

                float targetSpeed = Mathf.Lerp(0.2f, maxSpeedRun, Mathf.Clamp01(Time.time)); 
                animator.SetFloat("Speed", targetSpeed);
                navMeshAgent.speed = targetSpeed * 3;
            }
            else if (nearbyUMACount > 3 && nearbyUMACount <= 6)
            {
                //Rallenta fino alla MedSpeedRun
                float targetSpeed = Mathf.Lerp(maxSpeedRun,medSpeedRun, Mathf.Clamp01((nearbyUMACount - 3) / 10f));
                // Applica il nuovo speed all'animator
                animator.SetFloat("Speed", targetSpeed);
                navMeshAgent.speed = targetSpeed * 3;
            }
            else
            {
                // Rallenta fino ala MinSpeedRun
                float targetSpeed = Mathf.Lerp(medSpeedRun, minSpeedRun, Mathf.Clamp01((nearbyUMACount - 4) / 10f));
                // Applica il nuovo speed all'animator
                animator.SetFloat("Speed", targetSpeed);
                navMeshAgent.speed = targetSpeed * 3;
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

            // Controlla se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 1f && animator.GetBool("IsDeath") == false && CliccatoTastoSinistro == true)
            {
                animator.SetTrigger("IdleTrigger");
                navMeshAgent.speed = 0f;
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



