using System.Collections;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tenkoku.Core;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    public Scene[] scenes;
    public Node[] nodes;
    public Node currentNode;
    private Coroutine nodeWaitCoroutine;
    private Node alreadyChosenFollowingNode;
    public bool hasEndNodeAnimationsToRun;
    private bool hasTimerExpired;
    private bool currentNodeHasTimer;
    
    public TMP_Text UIWarning;
    public TMP_Text UINarrative;
    public Transform animalDialogue;
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
    public TextMeshProUGUI timerText;
    
    private TenkokuModule tenkokuSky;
    private bool prideEffectTriggered;
    
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
        
        tenkokuSky = FindObjectOfType<TenkokuModule>();
        if (tenkokuSky == null)
        {
            Debug.LogWarning("TenkokuModule not found in scene. Weather effects will not work.");
        }
        
        prideEffectTriggered = false;
        
        nodeWaitCoroutine = null;
        alreadyChosenFollowingNode = null;
        hasTimerExpired = false;
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

        if (currentNode.choices != null && currentNode.choices.Length > 0 && currentNodeHasTimer)
        {
            if (currentNode.timer > 0)
            {
                timerText.gameObject.SetActive(true);
                currentNode.timer -= Time.deltaTime;
                timerText.text = currentNode.timer.ToString("0.00");
            }
            else if (!hasTimerExpired)
            {
                hasTimerExpired = true;
                System.Random rng = new System.Random();
                var random = rng.Next(0, currentNode.choices.Length);
                print("Timer expired, randomly selecting choice: " + random);
                ApplyChoiceAndMoveToNextNode(currentNode.choices[random]);
            }
        }
        
        // Update weather based on player stats
        UpdateWeatherEffects();
    }
    
    void UpdateWeatherEffects()
    {
        if (tenkokuSky == null) return;
        
        // trigger rain and lightning
        if (playerState.pride >= 5 && !prideEffectTriggered)
        {
            prideEffectTriggered = true;
            Debug.Log("Pride is 5! Triggering rain and lightning...");
            
            // disable auto weather
            tenkokuSky.weather_setAuto = false;
            
            tenkokuSky.weather_cloudCumulusAmt = 0.9f;
            tenkokuSky.weather_cloudAltoStratusAmt = 0.8f;
            tenkokuSky.weather_cloudCirrusAmt = 0.3f;
            
            tenkokuSky.weather_OvercastAmt = 0.4f;
            tenkokuSky.weather_RainAmt = 0.8f;
            tenkokuSky.weather_lightning = 0.5f;
            tenkokuSky.weather_FogAmt = 1f;
            tenkokuSky.weather_WindAmt = 0.5f;
            
            tenkokuSky.weather_forceUpdate = true;
        }
        else if (playerState.pride < 5 && prideEffectTriggered)
        {
            prideEffectTriggered = false;
            Debug.Log("Pride dropped below 5! Clearing rain and lightning...");
            
            // clear weather
            tenkokuSky.weather_cloudCumulusAmt = 0.0f;
            tenkokuSky.weather_cloudAltoStratusAmt = 0.0f;
            tenkokuSky.weather_OvercastAmt = 0.0f;
            tenkokuSky.weather_RainAmt = 0.0f;
            tenkokuSky.weather_lightning = 0.0f;
            tenkokuSky.weather_FogAmt = 0.0f;
            tenkokuSky.weather_WindAmt = 0.0f;
            
            tenkokuSky.weather_setAuto = true;
            tenkokuSky.weather_forceUpdate = true;
        }
    }

    public void ApplyChoiceAndMoveToNextNode(Choice choice)
    {
        timerText.gameObject.SetActive(false);
        ApplyChoice(choice);
        SwitchNode(HelperFunctions.GetNextNodeBasedOnChoice(choice.nextNodeIdNullable));
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
        choicesPanel.gameObject.SetActive(false);
        stagDialogue.gameObject.SetActive(false);
        animalDialogue.gameObject.SetActive(false);
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in stagDialogue)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in animalDialogue)
        {
            Destroy(child.gameObject);
        }
    }

    private void calculateOutcome()
    {
        double maxScore = Math.Max(playerState.pride, Math.Max(playerState.empathy, playerState.practicality));
        finalOutcomeCalculated = true;
        const double EPS = 1e-6; // small tolerance

        if (Math.Abs(maxScore - playerState.pride) < EPS)
        {
            playerState.lastChoiceType = ChoiceType.Ego;
            return;
        }
        if (Math.Abs(maxScore - playerState.empathy) < EPS)
        {
            playerState.lastChoiceType = ChoiceType.Empathy;
            return;
        }

        // fallback to practicality
        playerState.lastChoiceType = ChoiceType.Practicality;
    }
    
    public void ShowNode(Node node)
    {
        currentNode = node;
        
        // clear UI
        clearUI();
        
        hasTimerExpired = false;
        currentNodeHasTimer = currentNode.timer > 0;
        timerText.gameObject.SetActive(false);
        
        // switch camera
        Debug.Log("START node: " + node.id + " Animal actions: " + node.animalActions);
        Debug.Log("Pride: " + playerState.pride + " Empathy: " + playerState.empathy + " Practicality: " + playerState.practicality);
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
                animalDialogue.gameObject.SetActive(true);
                GameObject UIAnimalDialoguePanel = Instantiate(stagDialoguePrefab, animalDialogue);
                Button btn = UIAnimalDialoguePanel.GetComponentInChildren<Button>();
                TextMeshProUGUI animalDialogueText = btn.GetComponentInChildren<TextMeshProUGUI>();
                textNode = animalDialogueText;
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
        
        // handle choices
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
                    ApplyChoiceAndMoveToNextNode(choice);
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
