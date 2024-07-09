using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum PlayerStates
{
       PlayerWaitBleConnection = 0,
       PlayerWaitFirstMeasure = 1,
       PlayerStart = 2,
       PlayerPlaying = 3,
       PlayserStop = 4
} ;

public class PlayerController : MonoBehaviour
{
    BleInterface _bleInterface = null;
    WheelSample last_left_event = null;
    WheelSample last_right_event = null;

    public Text speed_left_text;
    public Text speed_right_text;
    public Text distance_text;
    public Text speed_text;

    public Text message_text;

    public float _current_speed_left = 0.0f;
    public float _current_speed_right = 0.0f;
    public float delta_counter_left = 0.0f;
    public float delta_counter_right = 0.0f;

    PlayerStates _current_state;

    public float wheel_diameter_m = 0.030f;
    public float brake_coef = 0.1f;
    public float distance_wheel_m = 0.80f;
    public bool can_move = false;
    public float cumulative_distance = 0.0f;
    public float v, omega, theta;
    public float coef_test = 5.0f;


    public float last_distance = 0.0f;
    public float last_speed_calculation = 0.0f;
    public float speed_average = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        _current_state = PlayerStates.PlayerWaitBleConnection;
        _bleInterface = GetComponent<BleInterface>();
        _current_speed_right = 0.0f;
        _current_speed_left = 0.0f;
        last_left_event = null;
        last_right_event = null;
    }

    void updateMessage()
    {
        string s = "";
        switch (_current_state)
        {
            case PlayerStates.PlayerWaitBleConnection:
                s = "Wait Bluetooth connection...\n";
                s += "Wheel Left " + ( _bleInterface.IsLeftWheelConneted() ? "Ready" : "Waiting") + "\n";
                s += "Wheel Right " + (_bleInterface.IsRightWheelConnected() ? "Ready" : "Waiting");
                break;
            case PlayerStates.PlayerWaitFirstMeasure:
                s = "Roll a little bit\n";
                s += "Wheel Left " + (last_left_event != null ? "Ready" : "Waiting") + "\n";
                s += "Wheel Right " + (last_right_event != null ? "Ready" : "Waiting");
                break;
            case PlayerStates.PlayerStart:
                s = "Start";
                break;

            case PlayerStates.PlayerPlaying:
                break;
            case PlayerStates.PlayserStop:
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
        switch(_current_state)
        {
            case PlayerStates.PlayerWaitBleConnection:
                if(_bleInterface.IsLeftWheelConneted() && _bleInterface.IsRightWheelConnected())
                {
                    _current_state = PlayerStates.PlayerWaitFirstMeasure;
                }
                break;
            case PlayerStates.PlayerWaitFirstMeasure:
                if(last_left_event != null && last_right_event != null)
                {
                    _current_state = PlayerStates.PlayerStart;
                }
                break;
            case PlayerStates.PlayerStart:
                break;

            case PlayerStates.PlayerPlaying:
                break;
            case PlayerStates.PlayserStop:
                break;
        }
    }

    private void FixedUpdate()
    {
        //computeStates();
        //updateMessage();
      
    }

    void CalculateAverageSpeed()
    {
        if(Time.time - last_speed_calculation >= 1.0f)
        {
            speed_average = (cumulative_distance - last_distance) * 3.6f;
            last_distance = cumulative_distance;
            last_speed_calculation = Time.time;
        }
    }

    // Update is called once per frame
    void Update()
    {
        calculatePosition();
        CalculateAverageSpeed();
        speed_left_text.text = $"Speed L: {this._current_speed_left}";
        speed_right_text.text = $"Speed R: {this._current_speed_right}";
        distance_text.text = $"Dist: {this.cumulative_distance} m";
        speed_text.text = $"Speed: "+ this.speed_average.ToString("F2") +" km/h";
        if (can_move)
        {
            // Calcul du temps écoulé depuis la dernière frame de physique
            float elapsedTime = Time.deltaTime;

            // Calcul des nouvelles vitesses et angle
            (v, omega, theta) = CalculateVelocityAndAngle(delta_counter_left * Time.deltaTime * coef_test, delta_counter_right * Time.deltaTime * coef_test, distance_wheel_m, elapsedTime);
            delta_counter_left -= delta_counter_left * Time.deltaTime * coef_test;
            delta_counter_right -= delta_counter_right * Time.deltaTime * coef_test;
            // Mettre à jour la position du prefab
            //transform.Translate(Vector3.back * v * elapsedTime);
            transform.Translate(Vector3.back * v);
            cumulative_distance += v;

            // Mettre à jour la rotation du prefab
            transform.Rotate(Vector3.up, omega * Mathf.Rad2Deg);
        }

        //calculateBreak();
    }

    (float, float, float) CalculateVelocityAndAngle(float v_l, float v_r, float d, float t)
    {
        // Calcul de la vitesse linéaire moyenne
        float v = (v_l + v_r) / 2;

        // Calcul de la vitesse angulaire
        float omega = (v_r - v_l) / d;

        // Calcul de l'angle de rotation
        float theta = omega * t;

        return (v, omega, theta);
    }


    void calculateBreak()
    {
        //delta_counter_left = 0.0f;
        //delta_counter_right = 0.0f;

        delta_counter_left -= delta_counter_left * brake_coef * Time.deltaTime;
        //delta_counter_left = Mathf.Max(0, delta_counter_left);
        if(delta_counter_left < 0.1f)
        {
            delta_counter_left = 0.0f;
        }
        delta_counter_right -= delta_counter_right * brake_coef * Time.deltaTime;
        //delta_counter_right = Mathf.Max(0, delta_counter_right);
        if(delta_counter_right < 0.1f)
        {
            delta_counter_right = 0.0f;
        }


        //_current_speed_left -= _current_speed_left * brake_coef * Time.deltaTime;
        //_current_speed_left = Mathf.Max(0, _current_speed_left);
        //if (_current_speed_left < 0.1f)
        //{
        //    _current_speed_left = 0.0f;
        //}
        //_current_speed_right -= _current_speed_right * brake_coef * Time.deltaTime;
        //_current_speed_right = Mathf.Max(0, _current_speed_right);
        //if (_current_speed_right < 0.1f)
        //{
        //    _current_speed_right = 0.0f;
        //}
    }

    public void ResetSpeed()
    {
        _current_speed_left = 0.0f;
        _current_speed_right = 0.0f;
        delta_counter_left = 0.0f;
        delta_counter_right = 0.0f;
    }

    void calculatePosition()
    {
        WheelSample _left_event = _bleInterface.GetLastWheelLeftEvent();
        WheelSample _right_event = _bleInterface.GetLastWheelRightEvent();
        float speed_left = 0.0f;
        float speed_right = 0.0f;

        if(last_left_event == null)
        {
            last_left_event = _left_event;
            return;
        }
        if (last_right_event == null)
        {
            last_right_event = _right_event;
            return;
        }
        if (_left_event != null && last_left_event.timestamp != _left_event.timestamp)
        {
            if (last_left_event != null)
            {
                //int delta_t;
                //if (_left_event.timestamp > last_left_event.timestamp) {
                //    delta_t = _left_event.timestamp - last_left_event.timestamp;
                //} else
                //{
                //    delta_t = short.MaxValue - last_left_event.timestamp + (_left_event.timestamp - short.MinValue);
                //}
                int delta_counter = _left_event.counter - last_left_event.counter;
                delta_counter_left += delta_counter * wheel_diameter_m * Mathf.PI;

                //speed_left = 3.6f * delta_counter * wheel_diameter_m * Mathf.PI / delta_t;

                this._current_speed_left = speed_left;
                /* Save as last received event */
            }
            last_left_event = _left_event;
        }

        if (_right_event != null && last_right_event.timestamp != _right_event.timestamp)
        {
            if (last_right_event != null)
            {
                int delta_t;
                if (_right_event.timestamp > last_right_event.timestamp)
                {
                    delta_t = _right_event.timestamp - last_right_event.timestamp;
                }
                else
                {
                    delta_t = short.MaxValue - last_right_event.timestamp + (_right_event.timestamp - short.MinValue);
                }
                int delta_counter = _right_event.counter - last_right_event.counter;
                delta_counter_right += delta_counter * wheel_diameter_m * Mathf.PI;
               // speed_right = 3.6f * delta_counter * wheel_diameter_m * Mathf.PI / delta_t;

                this._current_speed_right = speed_right;
            }
            /* Save as last received event */
            last_right_event = _right_event;
        }
    }

    public WheelSample GetLastSampleLeft()
    {
        return last_left_event;
    }

    public WheelSample GetLastSampleRight()
    {
        return last_right_event;
    }

}
