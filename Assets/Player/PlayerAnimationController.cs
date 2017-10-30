using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAnimationController : NetworkBehaviour {

	private const string HandAnimatorBoolUsingHands = "Using Hands";
	private const string HandAnimatorBoolAiming = "Aiming";
	private const string HandAnimatorIntAnimationID = "Animation ID";
	private const string HandAnimatorTriggerForceHolster = "Force Holster";
	private const string PlayerAnimatorFloatWalking = "Walking";
	private const string PlayerAnimatorBoolUsingHands = "Using Hands";
	private const string PlayerAnimatorBoolAiming = "Aiming";
	private const string PlayerAnimatorIntAnimationID = "Animation ID";
	public const string PlayerAnimatorTriggerForceHolster = "Force Holster";

	private GameObject firstPersonRightHand;
	private GameObject thirdPersonRightHand;
	private GameObject currentItemFirstPerson;
	private Animator playerAnimator;
	private Animator handAnimator;
	private NetworkAnimator networkAnimator;
	private float defaultFov;
	private int currentEquippedItemID;

	public int CurrentEquippedItemID { get { return currentEquippedItemID; }}
	public GameObject currentItemThirdPerson;
	public GameObject currentItemThirdPersonShadows;

	public override void OnStartClient ()
	{
		Debug.Log ("start client");
		foreach (Transform child in GetComponentsInChildren<Transform>())
		{
			if (child.tag == "Right Hand Third Person") {
				Debug.Log ("found hand third person");
				thirdPersonRightHand = child.gameObject;
			}
		}
	}

	public override void OnStartLocalPlayer () 
	{
		defaultFov = Player.localPlayer.GetCamera().fieldOfView;
		
		playerAnimator = transform.Find ("Player Model").GetComponent<Animator> ();
		handAnimator = transform.Find ("Camera").GetComponentInChildren<Animator> ();
		networkAnimator = GetComponent<NetworkAnimator> ();

		foreach (Transform child in GetComponentsInChildren<Transform>())
		{
			if (child.tag == "Right Hand First Person") {
				Debug.Log ("found hand fps local");
				firstPersonRightHand = child.gameObject;
			}
			else if (child.tag == "Right Hand Third Person") {
				Debug.Log ("found hand third person local");
				thirdPersonRightHand = child.gameObject;
			}
		}
	}

	public void ActivateHands ()
	{
		playerAnimator.SetBool (PlayerAnimatorBoolUsingHands, true);
		handAnimator.SetBool (HandAnimatorBoolUsingHands, true);
	}

	public void DeactivateHands ()
	{
		playerAnimator.SetBool (PlayerAnimatorBoolUsingHands, false);
		handAnimator.SetBool (HandAnimatorBoolUsingHands, false);
	}

	public void SetAiming (bool isAiming)
	{
		handAnimator.SetBool (HandAnimatorBoolAiming, isAiming);
		playerAnimator.SetBool (PlayerAnimatorBoolAiming, isAiming);
		if (!isAiming) 
		{
			StopAllCoroutines ();
			if (Player.localPlayer.GetCamera().fieldOfView != defaultFov) // Reset the field of view if it's been changed
			{
				StartCoroutine (ResetFov (0.15f));
			}
		}
	}

	public void SetAiming (bool isAiming, float fovMultiplier)
	{
		handAnimator.SetBool (HandAnimatorBoolAiming, isAiming);
		playerAnimator.SetBool (PlayerAnimatorBoolAiming, isAiming);
		if (isAiming) 
		{
			StopAllCoroutines ();
			StartCoroutine (SetFov (fovMultiplier, 0.1f));
		}
		else
		{
			StopAllCoroutines ();
			StartCoroutine (ResetFov (0.25f));
		}
	}
			
	public void SetEquippedItem (int itemID)
	{
		int oldEquippedItemID = currentEquippedItemID;
		currentEquippedItemID = itemID;

		if (itemID == 0)
		{
			playerAnimator.SetInteger (PlayerAnimatorIntAnimationID, 0);
			handAnimator.SetInteger (HandAnimatorIntAnimationID, 0);
			return;
		}

		Item item = ItemManager.Dictionary.GetItem (itemID);
		int oldItemAnimation;

		if (oldEquippedItemID == 0)
		{
			oldItemAnimation = 0;
		}
		else
		{
			oldItemAnimation = ItemManager.Dictionary.GetItem (oldEquippedItemID).AnimationID;
		}

		if (item.AnimationID == oldItemAnimation && handAnimator.GetBool(HandAnimatorBoolUsingHands) && currentEquippedItemID != 0)
		{
			handAnimator.SetTrigger (HandAnimatorTriggerForceHolster); // Make sure we holster the item when switching to another with the same animation (there must be a cleaner way to do this).
			networkAnimator.SetTrigger (PlayerAnimatorTriggerForceHolster); // For some reason triggers only work via a NetworkAnimator.
		}

		playerAnimator.SetInteger (PlayerAnimatorIntAnimationID, item.AnimationID);
		handAnimator.SetInteger (HandAnimatorIntAnimationID, item.AnimationID);
	}

	public void SpawnItemThirdPerson () // Called by anim behaviour
	{
		Debug.Log ("Spawning third person item");
		if (currentEquippedItemID != 0 && currentItemThirdPerson == null)
		{
			CmdSpawnItemThirdPerson (currentEquippedItemID, gameObject.GetComponent<NetworkIdentity>().netId);
		}
	}

	public void DespawnItemThirdPerson () // Called by anim behaviour
	{
		CmdDespawnItemThirdPerson (gameObject.GetComponent<NetworkIdentity>().netId);
		Debug.Log ("Calling command to despawn on " + name);
	}

	public void SpawnItemFirstPerson () // Called by animation event handler
	{
		if (currentEquippedItemID != 0 && currentItemFirstPerson == null)
		{
			currentItemFirstPerson = GameObject.Instantiate (ItemManager.Dictionary.GetItem (currentEquippedItemID).ItemModel, firstPersonRightHand.transform.position, firstPersonRightHand.transform.rotation, firstPersonRightHand.transform);
			foreach(Renderer renderer in currentItemFirstPerson.GetComponentsInChildren<Renderer>())
			{
				renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // Don't cast shadows for first person items
			}
		}
	}

	public void DespawnItemFirstPerson ()
	{
		Destroy (currentItemFirstPerson);
	}

	public void ResetFirstPersonForceHolsterTrigger ()
	{
		handAnimator.ResetTrigger (HandAnimatorTriggerForceHolster);
	}

	public void ResetThirdPersonForceHolsterTrigger ()
	{
		playerAnimator.ResetTrigger (PlayerAnimatorTriggerForceHolster);
	}

	[Command]
	void CmdSpawnItemThirdPerson (int itemID, NetworkInstanceId playerID) // Called by public void SpawnThirdPerson()
	{
		GameObject player = NetworkServer.FindLocalObject (playerID);
		PlayerAnimationController animController = player.GetComponent<PlayerAnimationController> ();

		animController.currentItemThirdPerson = GameObject.Instantiate (ItemManager.Dictionary.GetItem (itemID).ItemModel, thirdPersonRightHand.transform.position, thirdPersonRightHand.transform.rotation, thirdPersonRightHand.transform);
		animController.currentItemThirdPersonShadows = GameObject.Instantiate (ItemManager.Dictionary.GetItem (itemID).ItemModel, thirdPersonRightHand.transform.position, thirdPersonRightHand.transform.rotation, thirdPersonRightHand.transform);

		NetworkServer.Spawn (animController.currentItemThirdPerson);
		NetworkServer.Spawn (animController.currentItemThirdPersonShadows);
		RpcSpawnItemThirdPerson (animController.currentItemThirdPerson.GetComponent<NetworkIdentity> ().netId);
	}

	[Command]
	void CmdDespawnItemThirdPerson (NetworkInstanceId playerID) // Called by public void DespawnThirdPerson()
	{
		GameObject player = NetworkServer.FindLocalObject (playerID);
		PlayerAnimationController animController = player.GetComponent<PlayerAnimationController> ();

		Debug.Log ("[SERVER] Despawn thirdperson weapon command called");
		if (animController.currentItemThirdPerson)
			NetworkServer.Destroy (currentItemThirdPerson);
		if (animController.currentItemThirdPersonShadows)
			NetworkServer.Destroy (currentItemThirdPersonShadows);
	}

	[ClientRpc]
	void RpcSpawnItemThirdPerson (NetworkInstanceId netId) // Sets parent of third person item and sets it to proper visibility layer
	{
		currentItemThirdPerson = ClientScene.FindLocalObject (netId);
		currentItemThirdPerson.transform.SetParent (thirdPersonRightHand.transform);
		if (isLocalPlayer)
		{
			foreach (Transform child in currentItemThirdPerson.GetComponentsInChildren<Transform>())
			{
				child.gameObject.layer = 8;
			}
			foreach (Transform child in currentItemThirdPersonShadows.GetComponentsInChildren<Transform>())
			{
				if (child.GetComponent<Renderer> ())
					child.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
			}
		}
	}

	[ClientRpc]
	void RpcDespawnItemThirdPerson ()
	{
		// We might use this to call something on the clients after calling CmdDespawnItemThirdPerson().
	}

	IEnumerator SetFov (float fovMultiplier, float time) // TODO There's probably a better place to put FOV changing methods than PlayerAnimationController.
	{
		Camera camera = Player.localPlayer.GetCamera ();
		float currentTime = 0.0f;
		float startFov = camera.fieldOfView;
		float targetFov = startFov * fovMultiplier;

		while (currentTime < time)
		{
			currentTime += Time.deltaTime;
			camera.fieldOfView = Mathf.Lerp (startFov, targetFov, currentTime / time);
			yield return null;
		}
	}

	IEnumerator ResetFov (float time)
	{
		Camera camera = Player.localPlayer.GetCamera ();
		float currentTime = 0.0f;
		float startFov = camera.fieldOfView;
		float targetFov = defaultFov;

		while (currentTime < time)
		{
			currentTime += Time.deltaTime;
			camera.fieldOfView = Mathf.Lerp (startFov, targetFov, currentTime / time);
			yield return null;
		}
	}
}
