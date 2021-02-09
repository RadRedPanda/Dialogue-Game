using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeywordScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public InventoryController ic;
    public string word;
    public float alphaSpeed = 0.1f;
    
    private Text text;
    private bool picked;
    private Vector3 prevPointerPos;
    private Vector3 startPos;
    
	// Use this for initialization
	void Awake () {
        picked = false;
        text = GetComponent<Text>();
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        StartCoroutine(phaseIn());
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (picked) {
            transform.position = transform.position + Input.mousePosition - prevPointerPos;
            prevPointerPos = Input.mousePosition;
        }
    }
    
    IEnumerator phaseIn() {
        for(float a = 0; a < 1; a += alphaSpeed) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, a);
            yield return null;
        }
        yield return null;
    }

    public void setUp(string w, Vector3 pos, bool isCapital) {
        text = GetComponent<Text>();
		if (isCapital)
			text.text = char.ToUpper(w[0]) + w.Substring(1);
		else
			text.text = w;
		word = w;
        transform.position = pos;
        startPos = transform.localPosition;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        prevPointerPos = eventData.position;
        picked = true;
        StopAllCoroutines();

    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        picked = false;

        // check if the word is dropped in the inventory or whatever

        StartCoroutine(returnToStartPos());
    }

    IEnumerator returnToStartPos() {
        for(int i=0; i<50; i++) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, 0.2f);
            yield return null;
        }
        transform.localPosition = startPos;
        yield return null;
    }
}
