using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestText : MonoBehaviour {
    public Text textComp;
    public int charIndex;
    public Canvas canvas;

    public GameObject keywordBlueprint;
    public GameObject keywordContainer;

    void PrintPos() {
        string text = "";
        for (int i = 0; i < textComp.text.Length; i++) {
            switch (textComp.text[i]) {
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

        if (charIndex >= text.Length)
            return;

        TextGenerator textGen = new TextGenerator(text.Length);
        Vector2 extents = textComp.gameObject.GetComponent<RectTransform>().rect.size;
        textGen.Populate(text, textComp.GetGenerationSettings(extents));

        int newLine = text.Substring(0, charIndex).Split('\n').Length - 1;
        int whiteSpace = text.Substring(0, charIndex).Split(' ').Length - 1;
        int indexOfTextQuad = (charIndex * 4) + (newLine * 4) - 4;
        if (indexOfTextQuad < textGen.vertexCount) {
            print(textGen.verts[indexOfTextQuad].position);
            print(textGen.verts[indexOfTextQuad + 1].position);
            print(textGen.verts[indexOfTextQuad + 2].position);
            print(textGen.verts[indexOfTextQuad + 3].position);
            Vector3 avgPos = (textGen.verts[indexOfTextQuad].position +
                textGen.verts[indexOfTextQuad + 1].position +
                textGen.verts[indexOfTextQuad + 2].position +
                textGen.verts[indexOfTextQuad + 3].position) / 4f;

            float leftX = textGen.verts[indexOfTextQuad].position.x;
            avgPos = new Vector3(leftX, avgPos.y, avgPos.z);
            print(avgPos);
            avgPos /= canvas.scaleFactor;
            PrintWorldPos(avgPos);
        }
        else {
            Debug.LogError("Out of text bound");
        }
    }

    void PrintWorldPos(Vector3 testPoint) {
        Vector3 worldPos = textComp.transform.TransformPoint(testPoint);
        print(worldPos);
        charIndex += 6;
        new GameObject("point").transform.position = worldPos;
        Debug.DrawRay(worldPos, Vector3.up, Color.red, 50f);
    }

    void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 80), "Test")) {
            PrintPos();
        }
    }
}