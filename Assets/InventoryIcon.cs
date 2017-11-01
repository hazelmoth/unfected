using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler {

	private RectTransform rectTransform;
	private Vector3 startPosition;
	private bool isDragging = false;
	private GameObject lastTouchedObject;
	private GameObject originalParent;
	private GameObject draggingParent; // Parent object to child icons to as they are being dragged

	void Start () {
		rectTransform = GetComponent<RectTransform> ();
		originalParent = gameObject.transform.parent.gameObject;
		draggingParent = GameObject.Find ("Drag Parent Object");
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		if (!isDragging)
		{
			GameObject activeSlot = eventData.pointerCurrentRaycast.gameObject;

			if (activeSlot.GetComponent<InventoryIcon>())
			{
				activeSlot = activeSlot.transform.parent.gameObject;
			}

			if (activeSlot.tag == "Inventory Slot")
			{
				UIManager.instance.SetDisplayedInventorySlotAfterSync(activeSlot);
				Debug.Log ("Setting displayed inventory slot from InventoryIcon");
			}
		}
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		startPosition = rectTransform.position;
		isDragging = true;

		gameObject.transform.SetParent (draggingParent.transform);
	}

	public void OnDrag (PointerEventData eventData)
	{
		rectTransform.position = Input.mousePosition;

		lastTouchedObject = eventData.pointerCurrentRaycast.gameObject;
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		ManageDrag (lastTouchedObject);
	}
		

	void OnApplicationFocus(bool isFocused)
	{
		if (!isFocused && isDragging)
		{
			rectTransform.position = startPosition;
			isDragging = false;
			gameObject.transform.SetParent (originalParent.transform);
		}
	}

	void ManageDrag (GameObject dragDestination)
	{
		if (dragDestination.GetComponent<InventoryIcon>())
		{
			dragDestination = dragDestination.transform.parent.gameObject;
		}

		if (dragDestination.tag == "Inventory Slot")
		{
			UIManager.instance.ManageInventoryDrag (originalParent, dragDestination);
			Invoke ("ResetIconPosition", 0.1f);
		}
		else if (dragDestination.tag != "Inventory Panel" && dragDestination.transform.parent.tag != "Inventory Panel") 
		{
			// If an item is released not over any collection of inventory slots, drop it
			Debug.Log (dragDestination.name);
			Vector2 inventorySlotToDrop = UIManager.instance.FindIndexOfInventorySlot (originalParent);
			UIManager.instance.ActivateDropItemAfterSync ((int)inventorySlotToDrop.x, (int)inventorySlotToDrop.y);
			ResetIconPosition ();
		}
		else 
		{
			ResetIconPosition ();
		}
	}

	void ResetIconPosition ()
	{
		isDragging = false;
		rectTransform.position = startPosition;
		gameObject.transform.SetParent (originalParent.transform);
	}



}
