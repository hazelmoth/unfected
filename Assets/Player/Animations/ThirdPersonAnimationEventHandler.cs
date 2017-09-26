using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonAnimationEventHandler : MonoBehaviour {

	PlayerAnimationController animationController;

	void Start ()
	{
		animationController = Player.localPlayer.GetComponent<PlayerAnimationController> ();
	}

	void SpawnWeapon ()
	{
		animationController.SpawnItemThirdPerson ();
		animationController.ResetThirdPersonForceHolsterTrigger ();
	}

	public void DespawnWeapon ()
	{
		animationController.DespawnItemThirdPerson ();
	}

}
