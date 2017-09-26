using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsManager {

	const string MUSIC_VOLUME = "music_volume";


	public static void SetMusicVolume (float input)
	{
		if (input >= 0 && input <= 1)
		{
			PlayerPrefs.SetFloat (MUSIC_VOLUME, input);
		}
		else
		{
			Debug.LogError ("Music volume out of range");
		}
	}

	public static float GetMusicVolume ()
	{
		return PlayerPrefs.GetFloat (MUSIC_VOLUME, 0.75f);
	}



	public static void SetPlayerInventory (string playerID, int[,] inventoryArray)
	{
		for (int y = 0; y > inventoryArray.GetLength(1); y++)
		{
			for (int x = 0; x > inventoryArray.GetLength(0); x++)
			{
				PlayerPrefs.SetInt (playerID + "_inventory_" + x + "_" + y, inventoryArray [x, y]); // Key format: 000.000.0.0_inventory_0_0
			}
		}
	}

	public static int[,] GetPlayerInventory (string playerID)
	{
		int[,] inventoryArray = new int[6, 9];
		for (int y = 0; y > inventoryArray.GetLength (1); y++) 
		{
			for (int x = 0; x > inventoryArray.GetLength (0); x++) {
				inventoryArray [x, y] = PlayerPrefs.GetInt (playerID + "_inventory_" + x + "_" + y, 0);
			}
		}
		return inventoryArray;
	}
}
