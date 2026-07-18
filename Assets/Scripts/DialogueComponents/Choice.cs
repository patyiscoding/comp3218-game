using System;
using UnityEngine;

[Serializable]
public class Choice {
    [TextArea(1, 7)]
    public string choiceText;
    
    public int nextNodeId = Int32.MinValue; // parser doesn't support nullable int/float
    public int? nextNodeIdNullable;

    public int prideChange;
    public int empathyChange;
    public int practicalityChange;
    public bool restart = false;
    public bool exit = false;
}
