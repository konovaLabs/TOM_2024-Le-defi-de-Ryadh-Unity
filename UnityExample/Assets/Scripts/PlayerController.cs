using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum PlayerStates
{
       PlayerWaitBleConnection = 0,
       PlayerWaitFirstMeasure = 1,
       PlayerPlaying = 2,
       PlayserStop = 3
} ;

public class PlayerController : MonoBehaviour
{
    BleInterface _bleInterface = null;
    WheelSample last_left_event = null;
    WheelSample last_right_event = null;

    public Text speed_left_text;
    public Text speed_right_text;

    public float _current_speed_left = 0.0f;
    public float _current_speed_right = 0.0f;

    PlayerStates _current_state;

    public float wheel_diameter_mm = 50.0f;
    public float brake_coef = 0.1f;
    public float distance_wheel_m = 0.80f;
    public bool can_move = false;

    public float v, omega, theta;

    // Start is called before the first frame update
    void Start()
    {
        _current_state = PlayerStates.PlayerWaitBleConnection;
        _bleInterface = GetComponent<BleInterface>();
    }

    private void FixedUpdate()
    {
        calculatePosition();
        //calculateBreak();

        if(can_move)
        {
            // Calcul du temps écoulé depuis la dernière frame de physique
            float elapsedTime = Time.fixedDeltaTime;

            // Calcul des nouvelles vitesses et angle
            (v, omega, theta) = CalculateVelocityAndAngle(_current_speed_left, _current_speed_right, distance_wheel_m, elapsedTime);

            // Mettre à jour la position du prefab
            transform.Translate(Vector3.forward * v * elapsedTime);

            // Mettre à jour la rotation du prefab
            transform.Rotate(Vector3.up, omega * elapsedTime * Mathf.Rad2Deg);
        }
        }

    // Update is called once per frame
    void Update()
    {
        speed_left_text.text = $"Speed L: {this._current_speed_left}";
        speed_right_text.text = $"Speed R: {this._current_speed_right}";
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
        _current_speed_left -= _current_speed_left * brake_coef * Time.deltaTime;
        _current_speed_left = Mathf.Min(0, _current_speed_left);
        _current_speed_right -= _current_speed_right * brake_coef * Time.deltaTime;
        _current_speed_right = Mathf.Min(0, _current_speed_right);
    }

    void calculatePosition()
    {
        WheelSample _left_event = _bleInterface.getLastWheelLeftEvent();
        WheelSample _right_event = _bleInterface.getLastWheelRightEvent();
        float speed_left = 0.0f;
        float speed_right = 0.0f;

        if(last_left_event == null)
        {
            last_left_event = _left_event;
            return;
        }
        if (last_right_event == null)
        {
            last_right_event = _left_event;
            return;
        }
        if (_left_event != null && last_left_event.timestamp != _left_event.timestamp)
        {
            if (last_left_event != null)
            {
                int delta_t = _left_event.timestamp - last_left_event.timestamp;
                int delta_counter = _left_event.counter - last_left_event.counter;

                speed_left = 3.6f * delta_counter * wheel_diameter_mm * Mathf.PI / delta_t;

                this._current_speed_left = speed_left;
                /* Save as last received event */
            }
            last_left_event = _left_event;
        }

        if (_right_event != null && last_right_event.timestamp != _right_event.timestamp)
        {
            if (last_right_event != null)
            {
                int delta_t = _right_event.timestamp - last_right_event.timestamp;
                int delta_counter = _right_event.counter - last_right_event.counter;

                speed_right = 3.6f * delta_counter * wheel_diameter_mm * Mathf.PI / delta_t;

                this._current_speed_right = speed_right;
            }
            /* Save as last received event */
            last_right_event = _right_event;
        }
    }

}
