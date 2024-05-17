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
using System.IO;
public class MovimentoCorsaAlClick : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar;
    float minSpeedWalk = 1.27f; // Modificato per un rallentamento più marcato
    float medSpeedWalk = 1.45f;
    float maxSpeedWalk = 1.6f;
    bool CliccatoTastoDestro = false;
    float maxSpeedRun = 5.3f;
    float medSpeedRun = 4.5f;
    float minSpeedRun = 3f;
    float counterInciampo = 0f;


    void Start()
    {

    }

    void Update()
    {
        CaratteristicheUMA();


        if (CliccatoTastoDestro == false)
        {
            UpdateUMAAnimations();
            UpdateUMADestinations();
            counterPriorità += Time.deltaTime;
        }
        else
        {
            RaggiungimentoDestinazione();
            Inciampo();
            MovimentoPerico();
            counterInciampo += Time.deltaTime;
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
            navMeshAgent.radius = 0.25f;
            navMeshAgent.acceleration = 8f;
            navMeshAgent.angularSpeed = 180f;
            // Genera un livello di priorità random tra 1 e 100
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
    }

    bool accessoPriorità= false;
    float counterPriorità;
    void UpdateUMAAnimations()
    {

        foreach (Transform umaTransform in umaRandomAvatar.transform)
        {
            Animator animator = umaTransform.GetComponent<Animator>();
            NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();
            Rigidbody rigidbody = umaTransform.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                Destroy(rigidbody);
            }
            // Lista per memorizzare le distanze dagli altri UMA
            List<float> distancesToOtherUMAs = new List<float>();

            // Calcola la distanza dagli altri UMA
            foreach (Transform otherUMATransform in umaRandomAvatar.transform)
            {
                if (otherUMATransform != umaTransform)
                {
                    // Calcola il vettore dalla posizione dell'UMA corrente all'altro UMA
                    Vector3 toOtherUMA = otherUMATransform.position - umaTransform.position;

                    // Calcola l'angolo tra il vettore forward dell'UMA corrente e il vettore toOtherUMA
                    float angle = Vector3.Angle(umaTransform.forward, toOtherUMA);

                    // Filtra gli UMA basati sull'angolo per includere solo quelli posizionati davanti all'UMA corrente
                    if (angle < 50f) // Ad esempio, considera solo gli UMA posizionati entro un angolo di 90 gradi rispetto all'UMA corrente
                    {
                        // Calcola la distanza tra l'UMA corrente e l'altro UMA
                        float distance = toOtherUMA.magnitude;
                        distancesToOtherUMAs.Add(distance);
                    }
                }
            }
            if (counterPriorità > 3f)
            {
                accessoPriorità = false;
            }
            // Calcola la distanza minima dagli altri UMA
            float minDistanceToOtherUMAs = distancesToOtherUMAs.Count > 0 ? distancesToOtherUMAs.Min() : Mathf.Infinity;

            // Se la distanza minima è inferiore a un certo valore, rallenta l'UMA
            if (minDistanceToOtherUMAs <= 0.7f) // Imposta il valore appropriato
            {
                
                
                float targetSpeed = Mathf.Lerp(minSpeedWalk, medSpeedWalk, minDistanceToOtherUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = 0.8f; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
                if (accessoPriorità == false )
                {

                    navMeshAgent.avoidancePriority = UnityEngine.Random.Range(20, 50);
                    accessoPriorità = true;
                    counterPriorità = 0f;
                }
                
            }
            else
            {
                float targetSpeed = Mathf.Lerp(medSpeedWalk, maxSpeedWalk, minDistanceToOtherUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = 1f; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
            }
        }
    }


    Vector3 center = new Vector3(0f, 0f, 0f);
    float radius = 10f;
    void UpdateUMADestinations()
    {
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
        
            // Calcola la distanza rimanente alla destinazione
            float distanceToDestination = navMeshAgent.remainingDistance;
            // Se l'UMA ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && distanceToDestination < 1f)
            {
                // Ottieni la nuova destinazione casuale
                Vector3 randomDestination = GetRandomNavmeshLocation(center, radius);
                // Imposta la nuova destinazione
                navMeshAgent.SetDestination(randomDestination);
               
            }
        }
    }


    void DeathAnimation()
    {
        CliccatoTastoDestro = true;
        // Lanciare un raggio dal punto in cui è stato cliccato
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        // Se il raggio colpisce un oggetto sulla NavMesh
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {

            // Ottieni le coordinate del punto colpito
            Vector3 clickedPoint = hit.point;

            Debug.Log(clickedPoint);
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

            FleeFromPoint(clickedPoint);
        }
    }

    Vector3 CalculateFleeVector(Vector3 umaPosition, Vector3 clickPoint)
    {

        if (clickPoint.y > 0.5)
        {
            //Vector3 newclickPoint = new (clickPoint.x,0,clickPoint.z);
            // Calcola il vettore distanza dal UMA al punto cliccato
            Vector3 fleeVector = clickPoint - umaPosition;
            fleeVector.y = 0f;
            fleeVector = fleeVector.normalized;

            // Inverti il vettore di 180 gradi moltiplicandolo per -1
            fleeVector *= -1;

            // Rendi il vettore 
            fleeVector *= 30f;

            // Calcola il punto finale del vettore
            Vector3 newDestination = umaPosition + fleeVector;

            return newDestination;
        }
        else
        {
            Vector3 fleeVector = clickPoint - umaPosition;
            fleeVector.y = 0;
            fleeVector = fleeVector.normalized;

            // Inverti il vettore di 180 gradi moltiplicandolo per -1
            fleeVector *= -1;

            // Rendi il vettore 
            fleeVector *= 30f;
            // Calcola il punto finale del vettore
            Vector3 newDestination = umaPosition + fleeVector;

            return newDestination;
        }

    }

    void FleeFromPoint(Vector3 clickPoint)
    {
        foreach (Transform umaTransform in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();

            Vector3 umaPosition = umaTransform.position;

            // Calcola la nuova destinazione per l'UMA corrente
            Vector3 newDestination = CalculateFleeVector(umaPosition, clickPoint);
            //navMeshAgent.SetDestination(newDestination);

            // posizione sulla navmesh più vicina 
            NavMeshHit hit;
            NavMesh.SamplePosition(newDestination, out hit, 50f, NavMesh.AllAreas);
            //Calcola il percorso tra l'attuale posizione dell'UMA e la nuova destinazione
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(umaPosition, hit.position, NavMesh.AllAreas, path))
            {
                // Imposta il percorso calcolato come percorso dell'agente NavMesh
                navMeshAgent.SetPath(path);
            }
        }
    }


    void MovimentoPerico()
    {
        foreach (Transform umaTransform in umaRandomAvatar.transform)
        {
            Animator animator = umaTransform.GetComponent<Animator>();
            NavMeshAgent navMeshAgent = umaTransform.GetComponent<NavMeshAgent>();
            Rigidbody rigidbody = umaTransform.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                Destroy(rigidbody);
            }

            navMeshAgent.acceleration = 12f;
            navMeshAgent.angularSpeed = 360f;
            // Lista per memorizzare le distanze dagli altri UMA posizionati davanti
            List<float> distancesToFrontUMAs = new List<float>();

            // Calcola la distanza dagli altri UMA posizionati davanti
            foreach (Transform otherUMATransform in umaRandomAvatar.transform)
            {
                if (otherUMATransform != umaTransform)
                {
                    // Calcola il vettore dalla posizione dell'UMA corrente all'altro UMA
                    Vector3 toOtherUMA = otherUMATransform.position - umaTransform.position;

                    // Calcola l'angolo tra il vettore forward dell'UMA corrente e il vettore toOtherUMA
                    float angle = Vector3.Angle(umaTransform.forward, toOtherUMA);

                    // Filtra gli UMA basati sull'angolo per includere solo quelli posizionati davanti all'UMA corrente
                    if (angle < 50f) // Ad esempio, considera solo gli UMA posizionati entro un angolo di 90 gradi rispetto all'UMA corrente
                    {
                        // Calcola la distanza tra l'UMA corrente e l'altro UMA
                        float distance = toOtherUMA.magnitude;
                        distancesToFrontUMAs.Add(distance);
                    }
                }
            }

            // Calcola la distanza minima dagli altri UMA posizionati davanti
            float minDistanceToFrontUMAs = distancesToFrontUMAs.Count > 0 ? distancesToFrontUMAs.Min() : Mathf.Infinity;

            // Se la distanza minima è inferiore a un certo valore, rallenta l'UMA
            if (minDistanceToFrontUMAs <= 0.4f && animator.GetBool("IsInjury") == false) // Imposta il valore appropriato
            {
                float targetSpeed = Mathf.Lerp(medSpeedWalk, maxSpeedWalk, minDistanceToFrontUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = 2.5f; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
            }
            else if (minDistanceToFrontUMAs > 0.4f && minDistanceToFrontUMAs <= 0.6f && animator.GetBool("IsInjury") == false)
            {
                float targetSpeed = Mathf.Lerp(maxSpeedWalk, minSpeedRun, minDistanceToFrontUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = 2.5f; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
            }
            else if (minDistanceToFrontUMAs > 0.6f && minDistanceToFrontUMAs <= 1f && animator.GetBool("IsInjury") == false)
            {
                float targetSpeed = Mathf.Lerp(minSpeedRun, medSpeedRun, minDistanceToFrontUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = 2.5f; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
            }
            else if (minDistanceToFrontUMAs > 1f && animator.GetBool("IsInjury") == false)
            {
                float targetSpeed = Mathf.Lerp(medSpeedRun, maxSpeedRun, minDistanceToFrontUMAs / 2.0f); // Regola i valori di Lerp in base alla distanza minima desiderata
                navMeshAgent.speed = 2.5f; // Imposta la velocità dell'UMA
                animator.SetFloat("Speed", targetSpeed); // Imposta la velocità dell'animator
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
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 2f && animator.GetBool("IsDeath") == false && animator.GetBool("IsStumple") == false)
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
        if (PermessoInciampo == false && counterInciampo > 5f)
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
                if (nearbyUMACount >= 5)
                {
                    // Genera un numero casuale da 1 a 5
                    int randomNumber = UnityEngine.Random.Range(1, 4);
                    // Verifica se il numero casuale è uguale a 4
                    if (randomNumber == 2 && counterInciampo > 5f)
                    {
                        CapsuleCollider capsulecollider = umaTransform.GetComponent<CapsuleCollider>();
                        Destroy(capsulecollider);
                        animator.SetTrigger("StumbleTrigger");
                        navMeshAgent.speed = 0f;
                        animator.SetBool("IsStumple", true);
                        navMeshAgent.ResetPath();
                        PermessoInciampo = true;

                    }
                    if (randomNumber == 1)
                    {
                        int randomNumber1 = UnityEngine.Random.Range(1, 3);
                        if (randomNumber1 == 1)
                        {
                            animator.SetTrigger("InjuryRun");
                            animator.SetBool("IsInjury", true);
                            animator.SetFloat("Speed", 1.4f);
                            navMeshAgent.speed = 1f;
                        }
                        else
                        {
                            animator.SetTrigger("InjuryWalk");
                            animator.SetBool("IsInjury", true);
                            animator.SetFloat("Speed", 1.6f);
                            navMeshAgent.speed = 1f;
                        }
                    }
                }
            }
        }
    }

    // Genera una posizione casuale all'interno del navmesh, entro una sfera centrata in un punto arbitrario
    Vector3 GetRandomNavmeshLocation(Vector3 center, float radius)
    {
        NavMeshHit navHit;
        Vector3 randomPoint = Vector3.zero;
        Vector3 randomOffset = center + UnityEngine.Random.insideUnitSphere * radius;

        // Tenta di campionare una posizione all'interno del NavMesh vicino al punto casuale
        if (NavMesh.SamplePosition(randomOffset, out navHit, radius, NavMesh.AllAreas))
        {
            randomPoint = navHit.position;
        }
        else
        {
            // Se non riesce, prova a trovare una posizione nelle vicinanze del punto casuale
            randomPoint = FindNearestNavmeshPoint(randomOffset);
        }

        return randomPoint;
    }

    // Trova la posizione più vicina nel NavMesh
    Vector3 FindNearestNavmeshPoint(Vector3 position)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(position, out navHit, 10f, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        else
        {
            // Se non riesce a trovare una posizione valida, restituisci il punto originale
            return position;
        }
    }


}
