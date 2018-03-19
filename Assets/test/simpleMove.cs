using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class simpleMove : NetworkBehaviour {

    public Text txt;
    float speed = 50f;
    string mytxt = "Leer";
    string nfo = "";

    // Use this for initialization
    void Start () {
        txt = GameObject.Find("Canvas").transform.Find("Text").GetComponent<Text>();
        if (isServer) { nfo = "Server"; } else { nfo = "Client"; }
	}

    private void OnCollisionEnter(Collision collision)
    {
        debuglog("COLL");
        this.gameObject.GetComponent<Renderer>().material.color = Color.red;
        if (isServer)
        {
            RpcUpdate();
        }
    }

    // Update is called once per frame
    void Update () {
        if (isLocalPlayer)
        {
            float x = Input.GetAxis("Horizontal");
            transform.Translate(new Vector3(0, 0, x * speed * Time.deltaTime));
        }
    }

    [ClientRpc]
    void RpcUpdate()
    {
        this.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
    }

    List<string> l = new List<string>();
    void debuglog(string t)
    {
        string mt = nfo + ":" + t + System.Environment.NewLine;
        l.Add(mt);
        if (l.Count > 5) { l.RemoveRange(0, 3); }
    }

    private void OnGUI()
    {
            txt.text = string.Join(" ", l.ToArray());
    }
}
