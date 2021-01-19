using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
	private readonly Vector2 defaultNodeSize = new Vector2(x: 150, y: 200);

	#region Constructors
	public DialogueGraphView()
	{
		styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
		// allows zooming in and out
		SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

		this.AddManipulator(new ContentDragger());
		this.AddManipulator(new SelectionDragger());
		this.AddManipulator(new RectangleSelector());

		GridBackground grid = new GridBackground();
		Insert(0, grid);
		grid.StretchToParentSize();

		AddElement(GenerateEntryPointNode());
	}
	#endregion
	#region Public Methods
	//	Summary: creates a new dialogue node with an input port and a button to create new choices
	//	Input:
	//		nodeName - the name of the new node
	//	Output:
	//		the created dialogue node
	public DialogueNode CreateDialogueNode(string nodeName)
	{
		DialogueNode dialogueNode = new DialogueNode
		{
			title = nodeName,
			DialogueText = nodeName,
			GUID = Guid.NewGuid().ToString()
		};

		Port inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
		inputPort.portName = "Input";
		dialogueNode.inputContainer.Add(inputPort);

		Button button = new Button(() =>
		{
			AddChoicePort(dialogueNode);
		});
		dialogueNode.titleContainer.Add(button);
		button.text = "New Choice";
		dialogueNode.RefreshExpandedState();
		dialogueNode.RefreshPorts();
		dialogueNode.SetPosition(new Rect(position: Vector2.zero, defaultNodeSize));

		return dialogueNode;
	}

	//	Summary: creates a new node and adds it to the graph view
	//	Input:
	//		nodeName - the name of the new node
	public void CreateNode(string nodeName)
	{
		AddElement(CreateDialogueNode(nodeName));
	}
	#endregion
	#region Override Methods
	//	Summary: gets a list of the ports that we should be able to connect to
	//	Input:
	//		startPort - the port we are checking
	//		nodeAdapter - not used
	//	Output:
	//		a list of ports which are compatible with startPort
	public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
	{
		List<Port> compatiblePorts = new List<Port>();

		ports.ForEach((port) =>
		{
			if (startPort != port && startPort.node != port.node) // need to add check to make sure input -> output////////////////////////////
			{
				compatiblePorts.Add(port);
			}
		});

		return compatiblePorts;
	}
	#endregion
	#region Private Methods
	//	Summary: adds a new output port to the node
	//	Inputs:
	//		dialogueNode - the node we are adding a new port to
	private void AddChoicePort(DialogueNode dialogueNode)
	{
		Port generatedPort = GeneratePort(dialogueNode, Direction.Output);
		int outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
		generatedPort.portName = $"Choice {outputPortCount}";

		dialogueNode.outputContainer.Add(generatedPort);
		dialogueNode.RefreshExpandedState();
		dialogueNode.RefreshPorts();
	}

	//	Summary: creates the first node of the view
	//	Output:
	//		the node created
	private DialogueNode GenerateEntryPointNode()
	{
		DialogueNode node = new DialogueNode
		{
			title = "Start",
			GUID = Guid.NewGuid().ToString(),
			DialogueText = "",
			EntryPoint = true
		};

		Port generatedPort = GeneratePort(node, Direction.Output);
		generatedPort.portName = "Next";
		node.outputContainer.Add(generatedPort);

		node.RefreshExpandedState();
		node.RefreshPorts();

		node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

		return node;
	}

	//	Summary: creates a new port on a node
	//	Inputs:
	//		node - the node we are adding a new port to
	//		portDirection - whether the port is an input or output port (left or right)
	//		capacity - how many ports it can hold
	//	Output:
	//		the port created
	private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
	{
		return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
	}
	#endregion
}