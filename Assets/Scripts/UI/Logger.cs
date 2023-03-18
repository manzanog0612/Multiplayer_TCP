using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public GameObject prefab;
    public Transform logsParent;

    GameObject txtGo;
    TextMeshProUGUI text;

    public void SendLog(string log)
    {
        Debug.Log(log);
        //txtGo = Instantiate(prefab, logsParent);
        //text = txtGo.GetComponent<TextMeshProUGUI>();
        //text.text = log;
    }
}
