using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class PlayerInventory : NetworkBehaviour {
	
	private Player player;
	public bool isAwaitingInventorySync;

	// An array of ints to hold item IDs. I guess we're using '0' for empty.
	// Some of these slots will only be accessible when a backpack is equipped. In theory. That's the plan.
	// Ideally this should only be managed on the server. Currently the array is retrieved from the server and passed around locally.
	[SerializeField] private int[,] inventory = new int[6,9]; 

	public override void OnStartLocalPlayer ()
	{
		player = GetComponent<Player> ();
	}


	public int[,] GetInventoryArray ()
	{
		Debug.Log("GetInventoryArray() called");
		return inventory;
	}
		

	public void RequestSyncInventory()
	{
		isAwaitingInventorySync = true;
		CmdRequestSyncInventory ();
	}

	[Command]
	void CmdRequestSyncInventory ()
	{
		Debug.Log ("[SERVER] CmdReturnServerInventory called. " + this.GetComponent<NetworkIdentity> ());

		int[] deconstructedInventory = new int[54];

		for (int y = 0; y < inventory.GetLength(1); y++)
		{
			for (int x = 0; x < inventory.GetLength(0); x++)
			{
				deconstructedInventory [y * inventory.GetLength (0) + x] = inventory[x, y];
			}
		}

		TargetSyncInventory (this.connectionToClient, deconstructedInventory);
	}

	[TargetRpc]
	void TargetSyncInventory (NetworkConnection connection, int[] deconstructedInventory)
	{
		Debug.Log ("TargetRpcSetLocalInventory called on " + connection.address + ", " + this.gameObject.name);
		int[,] rebuiltInventory = new int[6, 9];
		for (int y = 0; y < 9; y++ )
		{
			for (int x = 0; x < 6; x++)
			{
				rebuiltInventory [x, y] = deconstructedInventory [y * 6 + x];
			}
		}
		inventory = rebuiltInventory;
		isAwaitingInventorySync = false;
	}


		
	public bool AddItemToInventory (int itemID, out int slotX, out int slotY) // Check if there's space for an item, and if so call command to add the item and return true.
	{          
		Debug.Log ("AddItemToInventory called.");
		Debug.Log ("Current inventory x length: " + inventory.GetLength (0));
		Debug.Log ("Current item at inventory[0, 0]: " + GetInventoryArray() [0, 0]);
		for (int y = 0; y < inventory.GetLength(1); y++, Debug.Log(y))
		{
			for (int x = 0; x < inventory.GetLength(0); x++)
			{
				if (GetInventoryArray()[x,y] == 0)
				{
					CmdPlaceItemInSlot (itemID, x, y);
					slotX = x;
					slotY = y;
					Debug.Log ("New item at inventory[0, 0]: " + GetInventoryArray() [0, 0]);
					return true;
				}
			}
		}
		slotX = -1; // We're using -1 to denote no available slot. It shouldn't really matter if this method is used properly.
		slotY = -1;
		RequestSyncInventory ();
		Debug.Log (inventory [0, 0]);
		return false;
	}

	[Command]
	void CmdPlaceItemInSlot (int itemID, int slotX, int slotY)
	{
		Debug.Log ("[SERVER] Placing ID " + itemID + " in slot (" + slotX + ", " + slotY + ").");
		inventory [slotX, slotY] = itemID;
		Debug.Log ("[SERVER] Item " + inventory [slotX, slotY] + " is now in server-side inventory at slot " + slotX + ", " + slotY);
	}


	public void DropItem (int slotX, int slotY)
	{
		CmdDropItem (slotX, slotY);
		RequestSyncInventory ();
	}
		
	[Command]
	void CmdDropItem (int slotX, int slotY)
	{
		if (inventory[slotX, slotY] != 0)
		{
			// TODO Throw the item on the ground!
			inventory[slotX, slotY] = 0;
		}
	}

	public void MoveInventoryItem (int oldX, int oldY, int newX, int newY) // Move an item from one slot in the inventory to another
	{
		CmdMoveInventoryItem (oldX, oldY, newX, newY);
		RequestSyncInventory ();
	}

	[Command]
	void CmdMoveInventoryItem (int oldX, int oldY, int newX, int newY)
	{
		Debug.Log (inventory[0,0] + ", " + inventory[0,0]);

		int startItemID = inventory [oldX, oldY];
		int endItemID = inventory [newX, newY];

		inventory [newX, newY] = startItemID; // Literally just swap item ID of two slots in inventory
		inventory [oldX, oldY] = endItemID;

		Debug.Log ("[SERVER] Moved inventory item from " + oldX + ", " + oldY + " to " + newX + ", " + newY);
		Debug.Log (inventory[0,0] + ", " + inventory[0,0]);
	}


	[Command]
	public void CmdSaveInventoryPlayerPrefs ()
	{
		PlayerPrefsManager.SetPlayerInventory (player.playerID, inventory);
	}

	[Command]
	public void CmdLoadInventoryPlayerPrefs ()
	{
		inventory = PlayerPrefsManager.GetPlayerInventory (player.playerID);
	}

}
