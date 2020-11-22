using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class regretScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMSelectable[] SubButtons;
	public KMSelectable[] StageButtons;
	public GameObject[] Text;
	public GameObject[] Leds;
	public GameObject[] Timer;
	public GameObject[] Displays;
	public KMBombModule Module;

	private int[] initCol;
	private int[] operators;
	private int[] stagesR;
	private int[] stagesG;
	private int[] stagesB;

	private int selected = 0;
	private int[] input = new int[6];
	private int index = 0;
	private int t;

	private bool solved = false;
	private bool checking = false;
	private bool twitch = false;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler StageButtonPressed(int pos)
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Tap", Module.transform);
			StageButtons[pos].AddInteractionPunch();
			if (!solved && !checking)
			{
				switch (pos)
				{
					case 0:
						if (selected > 0)
						{
							selected--;
						}
						break;
					case 1:
						if (selected < 2)
						{
							selected++;
						}
						break;
					default:
						break;
				}
				DispUpdate();
			}
			return false;
		};
	}

	private KMSelectable.OnInteractHandler ButtonPressed(int pos)
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Tap", Module.transform);
			SubButtons[pos].AddInteractionPunch();
			if (!solved && !checking)
			{
				input[index] = pos;
				index++;
				DispUpdate();
				if (index == 6)
					StartCoroutine(SolveCheck());
			}
			return false;
		};
	}

	void Awake () { 
		_moduleID = _moduleIdCounter++;
		Module.OnActivate += ActivateModule;
		for (int i = 0; i < StageButtons.Length; i++)
		{
			StageButtons[i].OnInteract += StageButtonPressed(i);
		}
		for (int i = 0; i < SubButtons.Length; i++)
		{
			SubButtons[i].OnInteract += ButtonPressed(i);
		}
	}

	void Start()
    {
		Stagegen();
	}
	
	private void ActivateModule ()
	{
		Audio.PlaySoundAtTransform("Solve", Module.transform);
		for (int i = 0; i < 3; i++)
		{
			Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(47, 47, 47, 255);
		}
		StartCoroutine(KeepingPace());
		DispUpdate();
	}

	private void Stagegen ()
	{
		stagesR = new int[3];
		stagesG = new int[3];
		stagesB = new int[3];
		initCol = new int[3];
		operators = new int[3];

		for (int i = 0; i < 3; i++)
		{
			stagesR[i] = Rnd.Range(0, 256);
			stagesG[i] = Rnd.Range(0, 256);
			stagesB[i] = Rnd.Range(0, 256);
			initCol[i] = Rnd.Range(0, 256);
			operators[i] = Rnd.Range(0, 6);
		}

		string[] hexconv = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
		string[] operand = { "+", "×", "÷", "⊻", "∧", "∨" };
		Debug.LogFormat("[ReGret-B Filtering #{0}] Starting with {1}.", _moduleID, initCol.Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		Debug.LogFormat("[ReGret-B Filtering #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[0]] + " " + hexconv[stagesR[0] / 16] + hexconv[stagesR[0] % 16] + hexconv[stagesG[0] / 16] + hexconv[stagesG[0] % 16] + hexconv[stagesB[0] / 16] + hexconv[stagesB[0] % 16], StageCalc(0, initCol).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		Debug.LogFormat("[ReGret-B Filtering #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[1]] + " " + hexconv[stagesR[1] / 16] + hexconv[stagesR[1] % 16] + hexconv[stagesG[1] / 16] + hexconv[stagesG[1] % 16] + hexconv[stagesB[1] / 16] + hexconv[stagesB[1] % 16], StageCalc(1, StageCalc(0, initCol)).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		Debug.LogFormat("[ReGret-B Filtering #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[2]] + " " + hexconv[stagesR[2] / 16] + hexconv[stagesR[2] % 16] + hexconv[stagesG[2] / 16] + hexconv[stagesG[2] % 16] + hexconv[stagesB[2] / 16] + hexconv[stagesB[2] % 16], StageCalc(2, StageCalc(1, StageCalc(0, initCol))).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
	}
	
	private void Stageshift ()
	{
		initCol = StageCalc(0, initCol);
		for (int i = 0; i < 2; i++)
		{
			stagesR[i] = stagesR[i + 1];
			stagesG[i] = stagesG[i + 1];
			stagesB[i] = stagesB[i + 1];
			operators[i] = operators[i + 1];
		}
		stagesR[2] = Rnd.Range(0, 256);
		stagesG[2] = Rnd.Range(0, 256);
		stagesB[2] = Rnd.Range(0, 256);
		operators[2] = Rnd.Range(0, 6);
		string[] hexconv = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
		string[] operand = { "+", "×", "÷", "⊻", "∧", "∨" };
		Debug.LogFormat("[ReGret-B Filtering #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[2]] + " " + hexconv[stagesR[2] / 16] + hexconv[stagesR[2] % 16] + hexconv[stagesG[2] / 16] + hexconv[stagesG[2] % 16] + hexconv[stagesB[2] / 16] + hexconv[stagesB[2] % 16], StageCalc(2, StageCalc(1, StageCalc(0, initCol))).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		DispUpdate();
	}

	private void DispUpdate ()
	{
		string[] hexconv = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
		string[] operand = { "+", "×", "÷", "⊻", "∧", "∨" };
		Text[0].GetComponent<TextMesh>().text = initCol.Select(x => hexconv[x / 16] + hexconv[x % 16]).Join("");
		Text[1].GetComponent<TextMesh>().text = operand[operators[selected]] + " " + hexconv[stagesR[selected] / 16] + hexconv[stagesR[selected] % 16] + hexconv[stagesG[selected] / 16] + hexconv[stagesG[selected] % 16] + hexconv[stagesB[selected] / 16] + hexconv[stagesB[selected] % 16];
		Text[2].GetComponent<TextMesh>().text = input.Take(index).Select(x => hexconv[x]).Join("");
		Text[3].GetComponent<TextMesh>().text = (selected + 1).ToString();
	}

	private int[] StageCalc(int stage, int[] colour)
	{
		switch (operators[stage])
		{
			case 0:
				return new int[3] { (stagesR[stage] + colour[0]) / 2, (stagesG[stage] + colour[1]) / 2, (stagesB[stage] + colour[2]) / 2 };
			case 1:
				return new int[3] { Sqrt(stagesR[stage] * colour[0]), Sqrt(stagesG[stage] * colour[1]), Sqrt(stagesB[stage] * colour[2]) };
			case 2:
				return new int[3] { InvAvg(new int[] { stagesR[stage], colour[0] }), InvAvg(new int[] { stagesG[stage], colour[1] }), InvAvg(new int[] { stagesB[stage], colour[2] }) };
			case 3:
				return new int[3] { (stagesR[stage] ^ colour[0]), (stagesG[stage] ^ colour[1]), (stagesB[stage] ^ colour[2]) };
			case 4:
				return new int[3] { (stagesR[stage] & colour[0]), (stagesG[stage] & colour[1]), (stagesB[stage] & colour[2]) };
			default:
				return new int[3] { (stagesR[stage] | colour[0]), (stagesG[stage] | colour[1]), (stagesB[stage] | colour[2]) };
		}
	}

	private int Sqrt(int value)
	{
		for (int i = 255; i > -1; i--)
		{
			if(i * i <= value)
			{
				return i;
			}
		}
		return 0;
	}

	private int InvAvg(int[] value)
	{
		if (value[0] * value[1] == 0)
		{
			return 0;
		}
		else
		{
			return (int)(2f / (1f / value[0] + 1f / value[1]) + 0.000000001f);
		}
	}

	private IEnumerator KeepingPace()
	{
		t = -1;
		while (!solved)
		{
			t++;
			if (t == 14)
			{
				t %= 14;
				Stageshift();
			}
			for (int i = 0; i < 13; i++)
			{
				if (12 - i < t)
				{
					Timer[i].GetComponent<MeshRenderer>().material.color = new Color32(159, 159, 192, 255);
				}
				else
				{
					Timer[i].GetComponent<MeshRenderer>().material.color = new Color32(47, 47, 47, 255);
				}
			}
			if (t != 0)
				Audio.PlaySoundAtTransform("Tick", Module.transform);
			yield return new WaitForSeconds(5f);
			while (checking)
			{
				yield return null;
			}
		}
	}

	private IEnumerator Statuslight()
    {
		Displays[38].GetComponent<MeshRenderer>().material.color = new Color(StageCalc(2, StageCalc(1, StageCalc(0, initCol)))[0] / 255f, StageCalc(2, StageCalc(1, StageCalc(0, initCol)))[1] / 255f, StageCalc(2, StageCalc(1, StageCalc(0, initCol)))[2] / 255f);
		yield return new WaitForSeconds(0.5f);
        Displays[38].GetComponent<MeshRenderer>().material.color = new Color32(159, 159, 192, 255);
	}

	private IEnumerator SolveCheck()
	{
		checking = true;
		bool[] checks = new bool[3];
		int[] answer = new int[6];
		if ((twitch) && (t < 3))
		{
			for (int i = 0; i < 6; i += 2)
			{
				answer[i] = StageCalc(1, StageCalc(0, initCol))[i / 2] / 16;
				answer[i + 1] = StageCalc(1, StageCalc(0, initCol))[i / 2] % 16;
			}
			for (int i = 0; i < 6; i += 2)
			{
				if ((input[i] == answer[i]) && (input[i + 1] == answer[i + 1]))
				{
					checks[i / 2] = true;
				}
			}
		}
		for (int i = 0; i < 6; i+=2)
		{
			answer[i] = StageCalc(2, StageCalc(1, StageCalc(0, initCol)))[i / 2] / 16;
			answer[i + 1] = StageCalc(2, StageCalc(1, StageCalc(0, initCol)))[i / 2] % 16;
		}
		for (int i = 0; i < 6; i+=2)
		{
			if ((input[i] == answer[i]) && (input[i + 1] == answer[i + 1]))
			{
				checks[i / 2] = true;
			}
		}
		for (int i = 0; i < 4; i++)
		{
			Text[i].GetComponent<TextMesh>().text = "";
		}
		for (int i = 0; i < 3; i++)
		{
			yield return new WaitForSeconds(1f);
			Audio.PlaySoundAtTransform("Tick", Module.transform);
			if (i < checks.Count(x => x))
			{
				Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(159, 159, 191, 255);
			}
			else
			{
				Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(79, 79, 95, 255);
			}
		}
		yield return new WaitForSeconds(1f);
		string[] hexconv = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
		if (checks.Count(x => x) == 3)
		{
			Debug.LogFormat("[ReGret-B Filtering #{0}] Submitted {1}, which is correct!.", _moduleID, input.Take(index).Select(x => hexconv[x]).Join(""));
			Audio.PlaySoundAtTransform("Solve", Module.transform);
			for (int i = 0; i < 60; i++)
            {
				int[] glitchy = new int[6];
				for (int j = 0; j < i / 10; j++)
				{
					glitchy[j] = input[j];
				}
				for (int j = i / 10; j < 6; j++)
                {
					glitchy[j] = Rnd.Range(0, 16);
				}
                foreach (GameObject obj in Displays)
                {
					obj.GetComponent<MeshRenderer>().material.color = new Color((glitchy[0] * 16 + glitchy[1]) / 255f, (glitchy[2] * 16 + glitchy[3]) / 255f, (glitchy[4] * 16 + glitchy[5]) / 255f);
				}
				Text[0].GetComponent<TextMesh>().text = glitchy.Select(x => hexconv[x]).Join("");
				yield return new WaitForSeconds(0.05f);
            }
			foreach (GameObject obj in Displays)
			{
				obj.GetComponent<MeshRenderer>().material.color = new Color((input[0] * 16 + input[1]) / 255f, (input[2] * 16 + input[3]) / 255f, (input[4] * 16 + input[5]) / 255f);
			}
			Text[0].GetComponent<TextMesh>().text = input.Select(x => hexconv[x]).Join("");

			if (input.Count(x => x == 0) == 6 || input.Count(x => x == 15) == 6)
			{
				Text[1].GetComponent<TextMesh>().text = "GOOD JOB";
				Text[2].GetComponent<TextMesh>().text = "IGUESS";
			}
			else if (Rnd.Range(0, 6) == 0)
			{
				Text[1].GetComponent<TextMesh>().text = "IGIVEUP!";
				Text[2].GetComponent<TextMesh>().text = "YOUWIN";
			}
			else if (Rnd.Range(0, 5) == 0)
			{
				Text[1].GetComponent<TextMesh>().text = "I AM NOW";
				Text[2].GetComponent<TextMesh>().text = "SOLVED";
			}
			else if (Rnd.Range(0, 4) == 0)
			{
				Text[1].GetComponent<TextMesh>().text = "ABSOLUTE";
				Text[2].GetComponent<TextMesh>().text = "MADLAD";
			}
			else if (Rnd.Range(0, 3) == 0)
			{
				Text[1].GetComponent<TextMesh>().text = "WEDIDIT";
				Text[2].GetComponent<TextMesh>().text = "REDDIT";
			}
			else if (Rnd.Range(0, 2) == 0)
			{
				Text[1].GetComponent<TextMesh>().text = "POGGERS!";
				Text[2].GetComponent<TextMesh>().text = "MOMENT";
			}
			else
			{
				Text[1].GetComponent<TextMesh>().text = "FRIGGIN'";
				Text[2].GetComponent<TextMesh>().text = "LEGEND";
			}
			solved = true;
			checking = false;
			Module.HandlePass();
		}
		else
		{
			Debug.LogFormat("[ReGret-B Filtering #{0}] Submitted {1}, which is incorrect!.", _moduleID, input.Take(index).Select(x => hexconv[x]).Join(""));
			Module.HandleStrike();
			StartCoroutine(Statuslight());
			t = -1;
			checking = false;
			index = 0;
			for (int i = 0; i < 3; i++)
			{
				Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(47, 47, 47, 255);
			}
			Stagegen();
			DispUpdate();
		}
	}



#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} 1' to move to stage 1. '!{0} #D42069' to insert hex code #D42069. On Twitch Plays, the system allows for submit delays of 15 seconds (2 LEDs on max).";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		twitch = true;
		command = command.ToLowerInvariant();
		string validcmds = "0123456789abcdef";
		string validstages = "123";
		if (command.Length == 1 && validstages.Contains(command[0]))
		{
            for (int i = 0; i < 3; i++)
            {
                if (command[0] == validstages[i])
                {
                    while (selected != i)
                    {
						if(i > selected)
                        {
							StageButtons[1].OnInteract();
                        }
                        else
                        {
							StageButtons[0].OnInteract();
						}
						yield return null;
					}
                }
            }
		}
		else
		{
			if (command.Length != 7 || command[0] != '#')
			{
				yield return "sendtochaterror Invalid command.";
				yield break;
			}
			for (int i = 1; i < 7; i++)
			{
				if (!validcmds.Contains(command[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			}
			for (int i = 1; i < 7; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if (validcmds[j] == command[i])
					{
						SubButtons[j].OnInteract();
						yield return null;
					}
				}
			}
			yield return "strike";
			yield return "solve";
		}
		yield return null;
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		twitch = true;
		int[] solution = new int[3];
        for (int i = 0; i < 3; i++)
        {
			solution[i] = StageCalc(2, StageCalc(1, StageCalc(0, initCol)))[i];
		}
        for (int i = 0; i < 3; i++)
        {
			SubButtons[solution[i] / 16].OnInteract();
			yield return null;
			SubButtons[solution[i] % 16].OnInteract();
			yield return null;
		}
	}
}
