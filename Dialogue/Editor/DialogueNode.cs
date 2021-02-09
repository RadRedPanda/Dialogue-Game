using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

public class DialogueNode : Node
{
	public string GUID;

	public string DialogueText;

	public EnumField Expression;

	public EnumField Speed;

    public bool EntryPoint;	
}
