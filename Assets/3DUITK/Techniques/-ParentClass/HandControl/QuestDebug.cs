using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestDebug : MonoBehaviour
{
    public static QuestDebug Instance;

    Text logText;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var txt = DebugUIBuilder.instance.AddLabel("Start");
        logText = txt.GetComponent<Text>();
    }
    
    void Update()
    {
        DebugUIBuilder.instance.Show();
    }

    public void Log(string msg)
    {
        logText.text = msg;
    }
}
