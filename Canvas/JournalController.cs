using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalController : MonoBehaviour
{
	public List<PageController> Pages;
	public int CurrentPage;
	public bool JournalOpen;

	private Canvas canvas;   // the canvas this is rendered on
	private FiniteStateMachine<JournalController> journalStateMachine;
	private CanvasController canvasC;

	public void Setup(CanvasController cc)
	{
		gameObject.SetActive(true);
		canvas = cc.GetComponent<Canvas>();
		canvasC = cc;
		CloseJournal();
		journalStateMachine = new FiniteStateMachine<JournalController>(this);
		journalStateMachine.TransitionTo<Normal>();
		CurrentPage = 0;
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void OpenJournal()
	{
		JournalOpen = true;
		StopAllCoroutines();
		StartCoroutine(openJournal());
	}

	public void CloseJournal()
	{
		JournalOpen = false;
		StopAllCoroutines();
		StartCoroutine(closeJournal());
	}

	public void GotoPage(int pageNumber)
	{

	}
	
	IEnumerator openJournal()
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

	IEnumerator closeJournal()
	{
		// loop to pull the dialogue box down
		float dY = 1;
		while (Mathf.Abs(dY) > 0.01f)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.SmoothDamp(transform.localPosition.y, -canvas.GetComponent<RectTransform>().rect.height, ref dY, 0.2f), transform.localPosition.z);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = new Vector3(transform.localPosition.x, -canvas.GetComponent<RectTransform>().rect.height, transform.localPosition.z);
	}


	public void FindConnection()
	{
		journalStateMachine.TransitionTo<Connection>();
	}

	public void FindDiscrepancy()
	{
		journalStateMachine.TransitionTo<Discrepancy>();
	}

	private class Normal : FiniteStateMachine<JournalController>.State
	{
		public override void OnEnter()
		{

		}

		public override void Update()
		{

		}

		public override void OnExit()
		{

		}
	}

	private class Connection : FiniteStateMachine<JournalController>.State
	{
		private GameObject clicked;
		public override void OnEnter()
		{
			clicked = null;
		}

		public override void Update()
		{
			if (Input.GetMouseButtonDown(0)){
				
			}
			if (Input.GetMouseButtonUp(0)){
				
			}
		}

		public override void OnExit()
		{

		}
	}

	private class Discrepancy : FiniteStateMachine<JournalController>.State
	{
		public override void OnEnter()
		{

		}

		public override void Update()
		{

		}

		public override void OnExit()
		{

		}
	}
}
