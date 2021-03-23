using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeywordScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public string Word;
	public float AlphaSpeed = 0.01f;
	public DialogueController DialogueC;
	public PanelScript CurrentPanel;
	[HideInInspector]
	public bool picked;     // true if player is holding word with mouse
	[HideInInspector]
	public bool homeless;   // true if it has nowhere to return to, prep for deletion
	[HideInInspector]
	public Transform OldParent;
	[HideInInspector]
	public Vector3 StartPos;
	
	private InventoryController inventoryC;
	private CanvasController canvasC;
	private Text text;
	private Vector3 prevPointerPos;
	private RectTransform rectTransform;

	// Use this for initialization
	void Awake()
	{
		picked = false;
		homeless = false;
		text = GetComponent<Text>();
		rectTransform = GetComponent<RectTransform>();
		text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
		StartCoroutine(phaseIn());
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (picked)
		{
			transform.position = transform.position + Input.mousePosition - prevPointerPos;
			prevPointerPos = Input.mousePosition;
		}
	}

	public void SetUp(string w, Vector3 pos, bool isCapital, CanvasController cc)
	{
		text = GetComponent<Text>();
		if (isCapital)
			text.text = char.ToUpper(w[0]) + w.Substring(1);
		else
			text.text = w;
		Word = text.text;
		transform.position = pos;
		StartPos = transform.localPosition;
		canvasC = cc;
		inventoryC = canvasC.InventoryC;
		DialogueC = canvasC.DialogueC;
	}

	// call this if keyword needs to be deleted
	public void MakeHomeless()
	{
		StopAllCoroutines();
		homeless = true;
	}

	public void SetNewHome(PanelScript newHome)
	{
		homeless = false;
		CurrentPanel = newHome;
		transform.SetParent(CurrentPanel.transform);
		StartPos = newHome.GetMiddle(rectTransform.rect.width * rectTransform.localScale.x);
	}

	public void SendHome()
	{
		text.raycastTarget = true;
		StartCoroutine(returnToStartPos());
	}

	public void DeleteThis()
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}

	#region Handler Methods
	// player dragging mouse on word
	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (!canvasC.JournalC.JournalOpen)
		{
			prevPointerPos = eventData.position;
			picked = true;
			OldParent = transform.parent;
			transform.SetParent(canvasC.KeywordContainer);
			text.raycastTarget = false;
			StopAllCoroutines();
		}
	}

	// released the mouse hold on the word
	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		picked = false;

		if (inventoryC.AddKeywordToPanel(this))
			return;

		// checks if we're hovering the mouse over the character the player is talking to
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
			if (DialogueC.IsSpeakerCollider(hit.collider))
				if (!DialogueC.IsHoveringDialogueBox())
					if (DialogueC.ChooseDialogue(Word))
						if (CurrentPanel == null)
							homeless = true;

		if (homeless)
		{
			DeleteThis();
		}
		else
		{
			transform.SetParent(OldParent);
			SendHome();
		}
	}
	#endregion
	#region Coroutine Methods
	// phases in the alpha from transparent to opaque
	IEnumerator phaseIn()
	{
		for (float a = 0; a < 1; a += AlphaSpeed)
		{
			text.color = new Color(text.color.r, text.color.g, text.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		yield return null;
	}

	// returns the word to where it originally came from
	IEnumerator returnToStartPos()
	{
		for (int i = 0; i < 50; i++)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, StartPos, 0.2f);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = StartPos;
		yield return null;
	}
	#endregion
	#region Private Methods
	#endregion
}
