using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    GameObject _gameControllerObj = null;
    private Collider colliderComponent;
    public int id;

    // Start is called before the first frame update
    void Start()
    {
        colliderComponent = GetComponent<Collider>();
        _gameControllerObj = GameObject.Find("GameControllerObj");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void NotifyCheckpointEntry()
    {
        Debug.Log("Notify End");
        _gameControllerObj.GetComponent<GameController>().NotifyCheckpoint(id, gameObject);
    }


    void OnTriggerEnter(Collider other)
    {
        // Vérifier si l'objet entrant a le tag "Player"
        if (other.CompareTag("Wheels"))
        {
            if (colliderComponent != null)
            {
                colliderComponent.enabled = false;
            }
            Debug.Log("Player entered the trigger zone!");

            NotifyCheckpointEntry();
        }
    }
}
