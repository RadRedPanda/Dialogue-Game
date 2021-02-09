using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
	private DialogueGraphView _targetGraphView;
	private DialogueContainer _containerCache;

	private List<Edge> Edges => _targetGraphView.edges.ToList();
	private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

	public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
	{
		return new GraphSaveUtility
		{
			_targetGraphView = targetGraphView
		};
	}

	public void SaveGraph(string fileName)
	{
		if (!Edges.Any())
			return;

		DialogueContainer dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

		// gets all the edges where they have an input
		Edge[] connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

		// loops through and adds the info about each edge to the container
		foreach (Edge edge in connectedPorts)
		{
			DialogueNode outputNode = edge.output.node as DialogueNode;
			DialogueNode inputNode = edge.input.node as DialogueNode;

			dialogueContainer.NodeLinks.Add(new NodeLinkData
			{
				BaseNodeGuid = outputNode.GUID,
				PortName = edge.output.portName,
				TargetNodeGuid = inputNode.GUID
			});
		}

		// loops through and adds the info about each dialogueNode to the container
		foreach(DialogueNode dialogueNode in Nodes.Where(node => !node.EntryPoint))
		{
			dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
			{
				Guid = dialogueNode.GUID,
				DialogueText = dialogueNode.DialogueText,
				Position = dialogueNode.GetPosition().position,
				Expression = (Expression)dialogueNode.Expression.value,
				Speed = (DialogueSpeed)dialogueNode.Speed.value
			});
		}

		// creates a folder if there isn't one
		if (!AssetDatabase.IsValidFolder("Assets/Resources"))
			AssetDatabase.CreateFolder("Assets", "Resources");

		// creates the save file
		AssetDatabase.CreateAsset(dialogueContainer, "Assets/Resources/" + fileName + ".asset");
		AssetDatabase.SaveAssets();
	}

	public void LoadGraph(string fileName)
	{
		_containerCache = Resources.Load<DialogueContainer>(fileName);
		if(_containerCache == null)
		{
			EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist!", "OK");
			return;
		}

		ClearGraph();
		CreateNodes();
		ConnectNodes();
	}

	private void ClearGraph()
	{
		// set the initial node guid to the new initial node's guid
		Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks.Find(x => x.PortName == "Start").BaseNodeGuid;

		foreach(DialogueNode node in Nodes)
		{
			// leave the initial node
			if (node.EntryPoint)
				continue;

			// remove edges connected to this node
			Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

			// remove the node
			_targetGraphView.RemoveElement(node);
		}
	}

	private void CreateNodes()
	{
		foreach(DialogueNodeData nodeData in _containerCache.DialogueNodeData)
		{
			DialogueNode tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText, nodeData.Expression, nodeData.Speed);
			tempNode.GUID = nodeData.Guid;
			_targetGraphView.AddElement(tempNode);

			List<NodeLinkData> nodePorts = _containerCache.NodeLinks.Where(X => X.BaseNodeGuid == nodeData.Guid).ToList();
			nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName));
		}
	}

	private void ConnectNodes()
	{
		foreach(DialogueNode node in Nodes)
		{
			var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == node.GUID).ToList();
			for(int i = 0; i < connections.Count; i++)
			{
				string targetNodeGuid = connections[i].TargetNodeGuid;
				DialogueNode targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
				LinkNodes(node.outputContainer[i].Q<Port>(), (Port)targetNode.inputContainer[0]);

				targetNode.SetPosition(new Rect(
					_containerCache.DialogueNodeData.First(x => x.Guid == targetNodeGuid).Position,
					_targetGraphView.DefaultNodeSize
				));
			}
		}
	}

	private void LinkNodes(Port output, Port input)
	{
		Edge tempEdge = new Edge
		{
			output = output,
			input = input
		};
		tempEdge.input.Connect(tempEdge);
		tempEdge.output.Connect(tempEdge);
		_targetGraphView.Add(tempEdge);
	}
}
