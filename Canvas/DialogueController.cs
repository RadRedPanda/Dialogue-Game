using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{

	public Canvas canvas;   // the canvas this is rendered on
	public GameObject keywordContainer; // the GameObject which holds all the keyword objects
	public GameObject keywordBlueprint; // the blueprint to make new keywords appear

	public float textDelay = 0.05f; // the delay in between each character in time
	public RectTransform rt;
	public GameObject DialogueBox;
	public Text dialogueName;   // the character's name
	public Text dialogueText;   // the actual text
	public AudioSource dialogueSound;   // a blip sound made while the character is speaking

	private DialogueManager dialogueManager;
	private Collider speakerCollider;   // the collider of who we're speaking to right now
	private Coroutine open;
	private Coroutine talking;
	private bool wait;
	private bool speaking;

	// Use this for initialization
	void Start()
	{
		gameObject.SetActive(true);
		transform.localPosition = new Vector3(0, -1000, 0);
		rt = GetComponent<RectTransform>();
	}

	public void StartDialogue(DialogueManager dm)
	{
		dialogueManager = dm;
		speakerCollider = dialogueManager.gameObject.GetComponent<Collider>();
		StopAllCoroutines();
		open = StartCoroutine(openDialogue());
		clearKeywords();
		dialogueText.text = "";
		talking = StartCoroutine(readDialogue(dialogueManager.StartConversation()));
	}

	public void NextDialogue()
	{
		if (speaking)
			wait = false;
		else
		{
			string text = dialogueManager.NextDialogue();
			if (!string.IsNullOrEmpty(text))
			{
				clearKeywords();
				dialogueText.text = "";
				talking = StartCoroutine(readDialogue(text));
			}
		}
	}

	public void ChooseDialogue(string keyword)
	{
		string text = dialogueManager.ChooseOption(keyword);
		if (!string.IsNullOrEmpty(text))
		{
			clearKeywords();
			dialogueText.text = "";
			talking = StartCoroutine(readDialogue(text));
		}
	}

	public void EndDialogue()
	{
		StopAllCoroutines();
		open = StartCoroutine(closeDialogue());
	}

	public bool IsSpeakerCollider(Collider c)
	{
		return c == speakerCollider;
	}

	public bool IsHoveringDialogueBox()
	{
		return isHoveringObject(DialogueBox);
	}

	// returns true if the mouse is currently over the canvas object, doesn't care if there's anything in between
	private bool isHoveringObject(GameObject g)
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

	private void clearKeywords()
	{
		foreach (Transform t in keywordContainer.GetComponentsInChildren<Transform>())
			if (t != keywordContainer.transform)
			{
				KeywordScript targetScript = t.GetComponent<KeywordScript>();
				if (targetScript.picked)
					targetScript.MakeHomeless();
				else
					Destroy(t.gameObject);
			}
	}

	IEnumerator openDialogue()
	{
		// loop to pull the dialogue box up
		float dY = 1;
		while (Mathf.Abs(dY) > 0.01f)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.SmoothDamp(transform.localPosition.y, 0, ref dY, 0.2f), transform.localPosition.z);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
	}

	IEnumerator closeDialogue()
	{
		// loop to pull the dialogue box down
		float dY = 1;
		while (Mathf.Abs(dY) > 0.01f)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.SmoothDamp(transform.localPosition.y, -1000, ref dY, 1), transform.localPosition.z);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = new Vector3(transform.localPosition.x, -1000, transform.localPosition.z);
	}

	IEnumerator readDialogue(string inputText)
	{
		yield return new WaitForFixedUpdate();
		wait = true;
		speaking = true;
		int lastIndex = 0;
		// display one character at a time
		for (int currentChar = 1; currentChar <= inputText.Length; currentChar++)
		{
			dialogueText.text = inputText.Substring(0, currentChar);

			Keyword[] keywords = (Keyword[])Enum.GetValues(typeof(Keyword));
			// check if one of the keywords showed up
			foreach (Keyword keyword in keywords)
			{
				string keywordString = keyword.ToString();
				int index = dialogueText.text.ToLower().Substring(lastIndex).IndexOf(keywordString);
				if (index > -1)
				{
					int canvasIndex = index + lastIndex;    // the absolute index of the found string

					// check if the word is isolated and not inside of another word
					//	left condition
					if (canvasIndex > 0)
						if (char.IsLetter(dialogueText.text[canvasIndex - 1]))
							continue;
					//	right condition
					if (canvasIndex + keywordString.Length < dialogueText.text.Length)
						if (char.IsLetter(dialogueText.text[canvasIndex + keywordString.Length]))
							continue;

					string text = "";
					bool isCapital = char.IsUpper(dialogueText.text[canvasIndex]);
					for (int i = 0; i < inputText.Length; i++)
					{
						switch (inputText[i])
						{
							case ' ':
								break;
							case '\n':
								text += '\n';
								break;
							default:
								text += 'a';
								break;
						}
					}

					TextGenerator textGen = new TextGenerator(text.Length);
					Vector2 extents = dialogueText.gameObject.GetComponent<RectTransform>().rect.size;
					textGen.Populate(text, dialogueText.GetGenerationSettings(extents));

					int newLine = text.Substring(0, canvasIndex).Split('\n').Length - 1;
					int whiteSpace = text.Substring(0, canvasIndex).Split(' ').Length - 1;
					int indexOfTextQuad = ((canvasIndex) * 4) + (newLine * 4);
					Vector3 avgPos = (textGen.verts[indexOfTextQuad].position +
						textGen.verts[indexOfTextQuad + 1].position +
						textGen.verts[indexOfTextQuad + 2].position +
						textGen.verts[indexOfTextQuad + 3].position) / 4f;

					float leftX = textGen.verts[indexOfTextQuad].position.x;
					avgPos = new Vector3(leftX, avgPos.y, avgPos.z);
					avgPos /= canvas.scaleFactor;
					Vector3 worldPos = dialogueText.transform.TransformPoint(avgPos);

					GameObject key = Instantiate(keywordBlueprint, keywordContainer.transform);
					KeywordScript ks = key.GetComponent<KeywordScript>();
					ks.SetUp(keywordString, worldPos, isCapital, this);
					lastIndex += index + 1;
				}

			}
			if (wait)
			{
				dialogueSound.PlayOneShot(dialogueSound.clip);
				yield return new WaitForSeconds(textDelay);
			}
		}
		speaking = false;
	}
}
