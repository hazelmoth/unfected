using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item {

	[SerializeField] private int damageToHead = 100;
	[SerializeField] private int damageToTorso = 50;
	[SerializeField] private int damageToLimbs = 25;

	public int DamageToHead ()
	{
		return damageToHead;
	}

	public int DamageToTorso ()
	{
		return damageToTorso;
	}

	public int DamageToLimbs ()
	{
		return damageToLimbs;
	}
}
