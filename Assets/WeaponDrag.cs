using UnityEngine;
using System.Collections;

public class WeaponDrag : MonoBehaviour {
	
	[SerializeField] private float MoveAmount = 1;
	[SerializeField] private float MoveSpeed = 2;
	[SerializeField] private float MoveOnX;
	[SerializeField] private float MoveOnY;
	private Vector3 DefaultPos;
	private Vector3 NewGunPos;

	void Start () 
	{
		DefaultPos = transform.localPosition;    
	}           

	void Update () 
	{
		MoveOnX = Input.GetAxis("Mouse X") * Time.deltaTime * MoveAmount;

		MoveOnY = Input.GetAxis("Mouse Y") * Time.deltaTime * MoveAmount;

		NewGunPos = new Vector3 (DefaultPos.x+MoveOnX, DefaultPos.y+MoveOnY, DefaultPos.z);

		gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, NewGunPos, MoveSpeed*Time.deltaTime);
	}
}