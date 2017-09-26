using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDictionary", menuName = "Item Dictionary")]

public class ItemDictionary : ScriptableObject {
	
	Dictionary<int, GameObject> itemDictionary = new Dictionary<int, GameObject>();
	[SerializeField] List<GameObject> itemPrefabs = null;

	void OnEnable () 
	{
		BuildItemDictionary ();
	}
		
	void BuildItemDictionary()
	{
		Debug.Log ("Building item dictionary");
		for (int i = 1; i < itemPrefabs.Count; i++)
		{
			itemDictionary.Add (i, itemPrefabs [i]);
			if (itemPrefabs[i].GetComponent<Item>().ItemID != i)
			{
				Debug.LogError("Item ID for item \"" + itemPrefabs[i].GetComponent<Item>().ItemName + "\" is inconsistent with position in dictionary list");
			}
		}
	}

	public GameObject GetItemObject(int itemID)
	{
		return itemDictionary [itemID];
	}

	public Item GetItem(int itemID)
	{
		return itemDictionary [itemID].GetComponent<Item>();
	}
}
