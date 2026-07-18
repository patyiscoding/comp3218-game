
using System;
using UnityEngine;

[Serializable]
public class DialogueVersions
{
    [TextArea(1, 7)]
    public string defaultVersion;
    [TextArea(1, 7)]
    public string egoVersion;
    [TextArea(1, 7)]
    public string empathyVersion;
    [TextArea(1, 7)]
    public string practicalityVersion; 
}