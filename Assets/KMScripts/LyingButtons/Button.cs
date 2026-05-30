using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button 
{
	public ButtonColor buttonColor;
	public bool isTruth;
	public ClueObj clue;
	public string coord;
	public Button(ButtonColor buttonColor, bool isTruth, string coord)
	{
		this.buttonColor = buttonColor;
		this.isTruth = isTruth;
		this.coord = coord;
	}
	public Button(ButtonColor buttonColor, bool isTruth, string coord, ClueObj clue)
	{
		this.buttonColor = buttonColor;
		this.isTruth = isTruth;
		this.coord = coord;
		this.clue = clue;
	}

	public string toString()
	{

		string end = isTruth ? "Honest" : "Lying";

		string clueStr = ":";
		if (clue != null)
			clueStr = clue.toString();
		return getRow(coord[1]) + "" + getCol(coord[0]) + " " + buttonColor + " " + clueStr + " (" + end + ")";
	}
	private char getRow(char r)
	{
		switch(r)
		{
			case '1': return 'T';
			case '2': return 'M';
			case '3': return 'B';
		}
		return '-';
	}
	private char getCol(char r)
	{
		switch (r)
		{
			case 'A': return 'L';
			case 'B': return 'M';
			case 'C': return 'R';
		}
		return '-';
	}
}
