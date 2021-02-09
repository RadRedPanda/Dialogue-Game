using Assets.Scripts;
using System;
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
