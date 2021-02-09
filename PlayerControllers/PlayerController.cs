using UnityEngine;

public class PlayerController : MonoBehaviour
{

	public float speed = 3;
	public float acceleration = 5;
	public float interactiveRadius = 1.5f;

	private float lastInputAngle = 0;
	private Rigidbody rb;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	public void Move(float dx, float dz)
	{
		// current angle character is facing
		float angle = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

		// last direction we are facing for animation purposes
		if (dx != 0 && dz != 0)
			lastInputAngle = Mathf.Atan2(dz, dx);

		// gets input direction based on dx and dx, multiplied by the angle
		Vector3 direction = new Vector3(Mathf.Cos(angle) * dx + Mathf.Sin(angle) * dz, 0, Mathf.Cos(angle) * dz - Mathf.Sin(angle) * dx);

		rb.velocity = direction * speed;
	}

	public Collider CheckIfNear()
	{
		// check to see if there are nearby objects or NPCs we can talk to
		Collider[] interactables;
		LayerMask lm = LayerMask.GetMask("Interactable");
		interactables = Physics.OverlapSphere(transform.position, interactiveRadius, lm);

		Collider closest = null;
		foreach (Collider c in interactables)
			if (closest == null || (closest.transform.position - transform.position).magnitude > (c.transform.position - transform.position).magnitude)
				closest = c;
		// sorts based on distance
		//System.Array.Sort(interactables, (p1, p2) => (p1.transform.position - transform.position).magnitude.CompareTo((p2.transform.position - transform.position).magnitude));

		return closest;
	}
}
