using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_Mover : MonoBehaviour
{
    public JSONDataFetcher dataFetch;
    private Vector3 velocity;

    void Start()
    {
        if (dataFetch == null)
        {
            dataFetch = FindObjectOfType<JSONDataFetcher>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dataFetch == null) return;

        float x = dataFetch.GetFloatValue("Channel_1_Axis_1 (X)_CurPos");
        float y = dataFetch.GetFloatValue("Channel_1_Axis_2 (Y)_CurPos");
        float z = 0f;
        Vector3 targetPos = new Vector3(x * 0.05f, 0f, z);
        transform.position = targetPos;
    }
}