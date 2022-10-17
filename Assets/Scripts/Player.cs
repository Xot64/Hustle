using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateName))]
    //[SyncVar]
    public string s_myName;
    string myName;

    [SyncVar(hook = nameof(UpdateImmortal))]
    //[SyncVar]
    public int s_Immortal;
    int immortal;

    [SyncVar(hook = nameof(UpdatePoints))]
    public int s_Points;
    int points;

    public TextMeshPro nameText;
    public TextMeshPro pointsText;
    public Transform my_cam;
    public Transform person;
    static Camera actionCam;

    [SerializeField]
    float speed = 4f;
    [SerializeField]
    float shotLength = 5f;
    [SerializeField]
    Color normalColor;
    [SerializeField]
    Color immortalColor;
    [SerializeField]
    float immortalTime = 3f;
    [SerializeField]
    float sensetive = 5f;
    
    [HideInInspector]
    public bool isMe;
    [HideInInspector]
    public bool control = true;
    //public bool immortal;
    Vector3 lastposition;

    Rigidbody my_RB;
    Animator my_Anima;
    Renderer my_BodyRend;
    CapsuleCollider my_Cldr;
    public NetworkTransform my_NT;
    Level level;
    Status status;
    private void Awake()
    {

        my_RB = GetComponent<Rigidbody>();
        my_Anima = GetComponent<Animator>();
        my_BodyRend = GetComponentInChildren<Renderer>();

        my_Cldr = GetComponent<CapsuleCollider>();
        my_NT = GetComponent<NetworkTransform>();
        //-----------------Переместить

        //-----------------------------
    }
    // Start is called before the first frame update
    void Start()
    {
        isMe = hasAuthority;
        level = Level.instance;
        status = Status.instance;
        level.pointsText.gameObject.SetActive(true);
        level.AddPlayer(this);
        level.pointsText.text = points.ToString();
        lastposition = transform.position;
        if (isMe)
        {
            CallBang(netId);
            actionCam = GetComponentInChildren<Camera>();
            Status.instance.HideMouse();
            gameObject.layer = 0;
            nameText.gameObject.SetActive(false);
            TakeName();
        }
        else
        {
            UpdateColor();
            my_cam.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (isMe)
        {
            if (control)
            {
                my_RB.velocity = speed * (my_cam.forward * Input.GetAxis("Vertical") + my_cam.right * Input.GetAxis("Horizontal"));
                my_cam.Rotate(-Input.GetAxis("Mouse Y") * sensetive, Input.GetAxis("Mouse X") * sensetive, 0);
                my_cam.localEulerAngles = new Vector3(Mathf.Clamp(normalAngle(my_cam.transform.eulerAngles.x), -45, 45), my_cam.localEulerAngles.y, 0);
                if (Input.GetButtonDown("Fire1")) Shot();
                my_Anima.SetFloat("Speed", my_RB.velocity.magnitude);

            }
            else 
            {
                my_RB.velocity = Vector3.zero;
            }
        }
        else
        {
            if (actionCam != null) nameText.transform.LookAt(actionCam.transform);
            if (nameText.text != myName) nameText.text = myName;
        }

        UpdateColor();
        person.LookAt(2 * transform.position - lastposition);
        lastposition = transform.position;
    }

    private void OnDestroy()
    {
        level.RemovePlayer(netId);
        status.ShowMouse();
    }
    void UpdateColor()
    {
        my_BodyRend.material.color = immortal == 1 ? immortalColor : normalColor;
    }


    public void Shot()
    {
        RaycastHit hit, nearestHit;
        nearestHit = new RaycastHit();
        float dist = shotLength + 0.01f;
        Vector2Int numRays = new Vector2Int(2, 6);
        bool hited = false;
        for (int x = -numRays.x; x <= numRays.x; x++)
        {
            for (int y = 0; y <= numRays.y; y++)
            {
                if (Physics.Raycast(transform.position + my_cam.right * my_Cldr.radius / (2 * numRays.x) * x + transform.up * my_Cldr.height / (numRays.y) * y, my_cam.forward, out hit, shotLength, 1 << 6))
                {
                    if (hit.distance < dist)
                    {
                        nearestHit = hit;
                        dist = hit.distance;
                    }
                    hited = true;
                }
            }
        }
        if (hited)
        {
            if (nearestHit.transform.gameObject.tag == "Player")
            {
                Player other = nearestHit.transform.GetComponent<Player>();
                if (other.immortal == 0)
                {
                    CallBang(other.netId);
                    SetPoints(points + 1);

                }
            }
        }
        transform.Translate(new Vector3(my_cam.forward.x, 0, my_cam.forward.z).normalized * (dist - my_Cldr.radius), Space.World);
        person.LookAt(transform.position + new Vector3(my_cam.forward.x, 0, my_cam.forward.z).normalized);
    }

    [Command]
    void Finish() => level.Finish(this);

    public void TakeName()
    {
        Dictionary<uint,Player> players = Level.instance.players;
        string newName = status.playerName;
        if (newName == string.Empty) newName = "Player " + (players.Count).ToString();

        int thisNames = 0;
        foreach (Player p in players.Values)
        {
            if (p.GetName() == newName)
            {
                thisNames++;
                newName = status.playerName + " " + thisNames.ToString();
            }
        }
        SetName(newName);
        /*
        if (isServer) SetSrvName(newName);
        else SetComName(newName);
        */
    }

    public string GetName() => myName;

    
    public void Bang()
    {
        //SetImmortal(!immortal);
        StartCoroutine(C_Bang());
    }

    public void CallBang(uint netId)
    {
        if (isServer) CallBangSRV(netId);
        else CallBangCMD(netId);
                
    }
    [Server]
    public void CallBangSRV(uint netId)
    {
        level.players[netId].Bang();
    }
    [Command]
    public void CallBangCMD(uint netId)
    {
        CallBangSRV(netId);
    }

    public void Reset(Transform newPosition)
    {
        transform.position = newPosition.position;
        SetPoints(0);
        control = true;
        CallBang(netId);
    }
    IEnumerator C_Bang()
    {
        if (immortal == 0)
        {
            SetImmortal(1);
            //   UpdateColor();
            yield return new WaitForSeconds(immortalTime);
            SetImmortal(0);
            //   UpdateColor();
        }
    }

    //--------------------------------------------------
    //Sync

    //NAME--------------------------
    void UpdateName(string oldValue, string newValue)
    {
        myName = newValue;
        if (!isMe) nameText.text = newValue;
    }

    void SetName(string val)
    {
        if (isServer) SetSrvName(val);
        else SetComName(val);
    }

    [Server]
    void SetSrvName(string val)
    {
        s_myName = val;
    }
    [Command]
    void SetComName(string val)
    {
        SetSrvName(val);
    }
    //-----------------------NAME

    //Immortal-------------------
    void SetImmortal(int newVar)
    {
        if (isServer) SetSrvImmortal(newVar);
        else SetComImmortal(newVar);
    }

    [Server]
    public void SetSrvImmortal(int newVar) => s_Immortal = newVar;

    [Command]
    public void SetComImmortal(int newVar) => SetSrvImmortal (newVar);

    void UpdateImmortal(int oldValue, int newValue) => immortal = newValue;
    
    //-----------------Immortal
    //Points--------------------
    
    void SetPoints(int val)
    {
        if (isServer) SetSrvPoints(val);
        else SetComPoints(val);
    }
    [Server]
    public void SetSrvPoints(int val) => s_Points = val;
    [Command]
    public void SetComPoints(int val) => SetSrvPoints(val);
    void UpdatePoints(int oldValue, int newValue)
    {
        points = newValue;
        if (isMe)
        {
            level.pointsText.text = points.ToString();
            if (points >= 3)
            {
                Finish();
            }
        }
        pointsText.text = points.ToString();
    }
    //-------------Points
    //Math
    public float normalAngle(float angle) => angle < 180 ? angle : angle - 360;
}
