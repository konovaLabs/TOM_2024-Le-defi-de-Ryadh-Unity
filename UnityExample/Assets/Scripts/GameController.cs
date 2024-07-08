using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GameStates
{
    GameWaitBleConnection = 0,
    GameWaitFirstMeasure = 1,
    GameStart3 = 2,
    GameStart2 = 3,
    GameStart1 = 4,
    GamePlaying1 = 5,
    GamePlaying2 = 6,
    GameStop = 7
};

public class GameController : MonoBehaviour
{
    public Text message_text;

    GameStates _current_state;

    BleInterface _bleInterface = null;
    PlayerController _playerController = null;

    GameObject player = null;

    float currentTimestamp;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        _bleInterface = player.GetComponent<BleInterface>();
        _playerController = player.GetComponent<PlayerController>();

        _current_state = GameStates.GameWaitBleConnection;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        computeStates();
        updateMessage();
    }

    void updateMessage()
    {
        string s = "";
        switch (_current_state)
        {
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
                break;
        }
        SetMessageText(s);
    }

    void SetMessageText(string s)
    {
        message_text.text = s;
    }

    void computeStates()
    {
        switch (_current_state)
        {
            case GameStates.GameWaitBleConnection:
                if (_bleInterface.IsLeftWheelConneted() && _bleInterface.IsRightWheelConnected() && Time.frameCount > 100)
                {
                    _bleInterface.SetRightLed(0x00, 0x00, 0x00);
                    _bleInterface.SetLeftLed(0x00, 0x00, 0x00);
                    _current_state = GameStates.GameWaitFirstMeasure;
                }
                break;
            case GameStates.GameWaitFirstMeasure:
                if (_playerController.GetLastSampleLeft() != null && _playerController.GetLastSampleRight() != null)
                {
                    currentTimestamp = Time.time;
                    //_bleInterface.SetRightLed(0xFF, 0x00, 0x00);
                    //_bleInterface.SetLeftLed(0xFF, 0x00, 0x00);

                    _current_state = GameStates.GameStart3;
                }
                break;
            case GameStates.GameStart3:
                if(Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0xFF, 0x00, 0x00);
                    _bleInterface.SetLeftLed(0xFF, 0x00, 0x00);

                    _current_state = GameStates.GameStart2;

                }
                break;
            case GameStates.GameStart2:
                if (Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0xFF, 90, 0x00);
                    _bleInterface.SetLeftLed(0xFF, 90, 0x00);

                    _current_state = GameStates.GameStart1;

                }
                break;
            case GameStates.GameStart1:
                if (Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0x00, 0xFF, 0x00);
                    _bleInterface.SetLeftLed(0x00, 0xFF, 0x00);

                    _playerController.can_move = true;
                    _current_state = GameStates.GamePlaying1;

                }

                break;

            case GameStates.GamePlaying1:
                if(Time.time - currentTimestamp >= 1.0)
                {
                    currentTimestamp = Time.time;

                    _bleInterface.SetRightLed(0x00, 0x00, 0x00);
                    _bleInterface.SetLeftLed(0x00, 0x00, 0x00);

                    _current_state = GameStates.GamePlaying2;
                }
                break;
            case GameStates.GamePlaying2:
                break;
            case GameStates.GameStop:
                _playerController.can_move = false;
                break;
        }
    }

}
