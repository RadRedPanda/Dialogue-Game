using UnityEngine;

public class InputManager : MonoBehaviour
{

	public PlayerController playerC;
	public CameraController cameraC;
	public CanvasController canvasC;

	private FiniteStateMachine<InputManager> inputStateMachine;
	// Use this for initialization
	void Start()
	{
		inputStateMachine = new FiniteStateMachine<InputManager>(this);
		inputStateMachine.TransitionTo<Normal>();
	}

	// Update is called once per frame
	void Update()
	{
		inputStateMachine.Update();
		cameraC.turnBillboards();
	}

	void FixedUpdate()
	{
		inputStateMachine.FixedUpdate();
	}

	void LateUpdate()
	{
		inputStateMachine.LateUpdate();
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
			if (Input.GetButtonDown("Tab"))
				TransitionTo<InJournal>();
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

		public override void FixedUpdate()
		{
			Context.PlayerMovement();

			// normal camera movement
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			cameraC.followPlayer(scroll, mouseX, mouseY);
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
			canvasC.OpenInventory();
			closest = playerC.CheckIfNear();
			///////////////////////////// should remove the overhead popup on the character when in dialogue

			// do stuff when interacting with thing like show dialogue box and point camera
			cameraC.setTarget(closest.gameObject);
			DialogueManager dm = closest.GetComponentInParent<DialogueManager>();
			if (dm != null)
				canvasC.StartDialogue(dm);
		}

		public override void Update()
		{
			if (Input.GetButtonDown("Interact"))
				canvasC.NextDialogue();
			if (!playerC.StillInRange(closest))
				TransitionTo<Normal>();
		}

		public override void FixedUpdate()
		{
			Context.PlayerMovement();
			cameraC.inDialogue();
		}

		public override void OnExit()
		{
			canvasC.CloseInventory();
			canvasC.EndDialogue();
		}
	}

	private class InMenu : BaseState
	{
		public override void OnEnter()
		{
			Cursor.visible = true;
			Time.timeScale = 0;
			canvasC.OpenMenu(); //////////////////////// change to be coroutine
		}

		public override void Update()
		{

		}

		public override void OnExit()
		{
			Time.timeScale = 1;
			canvasC.CloseMenu();    ///////////////////////////// change to be coroutine
		}
	}

	private class InJournal : BaseState
	{
		public override void OnEnter()
		{
			Cursor.visible = true;
			canvasC.OpenJournal();

		}

		public override void Update()
		{
			if (Input.GetButtonDown("Tab"))
				TransitionTo<Normal>();
		}

		public override void OnExit()
		{
			Cursor.visible = false;
			canvasC.CloseJournal();
		}
	}
}
