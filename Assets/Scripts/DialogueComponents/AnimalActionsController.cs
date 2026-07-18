using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimalActionsController
{
    public void TriggerAnimalActions(Node node, bool isEndNodeRun = false)
    {
        if (node.animalActions == null) return;

        GameObject area = DialogueManager.Instance.animalsLayer;
        if (node.area != null)
        {
            area = DialogueManager.Instance.animalsLayer.transform.Find(node.area).gameObject;
        }
        Debug.Log("Getting animals from area: " + area);
        
        foreach (var actionObj in node.animalActions)
        {
            Debug.Log("Evaluating animal action: " + actionObj.animalName + ", " + actionObj.action + ", condition: " + actionObj.condition);

            // flag end node animation for running later
            if (!isEndNodeRun && actionObj.endNodeAnimation)
            {
                DialogueManager.Instance.hasEndNodeAnimationsToRun = true;
                continue;
            }

            if (isEndNodeRun && !actionObj.endNodeAnimation)
            {
                continue;
            }
            
            if (actionObj.condition != null)
            {
                // if condition of the action doesn't match up with what the user chose
                if (!((actionObj.condition == "ego" &&
                       DialogueManager.Instance.playerState.lastChoiceType == ChoiceType.Ego)
                      || (actionObj.condition == "empathy" &&
                          DialogueManager.Instance.playerState.lastChoiceType == ChoiceType.Empathy)
                      || (actionObj.condition == "practicality" &&
                          DialogueManager.Instance.playerState.lastChoiceType == ChoiceType.Practicality)))
                {
                    Debug.Log("Skipping action: " + actionObj.action);
                    continue;
                }
            }
            
            // Find animal or animals from the defined action object in the animalslayer
            List<GameObject> animals = new List<GameObject>();
            if (actionObj.animalName == "All")
            {
                List<GameObject> allChildren = area.transform
                    .Cast<Transform>()
                    .Select(t => t.gameObject)
                    .ToList();
                
                animals.AddRange(allChildren);
            } else if (actionObj.pluralAnimals)
            {
                animals.AddRange(HelperFunctions.FindChildrenByNameContaining(area, actionObj.animalName));
            }
            else
            {
                animals.Add(HelperFunctions.FindChildByNameContaining(area, actionObj.animalName));
            }
            
            Debug.Log("Found " + animals.Count + " animals to animate");

            if (animals.Count == 0)
            {
                Debug.LogError("Couldn't find animal object");
                return;
            }
            
            foreach (GameObject animal in animals)
            {
                ExecuteAnimalAction(actionObj, animal);
                if (actionObj.includeChildrenLayers)
                {
                    List<GameObject> childrenLayers = HelperFunctions.FindChildrenByNameContaining(animal, actionObj.animalName);
                    foreach (var childLayer in childrenLayers)
                    {
                        ExecuteAnimalAction(actionObj, childLayer);
                    }
                }
            }
        }
    }

    void ExecuteAnimalAction(AnimalAction actionObj, GameObject animal)
    {
        Debug.Log("Animation " + actionObj.action + " for animal " + animal.name);
        if (actionObj.fromAnimator)
        {
            Debug.Log("Getting animal animator component");
            Animator animator = animal.GetComponent<Animator>();
            if (actionObj.action == "Enter")
            {
                animal.gameObject.SetActive(true);
            }else if (actionObj.action == "Exit") {
                animal.gameObject.SetActive(false);
            }else { // walk, run, drowning
                animator.SetTrigger(actionObj.action);
            }
        }
        else
        {
            if (actionObj.action == "Exit") {
                if (actionObj.animalName.Contains("Bird"))
                {
                    lb_BirdController birdControl = animal.GetComponent<lb_BirdController>();
                    birdControl.AllFlee();
                }
            }
        }
    }
}