using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public PlayerController playerC;
    public CameraController cameraC;
    public CanvasController canvasC;

    private bool menuOpen;
    private bool inDialogue;
	// Use this for initialization
	void Start () {
        menuOpen = false;
        inDialogue = false;
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (menuOpen) { // when the menu is open

        }
        else if (inDialogue) { // when the player is talking to someone
            cameraC.inDialogue();

            if (Input.GetButtonDown("Escape")) {
                // do stuff when interacting with thing like show dialogue box and move camera
                inDialogue = false;
                canvasC.endDialogue();
                Cursor.visible = false;
                //////////////////////////
            }
        }
        else { // not in dialogue or menu

            // player movement
            float dx = Input.GetAxis("Horizontal");
            float dz = Input.GetAxis("Vertical");
            playerC.Move(dx, dz);

            if (!Input.GetButton("Tab")) {
                // normal camera movement
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                cameraC.followPlayer(scroll, mouseX, mouseY);
            }
            else {
                cameraC.followPlayer(0, 0, 0);
            }
            if (Input.GetButtonDown("Tab")) {
                canvasC.openInventory();
                Cursor.visible = true;
            }
            else if (Input.GetButtonUp("Tab")) {
                canvasC.closeInventory();
                Cursor.visible = false;
            }

            if (Input.GetButtonDown("Escape")) {
                // pause game
                menuOpen = !menuOpen;
                Time.timeScale = menuOpen ? 0 : 1;
                canvasC.openMenu();
                menuOpen = true;
                Cursor.visible = true;
                ///////////////////////////////
            }

            Collider closest = playerC.CheckIfNear();
            if (closest != null) {
                // show popup for interactable (like a speech bubble or exclamation mark)
                /////////////
                if (Input.GetButtonDown("Interact")) {
                    // do stuff when interacting with thing like show dialogue box and move camera
                    inDialogue = true;
                    cameraC.setTarget(closest.gameObject);
                    DialogueManager dm = closest.GetComponentInParent<DialogueManager>();
                    if (dm != null)
                        canvasC.startDialogue(dm);
                    Cursor.visible = true;
                }
            }
            else {
                // remove pop up
                ////////////////////
            }
        }

        // use for notebook or inventory, not sure. need another button for the other one, could be something like shift or E
        



        cameraC.turnBillboards();
    }
}
