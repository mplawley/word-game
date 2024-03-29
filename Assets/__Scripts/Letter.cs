﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Letter : MonoBehaviour
{
	private char _c; //The char shown on this letter
	public TextMesh tMesh; //The TextMesh shows the char
	public Renderer tRend; //The Renderer of the 3D Text. This will determine whether the char is visible
	public bool big = false; //Big letters act differently

	//Linear interpolation fields
	public List<Vector3> pts = null;
	public float timeDuration = 0.5f;
	public float timeStart = -1;
	public string easingCurve = Easing.InOut; //Easing from Utils.cs

	void Awake()
	{
		tMesh = GetComponentInChildren<TextMesh>();
		tRend = tMesh.GetComponent<Renderer>();
		visible = false;
	}

	//Used to get or set _c and the letter shown by 3D Text
	public char c
	{
		get
		{
			return _c;
		}
		set
		{
			_c = value;
			tMesh.text = _c.ToString();
		}
	}

	//Gets or sets _c as a strin
	public string str
	{
		get
		{
			return _c.ToString();
		}
		set
		{
			c = value[0];
		}
	}

	//Enables or disables the renderer for 3D Text, which causes the char to be visible or invisible respectively
	public bool visible
	{
		get
		{
			return tRend.enabled;
		}
		set
		{
			tRend.enabled = value;
		}
	}

	//Gets or sets the color of the rounded rectangle
	public Color color
	{
		get
		{
			return GetComponent<Renderer>().material.color;
		}
		set
		{
			GetComponent<Renderer>().material.color = value;
		}
	}

	//Sets the position of the Letter's gameObject
	public Vector3 pos
	{
		set
		{
			//transform.position = value; No longer needed

			//Find a midpoint that is a random distance from the actual midpoint between the current position and the value passed in
			Vector3 mid = (transform.position + value) / 2f;

			//The random distance will be within 1/4th of the magnitude of the line from the actual midpoint
			float mag = (transform.position - value).magnitude;
			mid += Random.insideUnitSphere * mag * 0.25f;

			//Creates a List<Vector3> of Bezier points
			pts = new List<Vector3>() { transform.position, mid, value };

			//If timeStart is at the default -1, then set it
			if (timeStart == -1)
			{
				timeStart = Time.time;
			}
		}
	}

	//Moves immediately to the new position
	public Vector3 position
	{
		set 
		{
			transform.position = value;
		}
	}

	//Interpolation code
	void Update()
	{
		if (timeStart == -1)
		{
			return;
		}

		//Standard linear interpolation code
		float u = (Time.time - timeStart) /timeDuration;
		u = Mathf.Clamp01(u);
		float u1 = Easing.Ease(u, easingCurve);
		Vector3 v = Utils.Bezier(u1, pts);
		transform.position = v;

		//If the interpolation is done, set timeStart back to -1
		if (u == 1)
		{
			timeStart = -1;
		}
	}
}
