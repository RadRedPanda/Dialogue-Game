using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public PlayerController playerC;
    public CameraController cameraC;
    public CanvasController canvasC;

	private FiniteStateMachine<InputManager> inputStateMachine;
	// Use this for initialization
	void Start ()
	{
		inputStateMachine = new FiniteStateMachine<InputManager>(this);
		inputStateMachine.TransitionTo<Normal>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		inputStateMachine.Update();
        cameraC.turnBillboards();
    }

	private void PlayerMovement()
	{
		// player movement
		float dx = Input.GetAxis("Horizontal");
		float dz = Input.GetAxis("Vertical");
		playerC.Move(dx, dz);
	}

	private class BaseState : FiniteStateMachine<InputManager>.State
	{
		public PlayerController playerC;
		public CameraController cameraC;
		public CanvasController canvasC;

		public override void Initialize()
		{
			playerC = Context.playerC;
			cameraC = Context.cameraC;
			canvasC = Context.canvasC;
		}
	}

	private class Normal : BaseState
	{
		public override void OnEnter()
		{
			Cursor.visible = false;
		}

		public override void Update()
		{
			Context.PlayerMovement();
			
			// normal camera movement
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			cameraC.followPlayer(scroll, mouseX, mouseY);

			if (Input.GetButtonDown("Tab"))
				TransitionTo<InInventory>();

			if (Input.GetButtonDown("Escape"))
				TransitionTo<InMenu>();

			Collider closest = playerC.CheckIfNear();
			if (closest != null)
			{
				// show popup for interactable (like a speech bubble or exclamation mark)
				/////////////
				if (Input.GetButtonDown("Interact"))
					TransitionTo<InDialogue>();
			}
			else
			{
				// remove pop up
				////////////////////
			}

		}

		public override void OnExit()
		{
			
		}
	}

	private class InDialogue : BaseState
	{
		private Collider closest;
		
		public override void OnEnter()
		{
			Cursor.visible = true;
			closest = playerC.CheckIfNear();
			///////////////////////////// should remove the overhead popup on the character when in dialogue
			
			// do stuff when interacting with thing like show dialogue box and move camera
			cameraC.setTarget(closest.gameObject);
			DialogueManager dm = closest.GetComponentInParent<DialogueManager>();
			if (dm != null)
				canvasC.startDialogue(dm);
		}
		
		public override void Update()
		{
			cameraC.inDialogue();
			if (Input.GetButtonDown("Interact"))
				canvasC.nextDialogue();
		}

		public override void OnExit()
		{

		}
	}

	private class InMenu : BaseState
	{
		public override void OnEnter()
		{
			Cursor.visible = true;
			Time.timeScale = 0;
			canvasC.openMenu();	//////////////////////// change to be coroutine
		}

		public override void Update()
		{
			
		}

		public override void OnExit()
		{
			Time.timeScale = 1;
			canvasC.closeMenu();	///////////////////////////// change to be coroutine
		}
	}

	private class InInventory : BaseState
	{
		public override void OnEnter()
		{
			Cursor.visible = true;
			canvasC.openInventory(); //////////////////////////// fix this to be a coroutine
		}

		public override void Update()
		{
			Context.PlayerMovement();
			cameraC.followPlayer(0, 0, 0);

			if (Input.GetButtonUp("Tab"))
				TransitionTo<Normal>();
		}

		public override void OnExit()
		{
			canvasC.closeInventory(); ///////////////////////// fix this to be a coroutine
		}
	}
}
