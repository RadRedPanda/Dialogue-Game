using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
	public GameObject PauseMenu;
	public InventoryController InventoryC;
	public DialogueController DialogueC;
	public JournalController JournalC;
	public GameObject Notebook;
	public Transform KeywordContainer;

	// Use this for initialization
	void Start()
	{
		Canvas c = GetComponent<Canvas>();
		InventoryC.Setup(this);
		DialogueC.Setup(this);
		JournalC.Setup(this);
	}

	void Update()
	{
		//////////////////////////////// look at this, fix tomorrow, idk what's even broken about this rn though or what it's supposed to do
		GraphicRaycaster g = GetComponent<GraphicRaycaster>();
		List<RaycastResult> results = new List<RaycastResult>();

		EventSystem m_EventSystem = GetComponent<EventSystem>();
		PointerEventData m_PointerEventData;
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		g.Raycast(m_PointerEventData, results);
		//print(results.Count);
		/////////////////////////////////////////////
	}

	public void OpenMenu()
	{

	}

	public void CloseMenu()
	{

	}

	public void OpenInventory()
	{
		InventoryC.OpenInventory();
	}

	public void CloseInventory()
	{
		InventoryC.CloseInventory();
	}

	// dialogue stuff
	public void StartDialogue(DialogueManager dm)
	{
		DialogueC.StartDialogue(dm);
	}

	public void NextDialogue()
	{
		DialogueC.NextDialogue();
	}

	public void EndDialogue()
	{
		DialogueC.EndDialogue();
	}

	public void OpenJournal()
	{
		JournalC.OpenJournal();
	}

	public void CloseJournal()
	{
		JournalC.CloseJournal();
	}

	// returns true if the mouse is currently over the canvas object, doesn't care if there's anything in between
	public bool IsHoveringObject(GameObject g)
	{
		PointerEventData pointerData = new PointerEventData(EventSystem.current)
		{
			pointerId = -1,
		};

		pointerData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerData, results);
		var res = results.Find(x => x.gameObject == g);
		return res.gameObject != null;
	}
}
