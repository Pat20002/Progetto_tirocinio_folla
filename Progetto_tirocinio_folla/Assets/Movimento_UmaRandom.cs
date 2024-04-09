using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
using UnityEngine.AI;

public class ScriptRandomMovimento : MonoBehaviour
{
    public UMARandomAvatar umaRandomAvatar; // Riferimento a UMARandomAvatar
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    void Start()
    {
       
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
                // Itera attraverso ogni UMA generato
                foreach (Transform child in umaRandomAvatar.transform)
                {
                    // Ottieni il componente NavMeshAgent dell'UMA
                    NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();

                    // Imposta la destinazione per il NavMeshAgent
                    navMeshAgent.SetDestination(hit.point);

                    // Ottieni il componente Animator dell'UMA
                    Animator animator = child.GetComponent<Animator>();

                    // Imposta il parametro "Speed" dell'Animator a 1 per farlo correre
                    animator.SetFloat("Speed", 1);

                    // Imposta il parametro in animator "Culling Mode" per ognuno degli UMA a Cull Update Transform
                    animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                }
            }
        }

        // Controllo se l'UMA è in movimento e ha raggiunto la destinazione
        foreach (Transform child in umaRandomAvatar.transform)
        {
            NavMeshAgent navMeshAgent = child.GetComponent<NavMeshAgent>();
            Animator animator = child.GetComponent<Animator>();

            // Se l'UMA è in movimento e ha raggiunto la destinazione
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                // Imposta il parametro "Speed" dell'Animator a 0 per fermare l'animazione della corsa
                animator.SetFloat("Speed", 0);
            }
        }
    }

}


