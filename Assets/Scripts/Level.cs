using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class Level : NetworkBehaviour
{
    public Dictionary<uint,Player> players = new Dictionary<uint, Player>();
    public static Level instance;
    public GameObject finalScreen;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI pointsText;
    NetworkStartPosition[] spawns;

    public float timeOut = 5;
    
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        spawns = FindObjectsOfType<NetworkStartPosition>();
        Status.instance.HideMouse();
    }
    public void AddPlayer(Player player) => players.Add(player.netId,player);
    
    public void RemovePlayer(uint id) => players.Remove(netId);
    
    
    [ClientRpc]
    public void Finish(Player player)
    {
        Status.instance.ShowMouse();
        winnerText.text = string.Format("{0} WIN", player.GetName());
        finalScreen.SetActive(true);
        
        foreach(Player p in players.Values) p.control = false;

        StartCoroutine(Timeout());
    }

    IEnumerator Timeout()
    {
        yield return new WaitForSeconds(timeOut);
        Restart();
    }
    public void Restart()
    {
        bool[] spawned = new bool[spawns.Length];
        int s;
        foreach (Player p in players.Values)
        {
            do s = Random.Range(0, spawns.Length);
            while (spawned[s]);
            spawned[s] = true;

            p.Reset(spawns[s].transform);
        }

        finalScreen.SetActive(false);
        Status.instance.HideMouse();
    }
}
