using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour 
{
	public GameObject CannonPrefb;
	void Start ()
	{
		initializeGamePlay();	
	}
	void initializeGamePlay()
	{
		GameObject cannon = (GameObject) Instantiate(CannonPrefb);
	}
	void Update () 
	{
	}
}