using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BallController : NetworkBehaviour
{
    GameManager gm;
    private NetworkInstanceId me = NetworkInstanceId.Invalid;
    MyNetworkManager m;
    Renderer ballRenderer;

    private void Start()
    {
        //GET NETWORK OBJECTS
        if (!isLocalPlayer)
        {
            this.transform.Find("PlayerCam").gameObject.SetActive(false);
            return;
        }
        //Camera.main.enabled = false;

        m = GameObject.Find("Network Manager").GetComponent<MyNetworkManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        ballRenderer = this.transform.Find("PlayerCam").Find("PlayerBall").GetComponent<Renderer>();
    }

    public override void OnStartClient()
    {
        me = this.netId;
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        //Debug.Log("StartLocal");
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        //Debug.Log("start gm" + gm);
        MyNetworkManager m = GameObject.Find("Network Manager").GetComponent<MyNetworkManager>();
        name = "Player " + m.numPlayers.ToString();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Ball")
        {
            if (isServer)
            {
                RpcEnableBall(me);
            }
            else
            {
                CmdBallTaken(me);
            }
        }
    }

    [Command]
    void CmdBallTaken(NetworkInstanceId id)
    {
        RpcEnableBall(id);
    }

    [ClientRpc]
    private void RpcEnableBall(NetworkInstanceId id)
    {
        CmddisableBall();
        ballRenderer.enabled = true;
    }

    [Command]
    void CmddisableBall()
    {
        gm.ball.SetActive(false);
    }

    private void Update()
    {
        checkInput();
    }

    private void checkInput()
    {
        if (GetComponent<NetworkIdentity>().hasAuthority == false) return;
    }
}
