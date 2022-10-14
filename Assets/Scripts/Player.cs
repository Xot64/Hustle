using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    float speed = 4f;
    [SerializeField]
    float shotLength = 3f;
    [SerializeField]
    float immortalTime = 3f;
    
    bool immortal;
    float shotLounchTime;
    
    Camera cam;
    Rigidbody my_RB;
    Animator my_Anima;

    private void Awake()
    {
        my_RB = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        
        my_Anima = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if (!hasAuthority)
        {
            cam.gameObject.SetActive(false);
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (hasAuthority && !onShot)
        {
            my_RB.velocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed;
            cam.transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
            transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
            if (Input.GetButtonDown("Fire1")) StartCoroutine(Shot());
            my_Anima.SetFloat("Speed", my_RB.velocity.magnitude);
        }
    }
    bool barier;

    private void OnCollisionEnter(Collision collision)
    {
        if (onShot)
        if (collision.gameObject.tag == "Barier") barier = true;
        if (collision.gameObject.tag == "Player")
        {
            Player other = collision.gameObject.GetComponent<Player>();
            if (!other.getOnShot() || other.getShotLounchTime() < shotLounchTime) other.Bang();
        }
    }

    bool onShot; 
   
    IEnumerator Shot()
    {
        onShot = true;
        my_RB.velocity = new Vector3(0, 0, 3 * speed);
        Vector3 startPosition = transform.position;
        while (Vector3.Magnitude(transform.position - startPosition) < shotLength && !barier) yield return new WaitForEndOfFrame();
        onShot = false;
        barier = false;
    }
    public void Bang()
    {
        if (!immortal) StartCoroutine(C_Bang());
    }

    IEnumerator C_Bang()
    {
        yield return new WaitForSeconds(immortalTime);
        immortal = true;
    }
    public float getShotLounchTime() => shotLounchTime;
    public bool getOnShot() => onShot;
}
