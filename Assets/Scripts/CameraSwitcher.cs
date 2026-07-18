using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public GameObject camerasContainer;
    public Dictionary<string, CinemachineVirtualCamera> cameras;
    public string currentCameraName;
    public CinemachineVirtualCamera currentCamera;
    static bool exists = false;

    public void Awake() {
        // if (exists)
        // {
        //     Destroy(gameObject);
        //     return;
        // }
        // exists = true;
        // DontDestroyOnLoad(gameObject);
        //
        
        cameras = new Dictionary<string, CinemachineVirtualCamera>();
        var VCs = camerasContainer.GetComponentsInChildren<CinemachineVirtualCamera>(true);

        foreach (var vc in VCs)
        {
            cameras[vc.gameObject.name] = vc;
            if (vc.gameObject.name != "VC-Intro")
            {
                vc.Priority = 0;
            }
            else
            {
                vc.Priority = 10;
                currentCameraName = vc.gameObject.name;
                currentCamera = vc;
                Animator animator = vc.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("StartAnimation");
                }
            }
        }
    }

    public void SwitchCamera(string newCameraName)
    {
        Debug.Log("Camera switch request: " + newCameraName);
        if (!cameras.ContainsKey(newCameraName)) return;
        
        //cameras[currentCameraName].PreviousStateIsValid = true;
        
        //cameras[newCameraName].transform.rotation = cameras[currentCameraName].State.RawOrientation;
        
        cameras[newCameraName].Priority = 10;
        cameras[currentCameraName].Priority = 0;
        
        Animator currentAnimator = cameras[currentCameraName].GetComponent<Animator>();
        Animator newAnimator = cameras[newCameraName].GetComponent<Animator>();
        
        if (currentAnimator != null) currentAnimator.ResetTrigger("StartAnimation");
        if (newAnimator != null) newAnimator.SetTrigger("StartAnimation");
        
        
        currentCameraName = newCameraName;
        currentCamera = cameras[newCameraName];
    }
}