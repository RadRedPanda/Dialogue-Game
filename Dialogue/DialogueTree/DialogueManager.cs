using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DialogueManager : MonoBehaviour {

	public Dictionary<string, DialogueClass> DialogueDictionary;
	public string Filename;

    private string startNode;	// the guid of the node all conversations start at
	private string currentNode;	// the guid of the current node we're talking about
	private int currentSentence;// the line number of the sentence we're on
	private DialogueContainer _containerCache;

	// Use this for initialization
	void Start () {
		if (string.IsNullOrEmpty(Filename))
			loadDialogueFromFile(transform.name);
		else
			loadDialogueFromFile(Filename);
	}

    public string startConversation() {
        currentSentence = 0;
        currentNode = startNode;
        return DialogueDictionary[currentNode].Dialogue[currentSentence];
    }

    public string nextDialogue() {
        if (currentSentence < DialogueDictionary[currentNode].Dialogue.Count - 1) {
            currentSentence++;
            return DialogueDictionary[currentNode].Dialogue[currentSentence];
        }
        return "";
    }
	
    public string prevDialogue() {
        if (currentSentence > 0) {
            currentSentence--;
            return DialogueDictionary[currentNode].Dialogue[currentSentence];
        }
        return "";
    }
	
    public void chooseOption(string choice) {
		Keyword keyword = DialogueClass.ParseStringToKeyword(choice);
		currentNode = DialogueDictionary[currentNode].Options[keyword];
        currentSentence = 0;
    }

    public void loadDialogueFromFile(string path) {
		_containerCache = Resources.Load<DialogueContainer>(path);
		DialogueDictionary = new Dictionary<string, DialogueClass>();
		if (_containerCache == null)
		{
			Debug.Log(path + " not found!");
			return;
		}
		startNode = _containerCache.NodeLinks[0].TargetNodeGuid;
		CreateNodes();
	}

	private void CreateNodes()
	{
		foreach (DialogueNodeData nodeData in _containerCache.DialogueNodeData)
		{
			// get all outgoing connections
			List<NodeLinkData> nodeLinks = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();

			// add it to the dictionary
			DialogueDictionary.Add(nodeData.Guid, new DialogueClass(nodeData, nodeLinks));
		}
	}
}