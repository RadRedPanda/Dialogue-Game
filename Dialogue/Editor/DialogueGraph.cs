using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
	private DialogueGraphView _graphView;
	private string _fileName = "New Narrative";

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

		TextField fileNameTextField = new TextField(label: "File Name:");
		fileNameTextField.SetValueWithoutNotify(_fileName);
		fileNameTextField.MarkDirtyRepaint();
		fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
		toolbar.Add(fileNameTextField);

		toolbar.Add(new Button(() => RequestDataOperation(true))
		{
			text = "Save Data"
		});

		toolbar.Add(new Button(() => RequestDataOperation(false))
		{
			text = "Load Data"
		});

		toolbar.Add(new Button(() =>
		{
			_graphView.CreateDNode("Dialogue Node");
		})
		{
			text = "Create Dialogue Node"
		});

		toolbar.Add(new Button(() =>
		{
			_graphView.CreateENode(0);
		})
		{
			text = "Create Entry Node"
		});

		rootVisualElement.Add(toolbar);
	}

	private void RequestDataOperation(bool save)
	{
		if (string.IsNullOrEmpty(_fileName))
		{
			EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
			return;
		}

		GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(_graphView);

		if (save)
			saveUtility.SaveGraph(_fileName);
		else
			saveUtility.LoadGraph(_fileName);
	}
}
