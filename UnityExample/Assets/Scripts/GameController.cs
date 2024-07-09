using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GameStates
{
    GameInit = 0,
    GameWaitBleConnection,
    GameWaitFirstMeasure,
    GameWaitStart,
    GameStart3,
    GameStart2,
    GameStart1,
    GamePlaying1,
    GamePlaying2,
    GameStop,
    GameStopWaiting
};

public class GameController : MonoBehaviour
{
    public Text message_text;
    public Text checkpoint_text;

    public GameStates _current_state = GameStates.GameInit;
    public GameStates _next_state = GameStates.GameInit;
    public List<GameObject> checkpoint_list;

    BleInterface _bleInterface = null;
    PlayerController _playerController = null;
    Chrono _chrono = null;

    GameObject player = null;

    GameObject lastCheckpoint = null;
    int numberCheckpointDone = 0;


    bool needRespawn = false;

    float currentTimestamp;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        _bleInterface = player.GetComponent<BleInterface>();
        _playerController = player.GetComponent<PlayerController>();
        _chrono = GetComponent<Chrono>();
        checkpoint_text.enabled = false;

        _current_state = GameStates.GameInit;

    }

    // Update is called once per frame
    void Update()
    {
        updateMessage();
        UpdateCheckpointCounter();
        computeStates();
        GoToNextState();
    }

    void updateMessage()
    {
        string s = "";
        switch (_current_state)
        {
            case GameStates.GameInit:
                break;
            case GameStates.GameWaitBleConnection:
                s = "Wait Bluetooth connection...\n";
                s += "Wheel Left " + (_bleInterface.IsLeftWheelConneted() ? "Ready" : "Waiting") + "\n";
                s += "Wheel Right " + (_bleInterface.IsRightWheelConnected() ? "Ready" : "Waiting");
                break;
            case GameStates.GameWaitFirstMeasure:
                s = "Roll a little bit\n";
                s += "Wheel Left " + (_playerController.GetLastSampleLeft() != null ? "Ready" : "Waiting") + "\n";
                s += "Wheel Right " + (_playerController.GetLastSampleRight() != null ? "Ready" : "Waiting");
                break;
            case GameStates.GameWaitStart:
                s = "Be ready...";
                break;
            case GameStates.GameStart3:
                s = "3";
                break;
            case GameStates.GameStart2:
                s = "2";
                break;
            case GameStates.GameStart1:
                s = "1";
                break;
            case GameStates.GamePlaying1:
                s = "Go";
                break;
            case GameStates.GamePlaying2:
                break;
            case GameStates.GameStop:
            case GameStates.GameStopWaiting:
                float elapsed_time = _chrono.elapsed_time;
                int minutes = Mathf.FloorToInt(elapsed_time / 60F);
                int seconds = Mathf.FloorToInt(elapsed_time % 60F);
                int milliseconds = Mathf.FloorToInt((elapsed_time * 1000F) % 1000F);
                s = "FINISH !\n";
                s += "Your time : ";
                s += string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                break;
        }
        SetMessageText(s);
    }

    void SetMessageText(string s)
    {
        message_text.text = s;
    }

    void UpdateCheckpointCounter()
    {
        checkpoint_text.text = $"{numberCheckpointDone} / {checkpoint_list.Count}";
    }

    void computeStates()
    {
        switch (_current_state)
        {
            case GameStates.GameInit:
                _bleInterface.SetRightLed(0x00, 0x00, 0x00);
                _bleInterface.SetLeftLed(0x00, 0x00, 0x00);
                _next_state = GameStates.GameWaitBleConnection;
                break;
            case GameStates.GameWaitBleConnection:
                if (_bleInterface.IsLeftWheelConneted() && _bleInterface.IsRightWheelConnected() && Time.frameCount > 100)
                {
                    _next_state = GameStates.GameWaitFirstMeasure;
                }
                break;
            case GameStates.GameWaitFirstMeasure:
                if (_playerController.GetLastSampleLeft() != null && _playerController.GetLastSampleRight() != null)
                {
                    currentTimestamp = Time.time;
                    //_bleInterface.SetRightLed(0xFF, 0x00, 0x00);
                    //_bleInterface.SetLeftLed(0xFF, 0x00, 0x00);

                    _next_state = GameStates.GameWaitStart;
                }
                break;
            case GameStates.GameWaitStart:
                if (Time.time - currentTimestamp >= 3.0)
                {
                    currentTimestamp = Time.time;

                    _next_state = GameStates.GameStart3;

                }
                break;
            case GameStates.GameStart3:
                if(Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0xFF, 0x00, 0x00);
                    _bleInterface.SetLeftLed(0xFF, 0x00, 0x00);

                    _next_state = GameStates.GameStart2;

                }
                break;
            case GameStates.GameStart2:
                if (Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0xFF, 90, 0x00);
                    _bleInterface.SetLeftLed(0xFF, 90, 0x00);

                    _next_state = GameStates.GameStart1;

                }
                break;
            case GameStates.GameStart1:
                if (Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0x00, 0xFF, 0x00);
                    _bleInterface.SetLeftLed(0x00, 0xFF, 0x00);

                    checkpoint_text.enabled = true;
                    _playerController.ResetSpeed();
                    _playerController.can_move = true;
                    _next_state = GameStates.GamePlaying1;
                    _chrono.SetVisible(true);
                    _chrono.StartChrono();

                }

                break;

            case GameStates.GamePlaying1:
                CheckRespawn();
                if (Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0x00, 0x00, 0x00);
                    _bleInterface.SetLeftLed(0x00, 0x00, 0x00);

                    _next_state = GameStates.GamePlaying2;
                }
                break;
            case GameStates.GamePlaying2:
                CheckRespawn();
                break;
            case GameStates.GameStop:
                _playerController.can_move = false;
                checkpoint_text.enabled = false;
                _chrono.StopChrono();
                _bleInterface.PlayLedSequence(LedSequence.LED_FINISH);
                _next_state = GameStates.GameStopWaiting;
                break;
            case GameStates.GameStopWaiting:
                break;
        }
    }

    int GetCountCheckpointDone()
    {
        int count = 0;
        foreach (GameObject obj in checkpoint_list)
        {
            if(obj.GetComponent<CheckpointController>().IsCheckpointTriggered())
            {
                count++;
            }
        }
        return count;
    }

    void CheckRespawn()
    {
        if (needRespawn || Input.GetKeyDown(KeyCode.Space))
        {
            if (lastCheckpoint != null)
            {
                Debug.Log("RESPAWNNNN");
                needRespawn = false;
                player.transform.rotation = lastCheckpoint.transform.rotation;
                player.transform.Rotate(Vector3.up, 180);
                player.transform.position = lastCheckpoint.transform.position;
                player.GetComponent<PlayerController>().ResetSpeed();
                _bleInterface.PlayLedSequence(LedSequence.LED_RESPAWN);
            }
        }
    }

    public bool EndCallback()
    {
        if(GetCountCheckpointDone() != checkpoint_list.Count)
        {
            return false;
        }

        _next_state = GameStates.GameStop;
        return true;
    }

    public void NotifyCheckpoint(int id, GameObject obj)
    {
        Debug.Log("Entry checkpoint " + id);
        lastCheckpoint = obj;
        numberCheckpointDone = GetCountCheckpointDone();
        _bleInterface.PlayLedSequence(LedSequence.LED_CHECKPOINT);
    }

    void GoToNextState()
    {
        if(_next_state != _current_state)
        {
            _current_state = _next_state;
        }
    }

    public void NotifyOufOfRoad()
    {
        needRespawn = true;
    }
}
