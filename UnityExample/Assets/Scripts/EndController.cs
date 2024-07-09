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

    bool NotifyEnd()
    {
        Debug.Log("Notify End");
        return _gameControllerObj.GetComponent<GameController>().EndCallback();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wheels"))
        {
            if(NotifyEnd())
            {
                colliderComponent.enabled = false;
            }
            // Ajoutez ici le code pour ce qui doit se passer lorsque le joueur entre dans le trigger
        }
    }
}
