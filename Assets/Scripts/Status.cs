using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class Status : MonoBehaviour
{
    public string playerName;
    public static Status instance;
    NetworkManagerHUD hud;
    // Start is called before the first frame update
    void Start()
    {
        hud = GetComponent<NetworkManagerHUD>();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);
    }
    public void SetName(TextMeshProUGUI name) => playerName = name.text.Trim();
    public void ShowMouse(bool show = true)
    {
        hud.enabled = show;
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.Confined : CursorLockMode.Locked; ;
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) Application.Quit();
    }
    public void HideMouse() => ShowMouse(false);    
}
