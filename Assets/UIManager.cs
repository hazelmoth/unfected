using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour {

	[SerializeField] private Color inventorySlotColorInactive = new Color32 (45, 45, 45, 200);
	[SerializeField] private Color inventorySlotColorActive = new Color32 (161, 161, 161, 200);
	[SerializeField] private Text interactText = null;
	[SerializeField] private Text inventoryItemTitle = null;
	[SerializeField] private Text inventoryItemDescription = null;
	[SerializeField] private Image inventoryItemImage = null;
	[SerializeField] private GameObject inventoryDragParent = null;
	[SerializeField] private GameObject inventoryPanel = null;
	[SerializeField] private GameObject[] inventoryRows = null;


	private GameObject currentSelectedIcon;
	private RectTransform currentIconTransform;
	private GameObject[,] inventoryPanelArray = new GameObject[6,9];
	private CursorLockMode currentCursorLockMode = CursorLockMode.None;
	private bool isInventoryOpen = false;
	private int? activeHotbarSlot = null;

	public static UIManager instance;


	void Start ()
	{
		instance = this;

		EventSystem.current.pixelDragThreshold = 0;
		inventoryPanel.SetActive (false);

		for (int y = 0; y < inventoryRows.Length; y++) // Find all the slots in the inventory rows and populate the inventoryPanelArray array with UI panels
		{
			for (int x = 0; x < inventoryRows[y].transform.childCount; x++)
			{
				inventoryPanelArray [x, y] = inventoryRows [y].transform.Find ("Slot " + x).gameObject;
			}
		}
	}

	void OnApplicationFocus(bool isFocused) // Handles cursor state after regaining focus
	{
		if (isFocused)
		{
			if (currentCursorLockMode == CursorLockMode.Locked)
			{
				EnableCursorLock();
			}
			else
			{
				DisableCursorLock();
			}
		}
	} 
		

	void UpdateInventoryPanels (int[,] inventoryArray)
	{
		for (int y = 0; y < inventoryPanelArray.GetLength(1); y++)
		{
			for (int x = 0; x < inventoryPanelArray.GetLength(0); x++)
			{
				Image iconImage = null;
				int id = inventoryArray [x, y];

				if (inventoryPanelArray [x, y].transform.childCount >= 1) 
				{
					iconImage = inventoryPanelArray [x, y].transform.GetChild (0).GetComponent<Image> ();
				}
				else
				{
					iconImage = inventoryDragParent.GetComponentInChildren<Image> ();
				}

				if (!iconImage)
					throw new UnityException ("Inventory slot panel missing image component on child");

				if (id == 0)
				{
					iconImage.enabled = false;
				}
				else 
				{
					iconImage.enabled = true;
					iconImage.sprite = ItemManager.Dictionary.GetItem (id).Icon;

					Debug.Log ("UpdateInventoryPanels found a " + ItemManager.Dictionary.GetItemObject (id).name + " at " + x + ", " + y);
				}
			}
		}
	}

	void UpdateActiveHotbarSlot ()
	{
		for (int i = 0; i < 6; i++) 
		{
			inventoryPanelArray [i, 0].GetComponent<Image> ().color = inventorySlotColorInactive;
		}

		if (activeHotbarSlot != null) 
		{
			inventoryPanelArray [(int)activeHotbarSlot, 0].GetComponent<Image> ().color = inventorySlotColorActive;
		}
	}
		
	void SetDisplayedInventorySlot (GameObject inventorySlot, int[,] inventoryArray)
	{
		PlayerInventory playerInventory = Player.localPlayer.GetComponent<PlayerInventory> ();

		Vector2 index = FindIndexOfInventorySlot (inventorySlot);
		int itemId = inventoryArray [(int)index.x, (int)index.y];

		if (itemId == 0)
		{
			EmptyInventoryDescription ();
			return;
		}
		Item item = ItemManager.Dictionary.GetItem (itemId);

		inventoryItemImage.enabled = true;
		inventoryItemTitle.text = item.ItemName;
		inventoryItemDescription.text = item.Description;
		inventoryItemImage.sprite = item.Icon;
	}

	void ActivateDropItem (int slotX, int slotY)
	{
		Player.localPlayer.ThrowItemOnGround (slotX, slotY); // NOTE: This assumes that the item we're throwing on the ground has NOT YET BEEN DELETED by the following line.
		Player.localPlayer.GetComponent<PlayerInventory> ().DeleteItemFromInventory (slotX, slotY);
		Player.localPlayer.GetComponent<PlayerInventory> ().RequestSyncInventory ();
		Player.localPlayer.UpdateItemInCurrentHotbarSlot ();
		UpdateInventoryPanelsAfterSync ();
		Debug.Log ("ActivateDropItem called");
	}


	IEnumerator UpdateInventoryPanelsCoroutine ()
	{
		PlayerInventory inv = Player.localPlayer.GetComponent<PlayerInventory> ();
		if (!inv)
		{
			Debug.LogError ("No inventory found on local player");
			yield break;
		}

		inv.RequestSyncInventory ();
		float syncStartTime = Time.unscaledTime;

		while (inv.isAwaitingInventorySync)
		{
			if (Time.unscaledTime - syncStartTime > 10.0f) // End coroutine if we've been waiting for sync longer than 10 seconds. TODO: maybe make a universal time out length somewhere.
			{
				Debug.LogError("Timed out attempting to retrieve inventory");
				yield break;
			}
			yield return null;
		}

		UpdateInventoryPanels (inv.GetInventoryArray());
	}

	IEnumerator SetDisplayedInventorySlotCoroutine (GameObject inventorySlot)
	{
		PlayerInventory inv = Player.localPlayer.GetComponent<PlayerInventory> ();
		if (!inv)
		{
			Debug.LogError ("No inventory found on local player");
			yield break;
		}

		inv.RequestSyncInventory ();
		float syncStartTime = Time.unscaledTime;

		while (inv.isAwaitingInventorySync)
		{
			if (Time.unscaledTime - syncStartTime > 10.0f) // End coroutine if we've been waiting for sync longer than 10 seconds. TODO: maybe make a universal time out length somewhere.
			{
				Debug.LogError("Timed out attempting to retrieve inventory");
				yield break;
			}
			yield return null;
		}

		SetDisplayedInventorySlot (inventorySlot, inv.GetInventoryArray());
	}

	IEnumerator ActivateDropItemCoroutine (int slotX, int slotY)
	{
		Debug.Log ("ActivateDropItemCoroutine started");
		PlayerInventory inv = Player.localPlayer.GetComponent<PlayerInventory> ();
		if (!inv)
		{
			Debug.LogError ("No inventory found on local player");
			yield break;
		}

		inv.RequestSyncInventory ();
		float syncStartTime = Time.unscaledTime;

		while (inv.isAwaitingInventorySync)
		{
			if (Time.unscaledTime - syncStartTime > 10.0f) // End coroutine if we've been waiting for sync longer than 10 seconds. TODO: maybe make a universal time out length somewhere.
			{
				Debug.LogError("Timed out attempting to retrieve inventory");
				yield break;
			}
			yield return null;
		}

		ActivateDropItem (slotX, slotY);
	}
		

	public void UpdateInventoryPanelsAfterSync ()
	{
		StartCoroutine (UpdateInventoryPanelsCoroutine());
	}

	public void SetDisplayedInventorySlotAfterSync (GameObject inventorySlot)
	{
		StartCoroutine (SetDisplayedInventorySlotCoroutine (inventorySlot));
	}

	public void ActivateDropItemAfterSync (int slotX, int slotY)
	{
		StartCoroutine (ActivateDropItemCoroutine (slotX, slotY));
	}

	public Vector2 FindIndexOfInventorySlot (GameObject slot)
	{
		if (slot.tag != "Inventory Slot")
		{
			Debug.LogError ("Object passed into FindIndexOfSlot is not a slot!");
			return new Vector2();
		}

		for (int y = 0; y < inventoryPanelArray.GetLength(1); y++)
		{
			for (int x = 0; x < inventoryPanelArray.GetLength(0); x++)
			{
				if (inventoryPanelArray[x, y].GetInstanceID() == slot.GetInstanceID())
				{
					Vector2 index = new Vector2 (x, y);
					Debug.Log ("Index for " + slot.name + " located as " + index);
					return index;
				}
			}
		}

		Debug.LogError ("No index found for given object");
		return new Vector2();
	}

	public void ManageInventoryDrag (GameObject draggedSlot, GameObject destinationSlot)
	{
		PlayerInventory playerInventory = Player.localPlayer.GetComponent<PlayerInventory> ();

		Debug.Log ("Managing inventory drag of " + draggedSlot.name + " to " + destinationSlot.name);
		Vector2 start = FindIndexOfInventorySlot (draggedSlot);
		Vector2 end = FindIndexOfInventorySlot (destinationSlot);

		playerInventory.MoveInventoryItem((int)start.x, (int)start.y, (int)end.x, (int)end.y);
		UpdateInventoryPanelsAfterSync ();
		Player.localPlayer.UpdateItemInCurrentHotbarSlot (); // Make sure the animations follow properly if we move the item in the current hotbar slot
	}
		
	public void EmptyInventoryDescription ()
	{
		inventoryItemTitle.text = "";
		inventoryItemDescription.text = "";
		inventoryItemImage.enabled = false;
	}

	public void EnableInventoryScreen ()
	{
		inventoryPanel.SetActive (true);
		isInventoryOpen = true;
		EmptyInventoryDescription ();
		DisableCursorLock ();
	}

	public void DisableInventoryScreen ()
	{
		inventoryPanel.SetActive (false);
		isInventoryOpen = false;
		EnableCursorLock ();
	}

	public bool IsInventoryOpen ()
	{
		return isInventoryOpen;
	}

	public void SetActiveHotbarSlot (int slot)
	{
		if (slot < 0 || slot > 5)
		{
			Debug.LogError ("Active hotbar slot out of range (should be 0-5)");
			return;
		}
		activeHotbarSlot = slot;
		UpdateActiveHotbarSlot ();
	}

	public void ClearActiveHotbarSlot ()
	{
		activeHotbarSlot = null;
		UpdateActiveHotbarSlot ();
	}

	public void SetInteractTextToItem (Item item)
	{
		interactText.text = "Press 'E' to pick up " + item.ItemName;
	}

	public void DisableInteractText ()
	{
		interactText.text = "";
	}

	public void EnableCursorLock ()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		currentCursorLockMode = CursorLockMode.Locked;
	}

	public void DisableCursorLock ()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		currentCursorLockMode = CursorLockMode.None;
	}

	public void DebugUI()
	{
		Debug.Log ("Cursor lock state: " + Cursor.lockState);
		Debug.Log ("Lock state variable: " + currentCursorLockMode);
		Debug.Log ("Cursor visibility: " + Cursor.visible);
		Debug.Log ("Inventory active: " + isInventoryOpen);
		Debug.Log ("Actual inventory visibility: " + inventoryPanel.activeInHierarchy);
	}
}
