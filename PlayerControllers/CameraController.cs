using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject player;
	public GameObject billboardContainer;

	public Vector3 speed = new Vector3(10, 3, 3);
	public float distance = 3;
	public float dialogueDistance = 1.5f;
	public Vector2 yBounds = new Vector2(-9, 60);
	public Vector2 scrollBounds = new Vector2(1, 7);
	public float cameraSnapSpeed = 0.1f;
	public int cameraMode = 0;
	public float cameraSpeed;

	public float lookOffset;

	private Vector3 camVelocity;
	private Transform[] billboards;
	private Vector3 playerPrevPos;
	private GameObject target;
	private Vector3 targetRotation;
	private Vector3 currentRotation;
	private Vector3 rotationVelocity;

	void Start()
	{
		billboards = billboardContainer.GetComponentsInChildren<Transform>();
		playerPrevPos = player.transform.position;
		camVelocity = Vector3.zero;
		rotationVelocity = Vector2.zero;
	}

	public void setTarget(GameObject t)
	{
		target = t;
	}

	public void inDialogue()
	{
		Vector3 middle = (target.transform.position + player.transform.position) / 2;
		middle.Set(middle.x, middle.y - lookOffset, middle.z);
		// slerp the cam
		Vector3 currentPos = transform.position - player.transform.position;
		Vector3 targetPos = (target.transform.position - player.transform.position) / 2 + (Quaternion.AngleAxis(90, Vector3.up) * (target.transform.position - player.transform.position)).normalized * dialogueDistance;
		transform.position = Vector3.SmoothDamp(currentPos, targetPos, ref camVelocity, cameraSpeed) + player.transform.position;
		transform.LookAt(middle);
	}

	public void followPlayer(float scroll, float mx, float my)
	{
		// sets the speed
		float mouseX = mx * speed.x;
		float mouseY = my * speed.y;

		// changes how far the camera is
		distance = Mathf.Max(Mathf.Min(distance - (scroll * speed.z), scrollBounds.y), scrollBounds.x);

		// calculates at what rotation the camera should be in
		targetRotation = new Vector3((targetRotation.x + mouseX), Mathf.Max(Mathf.Min(targetRotation.y - mouseY, yBounds.y), yBounds.x), distance);

		// lerp the camera's rotation
		currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, cameraSpeed);


		// make camera not lag behind when moving
		transform.position += player.transform.position - playerPrevPos;
		playerPrevPos = player.transform.position;

		// calculate camera pos from rotation
		Vector3 targetPos = currentRotation.z * new Vector3(-Mathf.Sin(Mathf.Deg2Rad * currentRotation.x) * Mathf.Cos(Mathf.Deg2Rad * currentRotation.y), Mathf.Sin(Mathf.Deg2Rad * currentRotation.y), -Mathf.Cos(Mathf.Deg2Rad * currentRotation.x) * Mathf.Cos(Mathf.Deg2Rad * currentRotation.y));
		transform.position = targetPos + player.transform.position;

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