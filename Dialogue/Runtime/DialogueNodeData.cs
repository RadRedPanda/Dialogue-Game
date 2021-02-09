using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
	public string Guid;
	public Expression Expression;
	public DialogueSpeed Speed;
	public string DialogueText;
	public Vector2 Position;
}
