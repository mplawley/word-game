using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Letter : MonoBehaviour
{
	private char _c; //The char shown on this letter
	public TextMesh tMesh; //The TextMesh shows the char
	public Renderer tRend; //The Renderer of the 3D Text. This will determine whether the char is visible
	public bool big = false; //Big letters act differently

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
			transform.position = value;
		}
	}
}
