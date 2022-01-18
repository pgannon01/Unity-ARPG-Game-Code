using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/*
    Without the below code, any time we try to rename our Dialogue assets, it will actually change the asset from a Dialogue SO, to a Dialogue Node SO
    Obviously, this is a bug and one that could potentially cause a lot of trouble for us
    The below code is meant to ensure that Unity can change the name, but keep it as the correct asset
*/

namespace RPG.Dialogue.Editor
{
    public class DialogueModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            // Make sure the asset being moved is ONLY a dialogue
            Dialogue dialogue = AssetDatabase.LoadMainAssetAtPath(sourcePath) as Dialogue;
            if (dialogue == null)
            {
                return AssetMoveResult.DidNotMove;
            }
            
            // Make sure it's JUST a rename of the file, rather than an actual moving of directories
            if (Path.GetDirectoryName(sourcePath) != Path.GetDirectoryName(destinationPath))
            {
                // If something's actually changing directories, return early
                return AssetMoveResult.DidNotMove;
            }

            dialogue.name = Path.GetFileNameWithoutExtension(destinationPath);

            return AssetMoveResult.DidNotMove;
        }
    }
}