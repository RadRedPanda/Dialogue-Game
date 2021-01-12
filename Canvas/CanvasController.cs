using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CanvasController : MonoBehaviour {

    public GameObject pauseMenu;
    public InventoryController inventory;
    public DialogueController dialogue;
    public GameObject notebook;

    // Use this for initialization
    void Start () {
        Canvas c = GetComponent<Canvas>();
    }

    void Update() {
        //////////////////////////////// look at this, fix tomorrow
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

    public void openMenu() {

    }

    public void closeMenu() {

    }

    public void openInventory() {
        inventory.openInventory();
    }

    public void closeInventory() {
        inventory.closeInventory();
    }
    // dialogue stuff
    public void startDialogue(DialogueManager dm) {
        dialogue.startDialogue(dm);
    }

    public void endDialogue() {
        dialogue.endDialogue();
    }
}
