using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class rgbRelayScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMSelectable[] SubButtons;
	public KMSelectable[] StageButtons;
	public GameObject[] Text;
	public GameObject[] Leds;
	public GameObject[] Timer;
	public GameObject[] Displays;
	public GameObject FakedStatusLight, EntireModule;
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
	int stagesCompleted = 0;

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
						if (selected < operators.Length - 1)
						{
							selected++;
						}
						break;
					default:
						break;
				}
				index = 0;
				for (int x = 0; x < 6; x++)
				{
					input[x] = 0;
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

	int[] IdxAncilleryGameObjects = { 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 38 },
        idxPossibleOperators = { 0, 1, 2, 3, 4, 5 },
		idxEasyOperators = { 3, 4, 5 };
	int[][] givenStageValues = new int[3][];
	void Start()
    {
		idxPossibleOperators.Shuffle();
		var pos = 0;
		for (int x = 0; x < 3; x++)
		{
			int[] attachedOperator = new int[x + 1];
			for (int y = 0; y <= x; y++)
			{
				attachedOperator[y] = idxPossibleOperators[pos];
				pos++;
			}
			givenStageValues[x] = attachedOperator.ToArray();
		}
		Stagegen();
		foreach (int idx in IdxAncilleryGameObjects)
			Displays[idx].GetComponent<MeshRenderer>().enabled = false;
		EntireModule.transform.Rotate(0, new[] { 90, 180, -90 }.PickRandom(), 0);
	}
	
	private void ActivateModule ()
	{
		Audio.PlaySoundAtTransform("Solve", Module.transform);
		for (int i = 0; i < 3; i++)
		{
			Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(47, 47, 47, 255);
		}
		//StartCoroutine(KeepingPace());
		DispUpdate();
	}

	private void Stagegen ()
	{
		stagesR = new int[stagesCompleted + 1];
		stagesG = new int[stagesCompleted + 1];
		stagesB = new int[stagesCompleted + 1];
		if (stagesCompleted < 1)
		{
			initCol = new int[3];
            for (int x = 0; x < 3; x++)
            {
				initCol[x] = Rnd.Range(0, 256);
			}
		}
		operators = new int[stagesCompleted + 1];

		for (int i = 0; i <= stagesCompleted; i++)
		{
			stagesR[i] = Rnd.Range(0, 256);
			stagesG[i] = Rnd.Range(0, 256);
			stagesB[i] = Rnd.Range(0, 256);
            operators[i] = givenStageValues[stagesCompleted][i];
		}

		string[] hexconv = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
		string[] operand = { "+", "×", "÷", "⊻", "∧", "∨" };
		Debug.LogFormat("[ReGrettaBle Relay #{0}] Stage {1}:", _moduleID, stagesCompleted + 1);
		Debug.LogFormat("[ReGrettaBle Relay #{0}] Starting with {1}.", _moduleID, initCol.Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		int[] stageResult = initCol.ToArray();
        for (int a = 0; a <= stagesCompleted; a++)
        {
			stageResult = StageCalc(a, stageResult);
			Debug.LogFormat("[ReGrettaBle Relay #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[a]] + " " + hexconv[stagesR[a] / 16] + hexconv[stagesR[a] % 16] + hexconv[stagesG[a] / 16] + hexconv[stagesG[a] % 16] + hexconv[stagesB[a] / 16] + hexconv[stagesB[a] % 16], stageResult.Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		}
		/*
		Debug.LogFormat("[ReGrettaBle Relay #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[0]] + " " + hexconv[stagesR[0] / 16] + hexconv[stagesR[0] % 16] + hexconv[stagesG[0] / 16] + hexconv[stagesG[0] % 16] + hexconv[stagesB[0] / 16] + hexconv[stagesB[0] % 16], StageCalc(0, initCol).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		Debug.LogFormat("[ReGrettaBle Relay #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[1]] + " " + hexconv[stagesR[1] / 16] + hexconv[stagesR[1] % 16] + hexconv[stagesG[1] / 16] + hexconv[stagesG[1] % 16] + hexconv[stagesB[1] / 16] + hexconv[stagesB[1] % 16], StageCalc(1, StageCalc(0, initCol)).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		Debug.LogFormat("[ReGrettaBle Relay #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[2]] + " " + hexconv[stagesR[2] / 16] + hexconv[stagesR[2] % 16] + hexconv[stagesG[2] / 16] + hexconv[stagesG[2] % 16] + hexconv[stagesB[2] / 16] + hexconv[stagesB[2] % 16], StageCalc(2, StageCalc(1, StageCalc(0, initCol))).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
		*/
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
		Debug.LogFormat("[ReGrettaBle Relay #{0}] Applied {1}, resulting in {2}.", _moduleID, operand[operators[2]] + " " + hexconv[stagesR[2] / 16] + hexconv[stagesR[2] % 16] + hexconv[stagesG[2] / 16] + hexconv[stagesG[2] % 16] + hexconv[stagesB[2] / 16] + hexconv[stagesB[2] % 16], StageCalc(2, StageCalc(1, StageCalc(0, initCol))).Select(x => hexconv[x / 16] + hexconv[x % 16]).Join(""));
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
		bool[] checks = new bool[stagesCompleted + 1];
		int[] answer = new int[6];
		int sectionsEach = 6 / (stagesCompleted + 1);
		/*
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
		}*/
		int[] finalResult = initCol.ToArray();
		for (int x = 0; x <= stagesCompleted; x++)
		{
			finalResult = StageCalc(x, finalResult);
		}
		for (int i = 0; i < 6; i += 2)
		{
			answer[i] = finalResult[i / 2] / 16;
			answer[i + 1] = finalResult[i / 2] % 16;
		}
        for (int i = 0; i < checks.Length; i++)
		{
            bool[] partCorrect = new bool[sectionsEach];
            for (int x = 0; x < partCorrect.Length; x++)
            {
				if (input[x + sectionsEach * i] == answer[x + sectionsEach * i])
                    partCorrect[x] = true;
            }
			checks[i] = partCorrect.ToList().TrueForAll(a => a);
		}
		for (int i = 0; i < 4; i++)
		{
			Text[i].GetComponent<TextMesh>().text = "";
		}
		for (int i = 0; i < stagesCompleted + 1; i++)
		{
			yield return new WaitForSeconds(0.5f);
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
		if (input.SequenceEqual(answer))
		{
			Debug.LogFormat("[ReGrettaBle Relay #{0}] Submitted {1}, which is correct!", _moduleID, input.Take(index).Select(x => hexconv[x]).Join(""));
			Audio.PlaySoundAtTransform("Solve", Module.transform);
			stagesCompleted++;
			if (stagesCompleted < 3)
			{
				Debug.LogFormat("[ReGrettaBle Relay #{0}] However the module wants more.", _moduleID);
				checking = false;
				index = 0;
				initCol = finalResult.ToArray();
				selected = 0;
				Stagegen();
				DispUpdate();
				for (int i = 0; i < 3; i++)
				{
					Leds[i].GetComponent<MeshRenderer>().material.color = i < stagesCompleted ? new Color32(47, 255, 47, 255) : new Color32(47, 47, 47, 255);
				}
				yield return new WaitForSeconds(0.5f);
				for (int i = 0; i < 3; i++)
				{
					Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(47, 47, 47, 255);
				}
				yield break;
			}
			Debug.LogFormat("[ReGrettaBle Relay #{0}] 3 stages completed! Module disarmed!", _moduleID);
			KMBombInfo bombInfo = gameObject.GetComponent<KMBombInfo>();
			if (bombInfo != null && bombInfo.GetTime() < 60f)
				Module.HandlePass();
			Vector3 lastRotation = EntireModule.transform.localEulerAngles * 1;
			for (int i = 0; i < 60; i++)
            {
				FakedStatusLight.SetActive(i % 4 != 3);
				foreach (int idx in IdxAncilleryGameObjects)
					Displays[idx].GetComponent<MeshRenderer>().enabled = i % 4 == 3;
				EntireModule.transform.localEulerAngles = i % 4 != 3 ? lastRotation : Vector3.zero;
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
			FakedStatusLight.SetActive(false);
			foreach (GameObject obj in Displays)
			{
				obj.GetComponent<MeshRenderer>().material.color = new Color((input[0] * 16 + input[1]) / 255f, (input[2] * 16 + input[3]) / 255f, (input[4] * 16 + input[5]) / 255f);
			}
			Text[0].GetComponent<TextMesh>().text = input.Select(x => hexconv[x]).Join("");

			Dictionary<string, string> disarmResponses = new Dictionary<string, string>() {
				{ "NO SWEAT","NEEDED" },
				{ "DISARMED","FORNOW" },
				{ "YOU'RE","WINNER" },
				{ "CONGRATS","MASTER" },
				{ "HE IS A","MASTER" },
				{ "FRIGGIN'","LEGEND" },
				{ "LET'S","GO!" },
			};

			if (input.Count(x => x == 0) == 6 || input.Count(x => x == 15) == 6)
			{
				Text[1].GetComponent<TextMesh>().text = "GOOD JOB";
				Text[2].GetComponent<TextMesh>().text = "IGUESS";
			}
			else
			{
				if (bombInfo != null && bombInfo.GetModuleIDs().Contains("regretbFiltering"))
				{
					Text[1].GetComponent<TextMesh>().text = "BROTHER";
					Text[2].GetComponent<TextMesh>().text = "HELPME";
				}
				else
				{
					KeyValuePair<string, string> randomDisarmResponse = disarmResponses.PickRandom();

					Text[1].GetComponent<TextMesh>().text = randomDisarmResponse.Key;
					Text[2].GetComponent<TextMesh>().text = randomDisarmResponse.Value;
				}
			}
			solved = true;
			checking = false;
			Module.HandlePass();
		}
		else
		{
			Debug.LogFormat("[ReGrettaBle Relay #{0}] Submitted {1}, which is incorrect!.", _moduleID, input.Take(index).Select(x => hexconv[x]).Join(""));
			Module.HandleStrike();
			//StartCoroutine(Statuslight());
			t = -1;
			checking = false;
			index = 0;
			for (int i = 0; i < 3; i++)
			{
				Leds[i].GetComponent<MeshRenderer>().material.color = new Color32(47, 47, 47, 255);
			}
			DispUpdate();
		}
	}



#pragma warning disable 414
	private string TwitchHelpMessage = "\"!{0} 1\" to move to stage 1. \"!{0} #D42069\" to insert hex code #D42069.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		twitch = true;
		command = command.ToLowerInvariant();
		string validcmds = "0123456789abcdef";
		string validstages = "123".Substring(0, stagesCompleted + 1);
		if (command.Length == 1 && validstages.Contains(command[0]))
		{
            for (int i = 0; i < stagesCompleted + 1; i++)
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
		while (stagesCompleted < 3 && !solved)
		{
			while (checking)
			{
				if (stagesCompleted >= 3) yield break;
				yield return true;
			}
			if (solved)
				yield break;
			int[] finalResult = initCol.ToArray();
			for (int x = 0; x <= stagesCompleted; x++)
			{
				finalResult = StageCalc(x, finalResult);
			}
			for (int i = 0; i < 3; i++)
			{
				SubButtons[finalResult[i] / 16].OnInteract();
				yield return null;
				SubButtons[finalResult[i] % 16].OnInteract();
				yield return null;
			}
			yield return true;
		}
	}
}
