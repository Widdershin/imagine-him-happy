using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnMenu()
    {
        menu.SetActive(!menu.activeInHierarchy);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
