using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour {

    public List<Assets.Scripts.DialogueNode> nodeTree;
    public List<string> keywords;

    private int startNode;
    private int currentSentence;
    private int currentNode;

	// Use this for initialization
	void Start () {
        startNode = 0;
        //loadDialogueFromFile(transform.name);
	}

    public string startConversation() {
        currentSentence = 0;
        currentNode = startNode;
        startNode = 1;
        return nodeTree[currentNode].dialogue[currentSentence];
    }

    public string nextDialogue() {
        if (currentSentence < nodeTree[currentNode].dialogue.Count - 1) {
            currentSentence++;
            return nodeTree[currentNode].dialogue[currentSentence];
        }
        return "";
    }

    public string prevDialogue() {
        if (currentSentence > 0) {
            currentSentence--;
            return nodeTree[currentNode].dialogue[currentSentence];
        }
        return "";
    }

    public void chooseOption(string choice) {
        foreach (int option in nodeTree[currentNode].options)
            if (nodeTree[option].choice == choice)
                currentNode = option;
        currentSentence = 0;
    }

    public void loadDialogueFromFile(string path) {

    }
}
