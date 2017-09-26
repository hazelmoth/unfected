using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {

	[SerializeField] private ItemDictionary itemDictionary;
	public static ItemDictionary Dictionary;

	// Use this for initialization
	void Start () {
		if (itemDictionary)
		{
			Dictionary = itemDictionary;
		}
		else 
		{
			Debug.LogError ("No ItemDictionary assigned in inspector for ItemManager");
		}
	}

}
