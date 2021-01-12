using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour {

    public Vector2 baseSize = new Vector2(600, 510);
    public GameObject cloud;
    public float resizeSpeed = 2;

    private RectTransform rt;

    // Use this for initialization
    void Start () {
        rt = cloud.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.zero;
    }
	
	// Update is called once per frame
	void Update () {
		////////////// have it follow the player on screen
	}

    public void openInventory() {
        StopAllCoroutines();
        StartCoroutine(openCloud());
    }

    public void closeInventory() {
        StopAllCoroutines();
        StartCoroutine(closeCloud());
    }

    IEnumerator openCloud() {
        while (rt.sizeDelta.x < baseSize.x) {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x + 6 * resizeSpeed, rt.sizeDelta.y + 5.1f * resizeSpeed);
            yield return null;
        }
        rt.sizeDelta = baseSize;
        yield return null;
    }

    IEnumerator closeCloud() {
        while (rt.sizeDelta.x > 0) {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x - 6 * resizeSpeed, rt.sizeDelta.y - 5.1f * resizeSpeed);
            yield return null;
        }
        rt.sizeDelta = Vector2.zero;
        yield return null;
    }
}
