using System;

[Serializable]
public class Node
{
    public int id;
    public string speaker;
    public string area;
    public AnimalAction[] animalActions;
    public DialogueVersions dialogueVersions;
    public Choice[] choices;
    public string camera;
    public int nextNodeId = Int32.MinValue; // parser doesn't support nullable int/float
    public int? nextNodeIdNullable;
    public bool followToNextNodeAutomatically = false;
    public float wait = float.MinValue;
    public float? waitNullable;
    public bool calculateOutcome = false;
    public string playSound;
}
