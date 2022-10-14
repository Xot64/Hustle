using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Player : NetworkBehaviour
{
    [SerializeField]
    float speed = 4f;
    [SerializeField]
    float shotLength = 3f;
    [SerializeField]
    float shotForce = 5f;
    [SerializeField]
    Color normalColor;
    [SerializeField]
    Color immortalColor;
    [SerializeField]
    float immortalTime = 3f;
    [SerializeField]
    float sensetive = 5f;
    [SerializeField]
    TextMeshProUGUI pointsText;
    int points;
    bool immortal;
    float shotLounchTime;
    
    Camera my_cam;
    Rigidbody my_RB;
    Animator my_Anima;
    Renderer my_BodyRend;
    CapsuleCollider my_Cldr;

    private void Awake()
    {
        my_RB = GetComponent<Rigidbody>();
        my_cam = GetComponentInChildren<Camera>();
        my_Anima = GetComponent<Animator>();
        my_BodyRend = GetComponentInChildren<Renderer>();
        pointsText = FindObjectOfType<TextMeshProUGUI>();
        pointsText.text = points.ToString();
        my_Cldr = GetComponent<CapsuleCollider>();
        
        //-----------------Переместить
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //-----------------------------
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if (!hasAuthority)
        {
            
            my_cam.gameObject.SetActive(false);
        }
        else
        {
            gameObject.layer = 0;
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (hasAuthority && !onShot)
        {
            my_RB.velocity = speed * (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal"));
            my_cam.transform.Rotate(-Input.GetAxis("Mouse Y") * sensetive, 0 , 0) ;
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensetive, 0);
            if (Input.GetButtonDown("Fire1")) Shot();
            my_Anima.SetFloat("Speed", my_RB.velocity.magnitude);
        }
    }
    bool stopShot;

    private void OnCollisionEnter(Collision collision)
    {
        if (onShot)
        {
            if (collision.gameObject.tag == "Barier") stopShot = true;
          /*  if (collision.gameObject.tag == "Player")
            {
                stopShot = true;
                Player other = collision.gameObject.GetComponent<Player>();
                if (!other.getOnShot() || other.getShotLounchTime() < shotLounchTime)
                {
                    other.Bang();
                    points++;
                    pointsText.text = points >= 3 ? "win" : points.ToString();
                }
            }*/
        }
    }

    bool onShot;
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
                if (Physics.Raycast(transform.position + transform.right * my_Cldr.radius / (2 *numRays.x) * x + transform.up * my_Cldr.height / (numRays.y) * y, transform.forward, out hit, shotLength, 1 << 6))
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
        pointsText.text = string.Format("{0}\n{1}\n{2}", (hited ? "!!!" : "???"), dist, (hited ? nearestHit.transform.gameObject.tag : ""));
        if (hited)
        {
            if (nearestHit.transform.gameObject.tag == "Player")
            {
                nearestHit.transform.GetComponent<Player>().Bang();
            }
        }
        transform.Translate(transform.forward * (dist - my_Cldr.radius),Space.World);

    }
    /*
    IEnumerator Shot()
    {
        RaycastHit hit;
        
        onShot = true;
        my_RB.velocity = shotForce * speed * transform.forward;
        Vector3 startPosition = transform.position;
        while (Vector3.Magnitude(transform.position - startPosition) < shotLength && !stopShot) yield return new WaitForEndOfFrame();
        onShot = false;
        stopShot = false;
        
    }*/
    public void Bang()
    {
        if (!immortal) StartCoroutine(C_Bang());
    }

    IEnumerator C_Bang()
    {
        immortal = true;
        my_BodyRend.material.color = immortalColor;
        yield return new WaitForSeconds(immortalTime);
        immortal = false;
        my_BodyRend.material.color = normalColor;
    }
    public float getShotLounchTime() => shotLounchTime;
    public bool getOnShot() => onShot;
}
