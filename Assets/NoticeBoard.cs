using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoticeBoard : MonoBehaviour
{
    [TextArea]
    public string text;
    private PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player.SetNoticeBoard(text);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player.SetNoticeBoard("");
        }
    }
}
