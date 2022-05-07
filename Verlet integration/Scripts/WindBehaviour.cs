using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindBehaviour : MonoBehaviour
{
    public Text text;
    public float air_speed = 0f;
    float timer = 2f;

    // Update is called once per frame
    void Update()
    {
        text.text = gameObject.name + " air speed = " + air_speed;
        
        // air-speed of a column changes about every 2s
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ChangeSpeed();
            timer = 2f;
        }
    }

    void ChangeSpeed()
    {
        air_speed = Random.Range(0f, 0.01f);
    }
}
