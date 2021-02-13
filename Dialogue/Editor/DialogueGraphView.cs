using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
	public readonly Vector2 DefaultNodeSize = new Vector2(x: 150, y: 200);

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

		AddElement(CreateStartNode());
	}
	#endregion
	#region Public Methods
	//	Summary: creates a new dialogue node with an input port and a button to create new choices
	//	Input:
	//		nodeName - the name of the new node
	//	Output:
	//		the created dialogue node
	public DialogueNode CreateDialogueNode(string nodeName, Expression expression = Expression.Normal, DialogueSpeed speed = DialogueSpeed.Normal)
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

		dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

		Button button = new Button(() =>
		{
			AddChoicePort(dialogueNode);
		});
		button.text = "New Choice";
		dialogueNode.titleContainer.Add(button);

		dialogueNode.Expression = new EnumField(expression);
		dialogueNode.titleContainer.Add(dialogueNode.Expression);

		dialogueNode.Speed = new EnumField(speed);
		dialogueNode.titleContainer.Add(dialogueNode.Speed);

		TextField textField = new TextField(-1, true, false, ' ');
		textField.RegisterValueChangedCallback(evt =>
		{
			dialogueNode.DialogueText = evt.newValue;
		});
		textField.SetValueWithoutNotify(dialogueNode.title);
		dialogueNode.mainContainer.Add(textField);

		dialogueNode.RefreshExpandedState();
		dialogueNode.RefreshPorts();
		dialogueNode.SetPosition(new Rect(position: Vector2.zero, DefaultNodeSize));

		return dialogueNode;
	}

	//	Summary: creates a new dialogue node and adds it to the graph view
	//	Input:
	//		nodeName - the name of the new node
	public void CreateDNode(string nodeName)
	{
		AddElement(CreateDialogueNode(nodeName));
	}

	//	Summary: adds a new output port to the node
	//	Inputs:
	//		dialogueNode - the node we are adding a new port to
	public void AddChoicePort(DialogueNode dialogueNode, string overridenPortName = "")
	{
		Port generatedPort = GeneratePort(dialogueNode, Direction.Output);

		// deletes the duplicate label, for some reason it makes it so you can't drag out of the port? super buggy
		//Label oldLabel = generatedPort.contentContainer.Q<Label>("type");
		//generatedPort.contentContainer.Remove(oldLabel);
		int outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;

		string choicePortName = string.IsNullOrEmpty(overridenPortName) ? "Choice " + (outputPortCount + 1) : overridenPortName;

		TextField textField = new TextField()
		{
			name = string.Empty,
			value = choicePortName
		};
		textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
		generatedPort.contentContainer.Add(new Label(""));
		generatedPort.contentContainer.Add(textField);
		Button deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort))
		{
			text = "X"
		};
		generatedPort.contentContainer.Add(deleteButton);

		generatedPort.portName = choicePortName;
		dialogueNode.outputContainer.Add(generatedPort);
		dialogueNode.RefreshExpandedState();
		dialogueNode.RefreshPorts();
	}

	//	Summary: creates a new node and adds it to the graph view
	//	Input:
	//		keyword - the keyword which begins this line
	public EntryNode CreateEntryNode(Keyword keyword)
	{
		EntryNode entryNode = new EntryNode
		{
			title = "Entry Node",
			GUID = Guid.NewGuid().ToString()
		};

		entryNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

		entryNode.Keyword = new EnumField(keyword);
		entryNode.titleContainer.Add(entryNode.Keyword);
		
		Port generatedPort = GeneratePort(entryNode, Direction.Output);
		generatedPort.portName = "Entry";
		entryNode.outputContainer.Add(generatedPort);

		entryNode.RefreshExpandedState();
		entryNode.RefreshPorts();
		entryNode.SetPosition(new Rect(position: Vector2.zero, DefaultNodeSize));

		return entryNode;
	}

	//	Summary: creates a new entry node and adds it to the graph view
	//	Input:
	//		keyword - the keyword of the new node
	public void CreateENode(Keyword keyword)
	{
		AddElement(CreateEntryNode(keyword));
	}

	//	Summary: creates the first node of the view
	//	Output:
	//		the node created
	public BaseNode CreateStartNode()
	{
		BaseNode node = new BaseNode
		{
			title = "Start",
			GUID = Guid.NewGuid().ToString()
		};

		Port generatedPort = GeneratePort(node, Direction.Output);
		generatedPort.portName = "Start";
		node.outputContainer.Add(generatedPort);

		node.RefreshExpandedState();
		node.RefreshPorts();

		node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

		return node;
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
			if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
			{
				compatiblePorts.Add(port);
			}
		});

		return compatiblePorts;
	}
	#endregion
	#region Private Methods
	//	Summary: creates a new port on a node
	//	Inputs:
	//		node - the node we are adding a new port to
	//		portDirection - whether the port is an input or output port (left or right)
	//		capacity - how many ports it can hold
	//	Output:
	//		the port created
	private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
	{
		return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
	}

	private void RemovePort(Node node, Port generatedPort)
	{
		var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);
		if (targetEdge.Any())
		{
			Edge edge = targetEdge.First();
			edge.input.Disconnect(edge);
			RemoveElement(targetEdge.First());
		}

		node.outputContainer.Remove(generatedPort);
		node.RefreshPorts();
		node.RefreshExpandedState();

	}
	#endregion
}