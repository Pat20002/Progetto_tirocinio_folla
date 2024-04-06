using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movimento : MonoBehaviour
{
    //definisco variabile privata di tipo NavMeshAgent chiamata agent, componente che gestirà il movimentodel personaggio
    private NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        //ottengo il componente NavMeshAgent associato al GameObject
        agent = GetComponent<NavMeshAgent>();   
    }

    // Update is called once per frame
    void Update()
    {
        //controllo che l'utente abbia cliccato con il tasto sinistro del mouse
        if (Input.GetMouseButtonDown(0)) 
        { 
            //crea un raggio dal punto in cui l'utente ha cliccato sullo schermo nella direzione della telecamera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // lancia il raggio e controlla se ha colpito qualcosa
            if (Physics.Raycast(ray, out hit))
            {
                // se ha colpito qualcosa si imposta il punto di destinazione del NavMeshAgent alla posizione colpita dal raggio
                agent.SetDestination(hit.point);
            }
        }
    }
}
