using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HelperFunctions {
    public static GameObject FindChildByNameContaining(GameObject parent, string contains)
    {
        GameObject child = parent.transform
            .Cast<Transform>()
            .FirstOrDefault(t => t.name.Contains(contains))
            ?.gameObject;
        
        Debug.Log("Found child in parent " + parent.gameObject.name + ": " + child);
        return child;
    }
    
    public static List<GameObject> FindChildrenByNameContaining(GameObject parent, string contains)
    {
        List<GameObject> children = parent.transform
            .Cast<Transform>()
            .Where(t => t.name.Contains(contains))
            .Select(t => t.gameObject)
            .ToList();
        
        return children;
    }
    
    public static Node GetNodeById(int nodeId)
    {
        if (DialogueManager.Instance.nodes.Length > nodeId)
        {
            return DialogueManager.Instance.nodes[nodeId];
        }
        else
        {
            throw new IndexOutOfRangeException();
        }
    }
    
    public static Node GetMinusOneNode()
    {
        int nextNodeId = DialogueManager.Instance.currentNode.id - 1;
        return GetNodeById(nextNodeId);
    }
    
    public static Node GetNextNodeNonReliantOnChoice()
    {
        int nextNodeId = DialogueManager.Instance.currentNode.nextNodeIdNullable ?? DialogueManager.Instance.currentNode.id + 1;
        return GetNodeById(nextNodeId);
    }
    
    public static Node GetNextNodeBasedOnChoice(int? id)
    {
        int nextNodeId = id ?? DialogueManager.Instance.currentNode.id + 1;
        return GetNodeById(nextNodeId);
    }
}
