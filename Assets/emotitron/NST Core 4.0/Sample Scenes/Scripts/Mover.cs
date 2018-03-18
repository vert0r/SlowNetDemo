using UnityEngine;
using emotitron.Network.NST;

/// <summary>
/// Basic automatic transform mover for objects for network testing.
/// </summary>
public class Mover : MonoBehaviour
{
	public float range;
	public float rate;

	private void Start()
	{
		if (!MasterNetAdapter.ServerIsActive)
		{
			Destroy(this);
		}
	}
	void Update ()
	{
		gameObject.transform.localPosition = new Vector3(0, 0, Mathf.Sin(Time.time * rate) * range);


	}
}
