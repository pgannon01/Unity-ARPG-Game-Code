using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RPG.Dialogue
{
    
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] List<DialogueNode> nodes = new List<DialogueNode> (); // An array of Dialoge Nodes
        // Want this SO to include a serialze field of Dialogue Nodes, where we can include multiple of them
        [SerializeField] Vector2 newNodeOffset = new Vector2(250, 0);

        Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>(); // Looking up the dialogue node by its ID, so we need a Dictionary

#if UNITY_EDITOR // Makes sure the following functions are ONLY being called in the Unity Editor, and not when we've built the game, or anything like that
        private void Awake() 
        {
            OnValidate(); // Need to call this here otherwise our fully built game won't work!
        }
#endif

        private void OnValidate() // NOTE: This will never get called in a fully built game
        {
            // Called either when a value is changed in the inspector (You've modified some of the data in this dialogue)
            // OR is called when the SO is loaded

            nodeLookup.Clear(); // Start from an empty dictionary so we can then rebuild it
            foreach (DialogueNode node in GetAllNodes())
            {
                // Store the lookup in the dictionary
                nodeLookup[node.name] = node; // Look for this specific node using its ID in the Dictionary
            }
        }

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            // an IEnumerable is something that can be foreached over
            foreach (string childID in parentNode.GetChildren())
            {
                if (nodeLookup.ContainsKey(childID)) // Checks if the dictionary HAS this key
                {
                    yield return nodeLookup[childID];
                }
            }
        }

        public IEnumerable<DialogueNode> GetPlayerChildren(DialogueNode currentNode)
        {
            foreach (DialogueNode nodes in GetAllChildren(currentNode))
            {
                if (nodes.IsPlayerSpeaking())
                {
                    yield return nodes;
                }
            }
        }

        public IEnumerable<DialogueNode> GetAIChildren(DialogueNode currentNode)
        {
            foreach (DialogueNode nodes in GetAllChildren(currentNode))
            {
                if (!nodes.IsPlayerSpeaking())
                {
                    yield return nodes;
                }
            }
        }

#if UNITY_EDITOR
        public void CreateNode(DialogueNode creatingNode)
        {
            DialogueNode newNode = MakeNode(creatingNode);
            Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node");
            Undo.RecordObject(this, "Added Dialogue Node");
            AddNode(newNode);
        }

        public void DeleteNode(DialogueNode nodeToDelete)
        {
            Undo.RecordObject(this, "Deleted Dialogue Node");
            nodes.Remove(nodeToDelete);
            OnValidate();
            foreach (DialogueNode node in GetAllNodes())
            {
                node.RemoveChild(nodeToDelete.name);
            }
            Undo.DestroyObjectImmediate(nodeToDelete);
        }

        private void AddNode(DialogueNode newNode)
        {
            nodes.Add(newNode);
            OnValidate();
        }

        private DialogueNode MakeNode(DialogueNode creatingNode)
        {
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = Guid.NewGuid().ToString();
            if (creatingNode != null)
            {
                creatingNode.AddChild(newNode.name);
                newNode.SetPlayerSpeaking(!creatingNode.IsPlayerSpeaking());
                newNode.SetPosition(creatingNode.GetPosition().position + newNodeOffset);
            }

            return newNode;
        }
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count == 0)
            {
                DialogueNode newNode = MakeNode(null);
                AddNode(newNode);
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (DialogueNode node in GetAllNodes())
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this); // Add any new node as a subobject to the dialogue's asset file
                        // This way we can save the nodes individually
                    }
                }
            }
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
