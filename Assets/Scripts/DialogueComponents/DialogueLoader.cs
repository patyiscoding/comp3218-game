using UnityEngine;

public class DialogueLoader
{
    public DialogueRoot dialogueRoot;
    public DialogueLoader()
    {
        TextAsset json = Resources.Load<TextAsset>("Dialogue");
        dialogueRoot = JsonUtility.FromJson<DialogueRoot>(json.text);
        
        if (dialogueRoot == null || dialogueRoot.scenes == null || dialogueRoot.scenes.Length == 0)
        {
            Debug.LogError("Dialogue JSON failed to load");
            return;
        }

        int nodeIndex = 0;
        foreach (Scene scene in dialogueRoot.scenes)
        {
            foreach (Node node in scene.sceneNodes)
            {
                node.id = nodeIndex;
                nodeIndex++;
                
                node.nextNodeIdNullable = node.nextNodeId >= 0 ? node.nextNodeId : null;
                node.waitNullable = node.wait >= 0 ? node.wait : null;
                
                if (node.choices == null) continue;
                foreach (Choice choice in node.choices)
                {
                    choice.nextNodeIdNullable = choice.nextNodeId >= 0 ? choice.nextNodeId : null;
                }
            }
        }

        Debug.Log("Dialogue loaded successfully. First scene: " + dialogueRoot.scenes[0]);
        Debug.Log(dialogueRoot.scenes.Length + " scenes loaded");
    }
}