using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimationEventHandler : MonoBehaviour {

	PlayerAnimationController animationController;

	void Start ()
	{
		animationController = Player.localPlayer.GetComponent<PlayerAnimationController> ();
	}

	void SpawnWeapon ()
	{
		animationController.SpawnItemFirstPerson ();
		animationController.ResetFirstPersonForceHolsterTrigger ();
	}

	void DespawnWeapon ()
	{
		Player.localPlayer.GetComponent<PlayerAnimationController> ().DespawnItemFirstPerson ();
	}

}
