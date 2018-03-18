using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;
using UnityEngine.UI;
using emotitron.Network.NST;

public class Startup : MonoBehaviour
{
	public int targetFramerate = 60;
	public Text framerateValueText;
	public Slider framerateSlider;

	private void Awake()
	{
		// Only show the framerate silder if vsync if off... it does nothing if it is on.
		if (QualitySettings.vSyncCount != 0)
		{
			framerateSlider.gameObject.SetActive(false);
			SetFrameRate(targetFramerate);
		}

	}
	void Start ()
	{

		// Make sure screen is big enough on mobile to mess with the network buttons.
		if (Screen.width > 1440)
			Screen.SetResolution(Screen.width / 3, Screen.height / 3, false);
	}

	// Teleport shortcuts
	private void Update()
	{
		if (MasterNetAdapter.ServerIsActive)
		{
			if (Input.GetKeyDown("0"))
				NetworkSyncTransform.allNsts[0].Teleport(MasterNetAdapter.GetPlayerSpawnPoint());

			if (Input.GetKeyDown("9"))
				NetworkSyncTransform.allNsts[1].Teleport(MasterNetAdapter.GetPlayerSpawnPoint());

			if (Input.GetKeyDown("8"))
				NetworkSyncTransform.allNsts[2].Teleport(MasterNetAdapter.GetPlayerSpawnPoint());

		}

	}

	public void SetFrameRate (Single rate)
	{
		
		Application.targetFrameRate = (int)rate;
		framerateValueText.text = ((int)rate).ToString();
		framerateSlider.value = rate;
	}

}
