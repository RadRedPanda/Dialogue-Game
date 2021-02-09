using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeywordScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

	public InventoryController ic;
	public string word;
	public float alphaSpeed = 0.01f;
	public DialogueController dialogueC;
	[HideInInspector]
	public bool picked;     // true if player is holding word with mouse
	[HideInInspector]
	public bool homeless;   // true if it has nowhere to return to, prep for deletion

	private Text text;
	private Vector3 prevPointerPos;
	private Vector3 startPos;

	// Use this for initialization
	void Awake()
	{
		picked = false;
		homeless = false;
		text = GetComponent<Text>();
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

	public void SetUp(string w, Vector3 pos, bool isCapital, DialogueController dc)
	{
		text = GetComponent<Text>();
		if (isCapital)
			text.text = char.ToUpper(w[0]) + w.Substring(1);
		else
			text.text = w;
		word = w;
		transform.position = pos;
		startPos = transform.localPosition;
		dialogueC = dc;
	}

	// call this if keyword needs to be deleted
	public void MakeHomeless()
	{
		homeless = true;
	}

	#region Handler Methods
	// player dragging mouse on word
	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		prevPointerPos = eventData.position;
		picked = true;
		StopAllCoroutines();
	}

	// released the mouse hold on the word
	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		picked = false;

		////////////////////// check if the word is dropped in the inventory or whatever, else return to home or delete
		////////////////////// if in inventory, set startPos and set homeless = false
		
		// checks if we're hovering the mouse over the character the player is talking to
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			if (dialogueC.IsSpeakerCollider(hit.collider) && !dialogueC.IsHoveringDialogueBox())
			{
				dialogueC.ChooseDialogue(word);
			}
		}

		if (homeless)
		{
			StopAllCoroutines();
			Destroy(this);
		}
		else
			StartCoroutine(returnToStartPos());
	}
	#endregion
	#region Coroutine Methods
	// phases in the alpha from transparent to opaque
	IEnumerator phaseIn()
	{
		for (float a = 0; a < 1; a += alphaSpeed)
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
			transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, 0.2f);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = startPos;
		yield return null;
	}
	#endregion
	#region Private Methods
	#endregion
}
