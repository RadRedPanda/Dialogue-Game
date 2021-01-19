using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
	private DialogueGraphView _graphView;

	[MenuItem("Graph/Dialogue Graph")]
	public static void OpenDialogueGraphWindow()
	{
		DialogueGraph window = GetWindow<DialogueGraph>();
		window.titleContent = new GUIContent("Dialogue Graph");
	}

	private void OnEnable()
	{
		ConstructGraphView();
		GenerateToolbar();
	}

	private void OnDisable()
	{
		rootVisualElement.Remove(_graphView);
	}

	private void ConstructGraphView()
	{
		_graphView = new DialogueGraphView();

		_graphView.StretchToParentSize();
		rootVisualElement.Add(_graphView);
	}

	private void GenerateToolbar()
	{
		Toolbar toolbar = new Toolbar();

		Button nodeCreateButton = new Button(() =>
		{
			_graphView.CreateNode("Dialogue Node");
		});
		nodeCreateButton.text = "Create Node";
		toolbar.Add(nodeCreateButton);

		rootVisualElement.Add(toolbar);
	}
}
