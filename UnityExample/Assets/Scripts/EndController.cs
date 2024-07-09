using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndController : MonoBehaviour
{

    GameObject _gameControllerObj = null;
    private Collider colliderComponent;

    // Start is called before the first frame update
    void Start()
    {
        _gameControllerObj = GameObject.Find("GameControllerObj");
        colliderComponent = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NotifyEnd()
    {
        Debug.Log("Notify End");
        _gameControllerObj.GetComponent<GameController>().EndCallback();
    }

    void OnTriggerEnter(Collider other)
    {
        // Vérifier si l'objet entrant a le tag "Player"
        Debug.Log("Collider" + other.tag);
        if (other.CompareTag("Wheels"))
        {
            if (colliderComponent != null)
            {
                colliderComponent.enabled = false;
            }
            Debug.Log("Player entered the trigger zone!");
            NotifyEnd();
            // Ajoutez ici le code pour ce qui doit se passer lorsque le joueur entre dans le trigger
        }
    }
}
