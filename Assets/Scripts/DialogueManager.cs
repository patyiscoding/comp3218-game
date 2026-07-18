using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    public Scene[] scenes;
    public Node[] nodes;
    public Node currentNode;
    private Coroutine nodeWaitCoroutine;
    private Node alreadyChosenFollowingNode;
    public bool hasEndNodeAnimationsToRun;
    
    public TMP_Text UIWarning;
    public TMP_Text UINarrative;
    //public TMP_Text UIStagDialogue;
    public TMP_Text UIAnimalDialogue;
    public Transform choicesPanel;
    public Transform stagDialogue;
    public GameObject choicePrefab;
    public GameObject stagDialoguePrefab;
    public GameObject animalsLayer;

    public GameObject soundsLayer;
    private GameObject state;
    public PlayerState playerState;
    public CameraSwitcher cameraSwitcher;
    public AnimalActionsController animalActionsController;
    public bool finalOutcomeCalculated;
    
    
    private void Awake()
    {
        // handles global DialogueManager Instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        DialogueLoader loader = new DialogueLoader();
        if (loader.dialogueRoot == null)
        {
            Debug.LogError("DialogueLoader.dialogue is null");
            return;
        }
        if (loader.dialogueRoot.scenes == null)
        {
            Debug.LogError("DialogueLoader.dialogue.nodes is null");
            return;
        }

        scenes = loader.dialogueRoot.scenes;
        nodes = scenes[0].sceneNodes; // TODO
        Debug.Log("Dialogue nodes loaded: " + nodes.Length);
        
        animalActionsController = new AnimalActionsController();
        
        state = new GameObject("PlayerState");
        playerState = state.AddComponent<PlayerState>();
        playerState.pride = 0;
        playerState.empathy = 0;
        playerState.practicality = 0;
        finalOutcomeCalculated = false;
        
        nodeWaitCoroutine = null;
        alreadyChosenFollowingNode = null;
        currentNode = nodes[0];
        ShowNode(currentNode);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // show warning to select a choice if one needs to be selected but hasn't yet
            // otherwise, try to proceed
            if (currentNode.choices != null && alreadyChosenFollowingNode == null)
            {
                ShowWarning("Please select a choice");
            }else{
                SwitchNode(HelperFunctions.GetNextNodeNonReliantOnChoice());
            }
        }
        
        // Debugging, previous node fetching
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            SwitchNode(HelperFunctions.GetMinusOneNode());
        }
        
        if (currentNode.followToNextNodeAutomatically)
        {
            Animator an = cameraSwitcher.currentCamera.GetComponent<Animator>();
            AnimatorStateInfo stateInfo = an.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("AnimationFinished"))
            {
                SwitchNode(HelperFunctions.GetNextNodeNonReliantOnChoice());
            }
        }
    }
    
    // if a node during which user input comes in (space or click) has a wait time, on user input, execute that wait time and then proceed
    // if followToNextNodeAutomatically = keep checking if animation has finished, then proceed to the next node
    // if space is pressed during waiting or animation, stop any ongoing wait time (if any) and go to next node; manual override
    public void SwitchNode(Node node)
    {
        Debug.Log("Request to switch nodes: " + currentNode.id + "->" + node.id);

        if (hasEndNodeAnimationsToRun)
        {
            animalActionsController.TriggerAnimalActions(currentNode, true);
            hasEndNodeAnimationsToRun = false;
        }
        
        // if there's assigned wait time for the currentNode, but hasn't started
        if (currentNode.waitNullable != null && nodeWaitCoroutine == null)
        {
            StartCoroutine(WaitAndSwitchNode(node, currentNode.waitNullable));
            return;
        }
        
        // regular node with no wait time
        if (nodeWaitCoroutine == null)
        {
            ShowNode(node);
            return;
        }
        
        // otherwise, there's wait time in progress and an interruption came in (space pressed)
        // then, ignore the incoming node and proceed to the one already assigned to the waiting time coroutine
        StopCoroutine(nodeWaitCoroutine);
        Node nextNode = alreadyChosenFollowingNode ?? node;
        
        nodeWaitCoroutine = null;
        alreadyChosenFollowingNode = null;
        ShowNode(nextNode);
    }
    
    private IEnumerator WaitAndSwitchNode(Node node, float? delay)
    {
        Debug.Log("WAIT TIME TRIGGERED (s): " + delay);
        alreadyChosenFollowingNode = node; // each wait time has an already chosen next node assigned
        yield return new WaitForSeconds(delay ?? 1);

        Debug.Log("WAIT TIME FINISHED");
        ShowNode(node);
    }

    public void clearUI()
    {
        UINarrative.text = "";
        UIAnimalDialogue.text = "";
        choicesPanel.gameObject.SetActive(false);
        stagDialogue.gameObject.SetActive(false);
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in stagDialogue)
        {
            Destroy(child.gameObject);
        }
    }

    private void calculateOutcome()
    {
        int maxScore = Math.Max(playerState.pride, Math.Max(playerState.empathy, playerState.practicality));
        finalOutcomeCalculated = true;
        if (maxScore == playerState.pride)
        {
            playerState.lastChoiceType = ChoiceType.Ego;
            return;
        }
        if (maxScore == playerState.empathy)
        {
            playerState.lastChoiceType = ChoiceType.Empathy;
            return;
        }
        if (maxScore == playerState.practicality)
        {
            playerState.lastChoiceType = ChoiceType.Practicality;
        }
    }
    
    public void ShowNode(Node node)
    {
        currentNode = node;
        
        // clear UI
        clearUI();
        
        // switch to camera
        Debug.Log("START node: " + node.id + " Animal actions: " + node.animalActions);
        if (node.camera != null && cameraSwitcher.currentCameraName != node.camera)
        {
            cameraSwitcher.SwitchCamera(node.camera);
        }

        if (node.calculateOutcome)
        {
            calculateOutcome();
        }
        
        // handle animal actions
        animalActionsController.TriggerAnimalActions(node);

        if (!String.IsNullOrEmpty(node.playSound))
        {
            GameObject sound = HelperFunctions.FindChildByNameContaining(soundsLayer, node.playSound);
            sound.SetActive(true);
            sound.GetComponent<AudioSource>().Play();
        }
        
        // handle dialogue + player actions
        Debug.Log("Dialogue: " + node.dialogueVersions.defaultVersion);
        if (node.dialogueVersions != null)
        {
            TMP_Text textNode = UINarrative;
            if (node.speaker == "Animal")
            {
                textNode = UIAnimalDialogue;
            } else if (node.speaker == "Stag" && node.choices == null)
            {
                stagDialogue.gameObject.SetActive(true);
                GameObject UIStagDialoguePanel = Instantiate(stagDialoguePrefab, stagDialogue);
                Button btn = UIStagDialoguePanel.GetComponentInChildren<Button>();
                TextMeshProUGUI stagDialogueText = btn.GetComponentInChildren<TextMeshProUGUI>();
                textNode = stagDialogueText;
            }
            
            if (!String.IsNullOrEmpty(node.dialogueVersions.defaultVersion))
            {
                
                textNode.text = node.dialogueVersions.defaultVersion;
            } else {
                if (playerState.lastChoiceType == ChoiceType.Ego)
                {
                    textNode.text = node.dialogueVersions.egoVersion;
                }
                else if(playerState.lastChoiceType == ChoiceType.Empathy)
                {
                    textNode.text = node.dialogueVersions.empathyVersion;
                }
                else if(playerState.lastChoiceType == ChoiceType.Practicality)
                {
                    textNode.text = node.dialogueVersions.practicalityVersion;
                }
            }
        }

        if (node.choices != null)
        {
            choicesPanel.gameObject.SetActive(true);
            foreach (Choice choice in node.choices)
            {
                GameObject UIchoice = Instantiate(choicePrefab, choicesPanel);
                Button btn = UIchoice.GetComponentInChildren<Button>();
                TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                btnText.text = choice.choiceText;

                btn.onClick.AddListener(() =>
                {
                    ApplyChoice(choice);
                    SwitchNode(HelperFunctions.GetNextNodeBasedOnChoice(choice.nextNodeIdNullable));
                });
            }
        }
    }
    
    void ShowWarning(string warning)
    {
        UIWarning.text = warning;
        UIWarning.gameObject.SetActive(true);
        StartCoroutine(HideAfter());
    }

    IEnumerator HideAfter()
    {
        yield return new WaitForSeconds(5f);
        UIWarning.gameObject.SetActive(false);
    }

    void ApplyChoice(Choice choice)
    {
        if (choice.restart)
        {
            choice.nextNodeIdNullable = 0;
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
            return;
        }
        if (choice.exit)
        {
            Application.Quit();
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
        clearUI();
        playerState.pride += choice.prideChange;
        playerState.empathy += choice.empathyChange;
        playerState.practicality += choice.practicalityChange;
        if(!finalOutcomeCalculated) playerState.lastChoiceType = choice.prideChange > 0 ? ChoiceType.Ego : (choice.empathyChange != 0 ? ChoiceType.Empathy : ChoiceType.Practicality);
        Debug.Log("Pride: " + choice.prideChange + ",  Empathy: " + choice.empathyChange +  ",  Practicality: " + choice.practicalityChange + ", egoistic choice?: " + playerState.lastChoiceType);
    }
}
