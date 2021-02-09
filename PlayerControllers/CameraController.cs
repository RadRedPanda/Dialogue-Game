using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject player;
	public GameObject billboardContainer;

	public Vector2 speed = new Vector2(3, 3);
	public float distance = 3;
	public float dialogueDistance = 1.5f;
	public Vector2 yBounds = new Vector2(-9, 60);
	public Vector2 scrollBounds = new Vector2(1, 7);
	public float cameraSnapSpeed = 0.1f;
	public int cameraMode = 0;
	public float cameraSpeed;

	private Vector3 camVelocity;
	private Transform[] billboards;
	private Vector2 rotation;
	private Vector3 playerPrevPos;
	private GameObject target;

	void Start()
	{
		billboards = billboardContainer.GetComponentsInChildren<Transform>();
		playerPrevPos = player.transform.position;
		camVelocity = Vector3.zero;
	}

	public void setTarget(GameObject t)
	{
		target = t;
	}

	public void inDialogue()
	{
		Vector3 middle = (target.transform.position + player.transform.position) / 2;
		// slerp the cam
		Vector3 currentPos = transform.position - player.transform.position;
		Vector3 targetPos = (target.transform.position - player.transform.position) / 2 + (Quaternion.AngleAxis(90, Vector3.up) * (target.transform.position - player.transform.position)).normalized * dialogueDistance;
		//transform.position = Vector3.Slerp(currentPos, targetPos, cameraSnapSpeed) + player.transform.position;
		transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref camVelocity, cameraSpeed) + player.transform.position;
		transform.LookAt(middle);
	}

	public void followPlayer(float scroll, float mx, float my)
	{
		// sets the speed
		float mouseX = mx * speed.x;
		float mouseY = my * speed.y;

		// calculates at what rotation the camera should be in
		rotation = new Vector2((rotation.x + mouseX) % 360, Mathf.Max(Mathf.Min(rotation.y - mouseY, yBounds.y), yBounds.x));

		// changes how far the camera is
		distance = Mathf.Max(Mathf.Min(distance - scroll, scrollBounds.y), scrollBounds.x);

		// make camera not lag behind when moving
		transform.position += player.transform.position - playerPrevPos;
		playerPrevPos = player.transform.position;

		// slerp the cam
		Vector3 currentPos = transform.position - player.transform.position;
		Vector3 targetPos = distance * new Vector3(-Mathf.Sin(Mathf.Deg2Rad * rotation.x) * Mathf.Cos(Mathf.Deg2Rad * rotation.y), Mathf.Sin(Mathf.Deg2Rad * rotation.y), -Mathf.Cos(Mathf.Deg2Rad * rotation.x) * Mathf.Cos(Mathf.Deg2Rad * rotation.y));
		transform.position = Vector3.Slerp(currentPos, targetPos, cameraSnapSpeed) + player.transform.position;

		// always looks at the player
		transform.LookAt(player.transform);
	}

	public void turnBillboards()
	{
		// makes all billboard objects face the same direction, might get expensive, change to only turn things player can see?
		for (int i = 1; i < billboards.Length; i++)
			billboards[i].rotation = Quaternion.Euler(new Vector3(billboards[i].rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0));
	}
}