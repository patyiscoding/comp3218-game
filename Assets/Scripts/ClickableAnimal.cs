using System;
using UnityEngine;
public class ClickableAnimal : MonoBehaviour
{
    public string clickAction = "Run";

    void OnMouseDown()
    {
        Debug.Log("Animal clicked");
        if (DialogueManager.Instance != null && DialogueManager.Instance.animalActionsController != null)
        {
            DialogueManager.Instance.animalActionsController.TriggerActionOn(gameObject, clickAction);
            DialogueManager.Instance.animalActionsController.TriggerActionOn(gameObject, "Idle");
            DialogueManager.Instance.playerState.practicality += 0.2;
            Debug.Log("Animal clicked, action triggered, practicality: " + DialogueManager.Instance.playerState.practicality);

        }
    }
}

