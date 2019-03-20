﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

#pragma warning disable
public class Player
{
    public string playerName;
    public GameObject avatar;
    public int connectionId;
}

public class Client : MonoBehaviour
{
    public GameObject playerPrefab;
    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    private const int MAX_CONNECTIONS = 100;

    private int port = 7777;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private int ourClientId;
    private int connectionId;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;

    public void Connect()
    {
        // Does the player have a name?
        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (pName == "") {
            Debug.Log("You must enter a name!");
            return;
        }

        playerName = pName;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, 0);
        // Local host IP is "127.0.0.1" or "LOCALHOST"
        connectionId = NetworkTransport.Connect(hostId, "142.160.105.52", port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
    }

    private void Update()
    {
        if (!isConnected) return;

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
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0]) {
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "CNN":
                        SpawnPlayer(splitData[1], int.Parse(splitData[2]));
                        break;
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;
                    case "ASKPOSITION":
                        OnAskPosition(splitData);
                        break;
                    default:
                        Debug.Log("Invalid message: " + msg);
                        break;
                }
                break;
            case NetworkEventType.BroadcastEvent:
                break;
        }
    }

    private void OnAskName(string[] data) {
        // Set this client's ID
        ourClientId = int.Parse(data[1]);

        // Send our name to the server.
        Send("NAMEIS|" + playerName, reliableChannel);

        // Create all the other players.
        for (int i = 2; i < data.Length - 1; i++) {
            string[] d = data[i].Split('%');

            if (d[0] != "TEMP")
                SpawnPlayer(d[0], int.Parse(d[1]));
        }
    }
    private void OnAskPosition(string[] data) {
        if (!isStarted)
            return;

        // Update everyone else.
        for (int i = 1; i < data.Length; i++) {
            string[] d = data[i].Split('%');

            // Prevent the server from updating us!
            if (ourClientId != int.Parse(d[0]))
            {
                Vector2 position = Vector2.zero;
                position.x = float.Parse(d[1]);
                position.y = float.Parse(d[2]);
                players[int.Parse(d[0])].avatar.transform.position = position;
            }
        }

        // Send our own position.
        Vector2 myPosition = players[ourClientId].avatar.transform.position;
        string m = "MYPOSITION|" + myPosition.x.ToString() + '|' + myPosition.y.ToString();
        Send(m, unreliableChannel);
    }
    private void SpawnPlayer(string playerName, int conId) {
        GameObject go = Instantiate(playerPrefab) as GameObject;

        // Is this ours?
        if (conId == ourClientId) {
            // Add mobility
            go.AddComponent<PlayerController>();
            GameObject.Find("Canvas").SetActive(false);
            isStarted = true;
        }

        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionId = conId;
        p.avatar.GetComponentInChildren<TextMesh>().text = playerName;
        players.Add(conId, p);
    }
    private void PlayerDisconnected(int conId) {
        Destroy(players[conId].avatar);
        players.Remove(conId);
    }

    private void Send(string message, int channelId)
    {
        Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
    }
}
