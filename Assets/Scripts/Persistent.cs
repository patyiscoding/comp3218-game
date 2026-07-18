using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persistent : MonoBehaviour
{
    private static bool exists = false;

    public void Awake()
    {
        if (exists)
        {
            Destroy(gameObject);
            return;
        }

        exists = true;
        DontDestroyOnLoad(gameObject);
    }
}
