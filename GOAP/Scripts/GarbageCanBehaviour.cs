using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GarbageCanBehaviour : MonoBehaviour
{
    public GameObject garbagePrefab;
    GameObject garbage;

    public enum State { Full, Empty };
    public State state;

    float timer = 10f;

    // Start is called before the first frame update
    void Start()
    {
        // initializes random state
        state = (State)Random.Range(0, 2);

        if(state == State.Full)
        {
            InstantiateGarbage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            // changes state after every 10s
            if(state == State.Empty)
            {
                state = State.Full;
                InstantiateGarbage();
            }
            else
            {
                state = State.Empty;
                if(garbage.transform.position == gameObject.transform.position + new Vector3(0f, 1.9f, 0f))
                {
                    Destroy(garbage);
                }
            }
            timer = 10f;
        }
    }

    void InstantiateGarbage()
    {
        // instantiates a garbage in the garbage can 
        garbage = Instantiate(garbagePrefab, gameObject.transform.position + new Vector3(0f, 1.9f, 0f), garbagePrefab.transform.rotation);
        garbage.transform.parent = gameObject.transform;
    }
}
