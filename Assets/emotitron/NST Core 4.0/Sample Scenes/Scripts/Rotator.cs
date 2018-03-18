using UnityEngine;
using emotitron.Network.NST;

/// <summary>
/// Rotates and object if it has as NetworkSyncTransform on it, and if it has authority. Used for demonstration only.
/// </summary>
public class Rotator : NSTComponent, INstPostInterpolate
{
	public float speed = 20;

	// Runs on the NST Update, so that this doesn't disable.
	public void OnNstPostInterpolate()
	{
		// Only objects with authority should be moving things.
		if (na != null && na.HasAuthority)
			transform.localEulerAngles = new Vector3(0, 0, Time.time * speed % 360);
		else
			Destroy(this);
	}
}
