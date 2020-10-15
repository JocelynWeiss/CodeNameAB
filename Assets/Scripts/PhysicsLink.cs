using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PhysicsLink : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    [Command]//function that runs on server when called by a client
    public void CmdResetPose()
    {
    }
}
