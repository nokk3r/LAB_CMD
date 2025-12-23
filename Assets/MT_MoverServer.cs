using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_MoverServer : MonoBehaviour
{
    public JSONDataServerFetcher dataFetch;
    public string number;
    private Vector3 velocity;

    void Start()
    {
        if (dataFetch == null)
        {
            dataFetch = FindObjectOfType<JSONDataServerFetcher>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dataFetch == null) return;

        float x = dataFetch.GetFloatValue("Channel_1_Axis_" + number + " (X)_Rotation");
        float y = dataFetch.GetFloatValue("Channel_1_Axis_" + number + " (Y)_Rotation");
        float z = dataFetch.GetFloatValue("Channel_1_Axis_" + number + " (Z)_Rotation");
        Quaternion targetPos = Quaternion.Euler(x, y, z);
        transform.rotation = targetPos;
    }
}