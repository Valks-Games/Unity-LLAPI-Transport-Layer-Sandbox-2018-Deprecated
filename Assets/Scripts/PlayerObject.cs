using UnityEngine;
using UnityEngine.Networking;

public class PlayerObject : NetworkBehaviour
{
    public GameObject playerPrefabObject;

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;
        CmdSpawnMyUnit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Server instructions.
    [Command]
    void CmdSpawnMyUnit() {
        GameObject go = Instantiate(playerPrefabObject);
        if (isClient)
        {
            NetworkServer.SpawnWithClientAuthority(go, connectionToClient);
        }
        else {
            NetworkServer.Spawn(go);
        }
    }
}
