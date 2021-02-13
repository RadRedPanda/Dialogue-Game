using Assets.Scripts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
	public GameObject KeywordContainer; // the GameObject which holds all the keyword objects
	public GameObject KeywordBlueprint; // the blueprint to make new keywords appear

	public float textDelay = 0.05f; // the delay in between each character in time
	public GameObject DialogueBox;
	public Text DialogueName;   // the character's name
	public Text DialogueText;   // the actual text

	private Canvas canvas;   // the canvas this is rendered on
	private AudioSource dialogueSound;   // a blip sound made while the character is speaking
	private RectTransform rt;
	private CanvasController canvasC;
	private DialogueManager dialogueManager;
	private Collider speakerCollider;   // the collider of who we're speaking to right now
	private Coroutine open;
	private Coroutine talking;
	private bool wait;
	private bool speaking;

	public void Setup(CanvasController cc)
	{

		gameObject.SetActive(true);
		transform.localPosition = new Vector3(0, -1000, 0);
		rt = GetComponent<RectTransform>();
		dialogueSound = GetComponent<AudioSource>();
		canvas = cc.GetComponent<Canvas>();
		canvasC = cc;
	}

	public void StartDialogue(DialogueManager dm)
	{
		dialogueManager = dm;
		speakerCollider = dialogueManager.gameObject.GetComponent<Collider>();
		StopAllCoroutines();
		open = StartCoroutine(openDialogue());
		clearKeywords();
		DialogueText.text = "";
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
				DialogueText.text = "";
				talking = StartCoroutine(readDialogue(text));
			}
		}
	}

	public bool ChooseDialogue(string keyword)
	{
		string text = dialogueManager.ChooseOption(keyword);
		if (!string.IsNullOrEmpty(text))
		{
			clearKeywords();
			DialogueText.text = "";
			talking = StartCoroutine(readDialogue(text));
			return true;
		}
		return false;
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
		return canvasC.IsHoveringObject(DialogueBox);
	}

	private void clearKeywords()
	{
		foreach (Transform t in KeywordContainer.GetComponentsInChildren<Transform>())
			if (t != KeywordContainer.transform)
				Destroy(t.gameObject);
		if (canvasC.KeywordContainer.childCount > 0)
		{
			KeywordScript targetScript = canvasC.KeywordContainer.GetChild(0).GetComponent<KeywordScript>();
			if (targetScript.OldParent == KeywordContainer)
				targetScript.MakeHomeless();
		}
	}

	IEnumerator openDialogue()
	{
		// loop to pull the dialogue box up
		float dY = 1;
		while (Mathf.Abs(dY) > 0.01f)
		{
			transform.position = new Vector3(transform.position.x, Mathf.SmoothDamp(transform.position.y, 0, ref dY, 0.2f), transform.position.z);
			yield return new WaitForFixedUpdate();
		}
		transform.position = new Vector3(transform.position.x, 0, transform.position.z);
	}

	IEnumerator closeDialogue()
	{
		// loop to pull the dialogue box down
		float dY = 1;
		while (Mathf.Abs(dY) > 0.01f)
		{
			transform.position = new Vector3(transform.position.x, Mathf.SmoothDamp(transform.position.y, -1000, ref dY, 1), transform.position.z);
			yield return new WaitForFixedUpdate();
		}
		transform.position = new Vector3(transform.position.x, -1000, transform.position.z);
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
			DialogueText.text = inputText.Substring(0, currentChar);

			Keyword[] keywords = (Keyword[])Enum.GetValues(typeof(Keyword));
			// check if one of the keywords showed up
			foreach (Keyword keyword in keywords)
			{
				string keywordString = keyword.ToString();
				int index = DialogueText.text.ToLower().Substring(lastIndex).IndexOf(keywordString);
				if (index > -1)
				{
					int canvasIndex = index + lastIndex;    // the absolute index of the found string

					// check if the word is isolated and not inside of another word
					//	left condition
					if (canvasIndex > 0)
						if (char.IsLetter(DialogueText.text[canvasIndex - 1]))
							continue;
					//	right condition
					if (canvasIndex + keywordString.Length < DialogueText.text.Length)
						if (char.IsLetter(DialogueText.text[canvasIndex + keywordString.Length]))
							continue;

					string text = "";
					bool isCapital = char.IsUpper(DialogueText.text[canvasIndex]);
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
					Vector2 extents = DialogueText.gameObject.GetComponent<RectTransform>().rect.size;
					textGen.Populate(text, DialogueText.GetGenerationSettings(extents));

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
					Vector3 worldPos = DialogueText.transform.TransformPoint(avgPos);

					CreateKeyword(keywordString, worldPos, isCapital);
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

	public KeywordScript CreateKeyword(string w, Vector3 pos, bool isCapital)
	{
		GameObject key = Instantiate(KeywordBlueprint, KeywordContainer.transform);
		KeywordScript ks = key.GetComponent<KeywordScript>();
		ks.SetUp(w, pos, isCapital, canvasC);
		return ks;
	}
}
