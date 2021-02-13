using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public KeywordScript Keyword;
	public bool Occupied;

	private IEnumerator openCloseCoroutine;
	private Vector2 velocity;
	private RectTransform rectTransform;

	private bool open;
	private float closeYPos;
	private float panelSpeed;

	public void SetUp(float yPos, float pSpeed){
		closeYPos = yPos;
		panelSpeed = pSpeed;

		Occupied = false;
		open = false;
		velocity = Vector2.zero;
		rectTransform = GetComponent<RectTransform>();
		transform.localPosition = new Vector2(transform.localPosition.x, closeYPos);
	}

	public Vector3 GetMiddle(float keywordWidth)
	{
		return new Vector3(0f - (keywordWidth / 2f), 0f - (rectTransform.rect.height / 4f), 0f);
	}

	private void openPanel()
	{
		if (!open)
		{
			open = true;
			if (openCloseCoroutine != null)
				StopCoroutine(openCloseCoroutine);
			openCloseCoroutine = openPanelCoroutine();
			StartCoroutine(openCloseCoroutine);
		}
	}

	private void closePanel()
	{
		if (open)
		{
			open = false;
			if (openCloseCoroutine != null)
				StopCoroutine(openCloseCoroutine);
			openCloseCoroutine = closePanelCoroutine();
			StartCoroutine(openCloseCoroutine);
		}
	}

	IEnumerator openPanelCoroutine()
	{
		while (transform.localPosition.y > 1)
		{
			transform.localPosition = Vector2.SmoothDamp(transform.localPosition, new Vector2(transform.localPosition.x, 0), ref velocity, panelSpeed);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = new Vector2(transform.localPosition.x, 0);
		yield return new WaitForFixedUpdate();
	}

	IEnumerator closePanelCoroutine()
	{
		while (Mathf.Abs(transform.localPosition.y - closeYPos) > 1)
		{
			transform.localPosition = Vector2.SmoothDamp(transform.localPosition, new Vector2(transform.localPosition.x, closeYPos), ref velocity, panelSpeed);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = new Vector2(transform.localPosition.x, closeYPos);
		yield return new WaitForFixedUpdate();
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		openPanel();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		closePanel();
	}
}
