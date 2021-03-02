using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// Jow: Transform is synchronized
// It needs the components NetworkIdentity + NetworkTransform


public class PillarMirror : NetworkBehaviour
{
    public override void OnStartClient()
    {
        //GameMan.s_instance.RegisterNewTool2(this, hasAuthority);
    }


    public override void OnStartServer()
    {
        JowLogger.Log($"{gameObject} OnStartServer @ {Time.fixedTime}s.");
    }


    // Start is called before the first frame update but after OnStartXXX
    void Start()
    {
    }
}
