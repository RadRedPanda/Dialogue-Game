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
	private List<BaseNode> Nodes => _targetGraphView.nodes.ToList().Cast<BaseNode>().ToList();

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
			BaseNode outputNode = edge.output.node as BaseNode;
			BaseNode inputNode = edge.input.node as BaseNode;

			dialogueContainer.NodeLinks.Add(new NodeLinkData
			{
				BaseNodeGuid = outputNode.GUID,
				PortName = edge.output.portName,
				TargetNodeGuid = inputNode.GUID
			});
		}

		// loops through and adds the info about each node to the container
		List<BaseNode> nodes = Nodes.ToList();
		foreach (Node node in nodes)
		{
			switch(node){
				case DialogueNode n:
					dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
					{
						Guid = n.GUID,
						DialogueText = n.DialogueText,
						Position = n.GetPosition().position,
						Expression = (Expression)n.Expression.value,
						Speed = (DialogueSpeed)n.Speed.value
					});
					break;
				case EntryNode n:
					dialogueContainer.EntryNodeData.Add(new EntryNodeData
					{
						Guid = n.GUID,
						Keyword = (Keyword)n.Keyword.value,
						Position = n.GetPosition().position
					});
					break;
				case BaseNode n:
					dialogueContainer.StartNodeData = new StartNodeData
					{
						Guid = n.GUID,
						Position = n.GetPosition().position
					};
					break;
			}
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

		clearGraph();
		createNodes();
		connectNodes();
	}

	private void clearGraph()
	{
		foreach(Node node in Nodes)
		{
			// remove edges connected to this node
			Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

			// remove the node
			_targetGraphView.RemoveElement(node);
		}
	}

	private void createNodes()
	{
		BaseNode startNode = _targetGraphView.CreateStartNode();
		startNode.GUID = _containerCache.StartNodeData.Guid;
		_targetGraphView.AddElement(startNode);
		startNode.SetPosition(new Rect(_containerCache.StartNodeData.Position, _targetGraphView.DefaultNodeSize));

		foreach (DialogueNodeData nodeData in _containerCache.DialogueNodeData)
		{
			DialogueNode tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText, nodeData.Expression, nodeData.Speed);
			tempNode.GUID = nodeData.Guid;
			_targetGraphView.AddElement(tempNode);

			List<NodeLinkData> nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
			nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName));

			setNodePosition(tempNode, nodeData.Position);
		}

		foreach (EntryNodeData nodeData in _containerCache.EntryNodeData)
		{
			EntryNode tempNode = _targetGraphView.CreateEntryNode(nodeData.Keyword);
			tempNode.GUID = nodeData.Guid;
			_targetGraphView.AddElement(tempNode);

			setNodePosition(tempNode, nodeData.Position);
		}
	}

	private void setNodePosition(BaseNode node, Vector2 position){

		node.SetPosition(new Rect(
			position,
			_targetGraphView.DefaultNodeSize
		));
	}

	private void connectNodes()
	{
		foreach(BaseNode node in Nodes)
		{
			var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == node.GUID).ToList();
			for(int i = 0; i < connections.Count; i++)
			{
				string targetNodeGuid = connections[i].TargetNodeGuid;
				BaseNode targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
				linkNodes(node.outputContainer[i].Q<Port>(), (Port)targetNode.inputContainer[0]);

				targetNode.SetPosition(new Rect(
					_containerCache.DialogueNodeData.First(x => x.Guid == targetNodeGuid).Position,
					_targetGraphView.DefaultNodeSize
				));
			}
		}
	}

	private void linkNodes(Port output, Port input)
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
