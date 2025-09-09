using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Program : MonoBehaviour
{

    void Start()
    {
        Debug.Log("Hello, World!");
        Publicsher publisher = new Publicsher();
        publisher.msg += ResultProcess;
        publisher.SendMessage("다음 문제");

        Debug.Log("작업 완료 !");
    }

    void ResultProcess(string msg)
    {
        Debug.Log($"메세지 수신: {msg}");
    }

    void OntherProcess(string text)
    {
        Debug.Log($"다른 처리: {text}");
    }
}

public class Publicsher
{
    public delegate void OnMessagge(string msg);
    public event OnMessagge msg;
    public void SendMessage(string text)
    {
            Debug.Log($"ChatGPT API 와 통신합니다.(오래걸림)...{text}");

        msg?.Invoke(text); ;
    }
    
}