using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

#pragma warning disable
public class ServerClient
{
    public int connectionId;
    public string playerName;
    public Vector2 position;
}

public class Server : MonoBehaviour
{
    private const int MAX_CONNECTIONS = 100;

    private int port = 7777;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.03f;

    private List<ServerClient> clients = new List<ServerClient>();

    private void Start()
    {
        Application.targetFrameRate = 60;
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted) return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player: " + connectionId + " has connected");
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                //Debug.Log("Recieving from: " + connectionId + " has sent: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "MYPOSITION":
                        OnMyPosition(connectionId, float.Parse(splitData[1]), float.Parse(splitData[2]));
                        break;
                    default:
                        Debug.Log("Invalid message: " + msg);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player: " + connectionId + " has disconnected");
                OnDisconnection(connectionId);
                break;
            case NetworkEventType.BroadcastEvent:
                break;
        }

        // Ask player for their position.
        if (Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            string m = "ASKPOSITION|";
            foreach (ServerClient sc in clients) {
                m += sc.connectionId.ToString() + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString() + '|';
            }
            m = m.Trim('|');

            Send(m, unreliableChannel, clients);
        }
    }

    private void OnConnection(int conId) {
        // Add the player to a list. When a player joins the
        // server, tell them their ID and request their name.
        // And send the name of all the other players.

        ServerClient c = new ServerClient();
        c.connectionId = conId;
        c.playerName = "TEMP";
        clients.Add(c);

        string msg = "ASKNAME|" + conId + '|';
        foreach (ServerClient sc in clients) {
            msg += sc.playerName + '%' + sc.connectionId + '|';
        }

        msg = msg.Trim('|');

        // ASKNAME|3|DAVE%1|BOB%2|TEMP%3
        Send(msg, reliableChannel, conId);
    }
    private void OnNameIs(int conId, string playerName) {
        // Link the name to the connection ID
        clients.Find(x => x.connectionId == conId).playerName = playerName;

        // Tell everyone that a new player connected.
        Send("CNN|" + playerName + '|' + conId, reliableChannel, clients);
    }
    private void OnDisconnection(int conId) {
        // Remove this player from our client list.
        clients.Remove(clients.Find(x => x.connectionId == conId));
        // Tell everyone that someone else has disconnected.
        Send("DC|" + conId, reliableChannel, clients);
    }
    private void OnMyPosition(int conId, float x, float y) {
        clients.Find(c => c.connectionId == conId).position = new Vector2(x, y);
    }
    private void Send(string message, int channelId, int conId) {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionId == conId));
        Send(message, channelId, c);

    }

    private void Send(string message, int channelId, List<ServerClient> c) {
        //Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (ServerClient sc in c) {
            NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
        }
    }
}
