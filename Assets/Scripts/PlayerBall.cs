using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBall : MonoBehaviour
{
    public Text txt;
    public float gravity = 20;
    
    float distToGround;
    Vector3 movement;

    float x, y, z;
    float bounce = 0.81111111111f;
    float lastSpeed = 0;

    Vector3 oldPos = new Vector3(0,0,0);
    Vector3 velo;
    Vector3 direction;
    Vector3 startPos = new Vector3(0,0,0);
    Rigidbody body;

    public bool isGround;

    void Start()
    {
        body = this.GetComponent<Rigidbody>();
        startPos = transform.position;
        oldPos = transform.position;
        // get the distance to ground
        distToGround = this.GetComponent<Collider>().bounds.extents.y;
        x = 0; y = 0; z = 0;
        //StartCoroutine(CalcVelocity());
    }

    //IEnumerator CalcVelocity()
    //{
    //    while (true)
    //    {
    //        // Position at frame start
    //        oldPos = transform.position;
    //        // Wait till the end of the frame
    //        yield return new WaitForEndOfFrame();
    //        velo = (oldPos - transform.position) / Time.deltaTime;
    //        direction = velo.normalized;
    //        //Debug.Log(velo);
    //    }
    //}

    IEnumerator restart()
    {
        transform.position = startPos;
        yield return new WaitForSeconds(4);
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    void LateUpdate()
    {
    }
}
