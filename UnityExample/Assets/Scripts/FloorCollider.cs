using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCollider : MonoBehaviour
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

    void NotifyOutOfRoad()
    {
        Debug.Log("Notify End");
        _gameControllerObj.GetComponent<GameController>().NotifyOufOfRoad();
    }

    void OnTriggerEnter(Collider other)
    {
        // Vérifier si l'objet entrant a le tag "Player"
        if (other.CompareTag("Wheels"))
        {
            NotifyOutOfRoad();
        }
    }
}
