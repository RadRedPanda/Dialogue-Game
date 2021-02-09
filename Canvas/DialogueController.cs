using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour {

    public Canvas canvas;	// the canvas this is rendered on
    public GameObject keywordContainer; // the GameObject which holds all the keyword objects
    public GameObject keywordBlueprint;	// the blueprint to make new keywords appear

    public float textDelay = 0.05f;	// the delay in between each character in time
    public RectTransform rt;
    public Text dialogueName;	// the character's name
    public Text dialogueText;	// the actual text
    public AudioSource dialogueSound;	// a blip sound made while the character is speaking

    private Coroutine open;
    private Coroutine talking;
    private bool wait;
	private bool speaking;

    // Use this for initialization
    void Start () {
        gameObject.SetActive(true);
        transform.localPosition = new Vector3(0, -1000, 0);
        rt = GetComponent<RectTransform>();
    }

    public void startDialogue(DialogueManager dm) {
        StopAllCoroutines();
        open = StartCoroutine(openDialogue());
        talking = StartCoroutine(readDialogue(dm.startConversation()));
        dialogueText.text = "";
        foreach (Transform t in keywordContainer.GetComponentsInChildren<Transform>())
            if(t != keywordContainer.transform)
				Destroy(t.gameObject);
    }

	public void nextDialogue(DialogueManager dm){
		if(speaking)
			wait = false;
		else
			talking = StartCoroutine(readDialogue(dm.nextDialogue()));
	}

    public void endDialogue() {
        StopAllCoroutines();
        open = StartCoroutine(closeDialogue());
    }

    IEnumerator openDialogue() {
        // loop to pull the dialogue box up
        float dY = 1;
        while (Mathf.Abs(dY) > 0.01f) {
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.SmoothDamp(transform.localPosition.y, 0, ref dY, 0.2f), transform.localPosition.z);
            yield return new WaitForFixedUpdate();
        }
        transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
    }

    IEnumerator closeDialogue() {
        // loop to pull the dialogue box down
        float dY = 1;
        while (Mathf.Abs(dY) > 0.01f) {
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.SmoothDamp(transform.localPosition.y, -1000, ref dY, 1), transform.localPosition.z);
            yield return new WaitForFixedUpdate();
        }
        transform.localPosition = new Vector3(transform.localPosition.x, -1000, transform.localPosition.z);
    }

    IEnumerator readDialogue(string inputText) {
        yield return new WaitForFixedUpdate();
        wait = true;
		speaking = true;
        int lastIndex = 0;
        // display one character at a time
        for (int currentChar = 1; currentChar <= inputText.Length; currentChar++) {
            dialogueText.text = inputText.Substring(0, currentChar);

			Keyword[] keywords = (Keyword[])Enum.GetValues(typeof(Keyword));
            // check if one of the keywords showed up
            foreach(Keyword keyword in keywords) {
                int index = dialogueText.text.ToLower().Substring(lastIndex).IndexOf(keyword.ToString());
                if (index > -1) {
                    string text = "";
					int canvasIndex = index + lastIndex;
					bool isCapital = char.IsUpper(dialogueText.text[canvasIndex]);
                    for (int i = 0; i < inputText.Length; i++) {
                        switch (inputText[i]) {
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
                    ks.setUp(keyword.ToString(), worldPos, isCapital);
                    lastIndex += index + 1;
                }
                
            }
            if (wait) {
                dialogueSound.PlayOneShot(dialogueSound.clip);
                yield return new WaitForSeconds(textDelay);
            }
        }
		speaking = false;
    }
}
