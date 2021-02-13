﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
	public List<PanelScript> InventoryPanels;
	public float PanelSpeed;
	public float CloseYPos;

	private CanvasController canvasC;
	private RectTransform rectT;
	private bool inInventory;

	public void Setup(CanvasController cc)
	{
		canvasC = cc;
		rectT = GetComponent<RectTransform>();
		CloseInventory();

		foreach (PanelScript panel in InventoryPanels)
		{
			panel.SetUp(CloseYPos, PanelSpeed);
		}
	}

	public void OpenInventory()
	{
		StopAllCoroutines();
		StartCoroutine(openInventoryCoroutine());
	}

	public void CloseInventory()
	{
		StopAllCoroutines();
		StartCoroutine(closeInventoryCoroutine());
	}

	public bool AddKeywordToPanel(KeywordScript k)
	{
		PanelScript panel = InventoryPanels.Find(x => canvasC.IsHoveringObject(x.gameObject));
		if (panel == null)
			return false;

		if (k.CurrentPanel == panel)    // drag word onto same slot
		{
			k.transform.SetParent(panel.transform);
			k.SendHome();
		}
		else if (k.CurrentPanel == null)
		{
			if (panel.Occupied)
				bootHomelessKeyword(panel.Keyword);
			DialogueController dialogueC = canvasC.DialogueC;
			Transform dialogueT = dialogueC.DialogueBox.transform;
			dialogueC.CreateKeyword(k.word, dialogueT.TransformPoint(k.StartPos), char.IsUpper(k.word[0]));
		}
		else if (k.CurrentPanel != null)
			if (panel.Occupied)
				sendKeywordToPanel(panel.Keyword, k.CurrentPanel);
		sendKeywordToPanel(k, panel);
		return true;
	}

	private void sendKeywordToPanel(KeywordScript k, PanelScript p)
	{
		p.Occupied = true;
		p.Keyword = k;
		k.SetNewHome(p);
		k.SendHome();
	}

	private void bootHomelessKeyword(KeywordScript k)
	{
		k.DeleteThis();
	}

	IEnumerator openInventoryCoroutine()
	{
		// loop to pull the inventory panels down
		float dY = 0;
		while (Mathf.Abs(rectT.anchoredPosition.y) > 1f)
		{
			rectT.anchoredPosition = new Vector2(rectT.anchoredPosition.x, Mathf.SmoothDamp(rectT.anchoredPosition.y, 0, ref dY, 0.2f));
			yield return new WaitForFixedUpdate();
		}
		rectT.anchoredPosition = new Vector3(rectT.anchoredPosition.x, 0);
	}

	IEnumerator closeInventoryCoroutine()
	{
		// loop to pull the inventory panels up
		float dY = 0;
		while (Mathf.Abs(rectT.anchoredPosition.y - 1000) > 1f)
		{
			rectT.anchoredPosition = new Vector3(rectT.anchoredPosition.x, Mathf.SmoothDamp(rectT.anchoredPosition.y, 1000, ref dY, 1));
			yield return new WaitForFixedUpdate();
		}
		rectT.anchoredPosition = new Vector2(rectT.anchoredPosition.x, 1000);
	}
}
