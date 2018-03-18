using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{

    public GameObject ball;
    public Text txt;

    [SyncVar]
    public NetworkInstanceId ballobject = NetworkInstanceId.Invalid;
    [SyncVar]
    public int playercount = 0;

    public override void OnStartServer()
    {
        Debug.Log("Server Startet");
        initBall();
        playercount = NetworkServer.connections.Count;
    }

    private void OnGUI()
    {
        string text = "";
        text = "Ball:" + ballobject.ToString() + System.Environment.NewLine;
        if (isServer)
        {

            foreach (NetworkConnection m in NetworkServer.connections)
            {
                text += m.connectionId + System.Environment.NewLine;
            }
        }
        text += "Spieler:" + playercount.ToString() + System.Environment.NewLine;
        txt.text = text;
    }

    void OnMessageRX(NetworkMessage netmsg)
    {    //receive from user
        Debug.Log("Message Received");
    }

    private void initBall()
    {
        if (ballobject == NetworkInstanceId.Invalid)
        {
            GameObject ba = (GameObject)Instantiate(ball);
            NetworkServer.Spawn(ba);
            ballobject = ba.GetComponent<NetworkIdentity>().netId;
            ball = ba;
        }
    }
}
