using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour {

    public Canvas canvas;
    public GameObject keywordContainer;
    public GameObject keywordBlueprint;

    public float textDelay = 0.05f;
    public RectTransform rt;
    public Text dialogueName;
    public Text dialogueText;
    public AudioSource dialogueSound;

    private Coroutine open;
    private Coroutine talking;
    private bool wait;
    private List<GameObject> keywords;

    // Use this for initialization
    void Start () {
        gameObject.SetActive(true);
        transform.localPosition = new Vector3(0, -1000, 0);
        rt = GetComponent<RectTransform>();
        keywords = new List<GameObject>();
    }

    void Update() {
        if (Input.GetButtonDown("Interact")) {
            wait = false;
        }
    }

    public void startDialogue(DialogueManager dm) {
        StopAllCoroutines();
        open = StartCoroutine(openDialogue());
        talking = StartCoroutine(readDialogue(dm));
        dialogueText.text = "";
        foreach (GameObject g in keywords)
            Destroy(g);
        keywords = new List<GameObject>();
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

    IEnumerator readDialogue(DialogueManager dm) {
        yield return new WaitForSeconds(0.5f);
        string inputText = dm.startConversation();
        wait = true;
        int lastIndex = 0;
        // display one character at a time
        for (int currentChar = 1; currentChar <= inputText.Length; currentChar++) {
            dialogueText.text = inputText.Substring(0, currentChar);

            // check if one of the keywords showed up
            foreach(string keyword in dm.keywords) {
                int index = dialogueText.text.Substring(lastIndex).IndexOf(keyword);
                if (index > -1) {
                    
                    string text = "";
                    for (int i = 0; i < inputText.Length; i++) {
                        switch (inputText[i]) {
                            case ' ':
                                text += ' ';
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

                    int newLine = text.Substring(0, index + lastIndex).Split('\n').Length - 1;
                    int whiteSpace = text.Substring(0, index + lastIndex).Split(' ').Length - 1;
                    int indexOfTextQuad = ((index + lastIndex) * 4) + (newLine * 4);
                    Vector3 avgPos = (textGen.verts[indexOfTextQuad].position +
                        textGen.verts[indexOfTextQuad + 1].position +
                        textGen.verts[indexOfTextQuad + 2].position +
                        textGen.verts[indexOfTextQuad + 3].position) / 4f;

                    float leftX = textGen.verts[indexOfTextQuad].position.x;
                    avgPos = new Vector3(leftX, avgPos.y, avgPos.z);
                    avgPos /= canvas.scaleFactor;
                    Vector3 worldPos = dialogueText.transform.TransformPoint(avgPos);

                    GameObject key = Instantiate(keywordBlueprint, keywordContainer.transform);
                    keywords.Add(key);
                    KeywordScript ks = key.GetComponent<KeywordScript>();
                    ks.setUp(keyword, worldPos);
                    lastIndex += index + 1;
                }
                
            }
            if (wait) {
                dialogueSound.PlayOneShot(dialogueSound.clip);
                yield return new WaitForSeconds(textDelay);
            }
        }
    }
}
