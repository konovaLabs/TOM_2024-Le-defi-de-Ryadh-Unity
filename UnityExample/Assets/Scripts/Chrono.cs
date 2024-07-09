using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Chrono : MonoBehaviour
{
    public Text chrono_text = null;
    public bool is_visible = false;
    public bool is_start = false;

    public float elapsed_time = 0.0F;

    // Use this for initialization
    void Start()
    {
        SetVisible(false);
        StopChrono();
    }
    // Update is called once per frame
    void Update()
    {
        if (is_start)
        {
            elapsed_time += Time.deltaTime;
        }

        if(chrono_text == null)
        {
            return;
        }

        if(is_visible)
        {
            int minutes = Mathf.FloorToInt(elapsed_time / 60F);
            int seconds = Mathf.FloorToInt(elapsed_time % 60F);
            int milliseconds = Mathf.FloorToInt((elapsed_time * 1000F) % 1000F);

            chrono_text.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
        else
        {
            chrono_text.text = "";
        }
    }

    public void StartChrono()
    {
        elapsed_time = 0.0f;
        is_start = true;
    }

    public void StopChrono()
    {
        is_start = false;
    }

    public void SetVisible(bool visible)
    {
        is_visible = visible;
    }
}
