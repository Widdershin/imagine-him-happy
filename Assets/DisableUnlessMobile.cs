using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DisableUnlessMobile : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern bool IsMobile();

    public bool isMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
                 return IsMobile();
#endif
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isMobile())
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
