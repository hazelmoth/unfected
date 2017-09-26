using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

	[SerializeField] private int itemID;
	[SerializeField] private int animationID;
	[SerializeField] private string itemName;
	[SerializeField] private string description;
	[SerializeField] private GameObject itemModel;
	[SerializeField] private float aimFOVMultiplier = 1.0f;

	public int ItemID              { get { return itemID; }}
	public int AnimationID         { get { return animationID; }}
	public string ItemName         { get { return itemName; }}
	public string  Description     { get { return description; }}
	public float AimFovMultiplier  { get { return aimFOVMultiplier; }}
	public GameObject ItemModel    { get { return itemModel; }}
	public Sprite Icon;

	void Start()
	{
		if (itemID == 0)
		{
			Debug.LogError("Item \"" + itemName + "\" has an ID of zero. Which shouldn't happen. It makes no sense. Fix it.");
		}
	}


}
