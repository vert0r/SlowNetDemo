using UnityEngine;
using emotitron.Network.NST;

/// <summary>
/// Automatic spawn points for testing. This makes use of the NSTMasterBehaviour - removing the need for a NetworkIdentity to
/// detect network starts.
/// </summary>
public class SpawnObject : NSTMasterBehaviour, INstMasterOnStartServer
{
	public GameObject prefab;

	public void OnNstMasterStartServer()
	{
		if (MasterNetAdapter.ServerIsActive)
		{
			GameObject go = Instantiate(prefab, null);
			go.transform.position = transform.position;
			MasterNetAdapter.Spawn(go);
		}
	}
}
