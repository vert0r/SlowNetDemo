using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager {

    GameManager gm;

    // Use this for initialization
    void Start () {
        
	}

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (gm == null) { gm = GameObject.Find("GameManager").GetComponent<GameManager>(); }
        if (conn.hostId >= 0)
        {
            gm.playercount = this.numPlayers;
        }
        if(conn.hostId == -1)
        {
            //LOKAL HOST CONNECTS ... 
            gm.playercount = this.numPlayers;
        }
        base.OnServerConnect(conn);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
