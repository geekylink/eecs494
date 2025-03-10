﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

public class SelectionScreen : MonoBehaviour {
	public static SelectionScreen S;

	struct PlayerSelection{
		public GameObject playerObj;
		public SpriteRenderer playerBody;
		public Color playerColor;
	}


	public GameObject playerSprite;
	public GameObject colorWheelSprite;

	public float selectionTimeToSwitch;
	//float selectionTimer = 0;
	List<PlayerSelection> players = new List<PlayerSelection>();
	List<GameObject> colorWheels = new List<GameObject>();

	List<GameObject> selectionCircles = new List<GameObject>();
	public GameObject selectCirclePrefab;

	public List<Color> possibleColorSelections;
	public UnityEngine.UI.Text topText;
	List<bool> hasPushedA = new List<bool>();

	public List<UnityEngine.UI.Text> readyTexts = new List<UnityEngine.UI.Text>();

	bool canDoStuff = false;
	// Use this for initialization
	void Start () {
		S = this;
	}

	public void StartSelection(){
		if(canDoStuff) return;
		topText.text = "Press A to Confirm";
		StartCoroutine(WaitTillEndOfFrame());
	}

	IEnumerator WaitTillEndOfFrame(){
		yield return new WaitForEndOfFrame();

		for(int i = 0; i < InputManager.Devices.Count; ++i){
			PlayerSelection temp = new PlayerSelection();
			GameObject tempObj = Instantiate(playerSprite) as GameObject;

			//Vector3 pos = new Vector2(-7, 0) + new Vector2(5f * i, 0);
			Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 8 + Screen.width / 4 * i, Screen.height / 2));
			pos.z = 0;

			tempObj.transform.position = pos;
			
			temp.playerObj = tempObj;
			temp.playerBody = tempObj.transform.FindChild("body").GetComponent<SpriteRenderer>();
			
			players.Add (temp);
			GameManager.S.playerColors.Add (Color.red);

			GameObject cw = Instantiate(colorWheelSprite) as GameObject;
			cw.transform.position = tempObj.transform.position;
			colorWheels.Add (cw);

			GameObject sc= Instantiate(selectCirclePrefab) as GameObject;
			selectionCircles.Add (sc);
			sc.transform.position = cw.transform.position + Vector3.up * 2.08f;

			temp.playerBody.color = Color.red;
			GameManager.S.playerColors[i] = Color.red * 2;

			bool tempBool = false;
			hasPushedA.Add (tempBool);

			Vector2 oldPos = readyTexts[i].rectTransform.anchoredPosition;
			oldPos.x = -Screen.width / 2 + Screen.width / 8 + Screen.width/4 * i;
			oldPos.y = Screen.height / 6;
			readyTexts[i].rectTransform.anchoredPosition = oldPos;
		}
		canDoStuff = true;
	}

	void Awake(){
	}

	void Continue(){
		Application.LoadLevel("dom-dev 1");
	}
	
	// Update is called once per frame
	void Update () {
		if(!canDoStuff) return;
		bool allHavePushedA = true;
		for(int i = 0; i < InputManager.Devices.Count; ++i){
			if(InputManager.Devices[i].Action1.WasPressed && i < hasPushedA.Count){
				hasPushedA[i] = !hasPushedA[i];
				if(hasPushedA[i]) {
					bool shouldBeReady = true;
					for(int j = 0; j < InputManager.Devices.Count; ++j){
						if(i == j) continue;
						if(GameManager.S.playerColors[i] == GameManager.S.playerColors[j] && hasPushedA[j]) {
							shouldBeReady = false;
						}
					}
					if(shouldBeReady){
						readyTexts[i].text = "READY";
					}
					else{
						hasPushedA[i] = false;
					}
				}
				else readyTexts[i].text = "";
			}

			if(i < hasPushedA.Count){
				if(!hasPushedA[i])
					allHavePushedA = false;
				else
					continue;
			}


			//move the selection thing
			if(selectionCircles.Count == 0) continue;
			float rightX = InputManager.Devices[i].RightStickX;
			float rightY = InputManager.Devices[i].RightStickY;
			
			float leftX = InputManager.Devices[i].LeftStickX;
			float leftY = InputManager.Devices[i].LeftStickY;

			float angleFloat = Mathf.Atan2 (rightY, rightX)*Mathf.Rad2Deg;
			if(angleFloat < 0) angleFloat += 360;
			angleFloat = Mathf.Round(angleFloat / 30) * 30;

			Vector2 angleVec = new Vector2(rightX, rightY);

			if(angleVec.magnitude < .9f) continue;
			angleVec = (Vector2)(Quaternion.AngleAxis(angleFloat, Vector3.forward) * Vector2.right);

			Color color;
			float r, g, b;
			
			//super awesome maths to choose the color based on angle
			angleFloat -= 90;
			if(angleFloat > 180) angleFloat -= 360;
			r = 1 - (Mathf.Abs (angleFloat) / 120);
			if(r < 0) r = 0;
			
			angleFloat += 120;
			if(angleFloat > 180) angleFloat -= 360;
			g = 1 - (Mathf.Abs (angleFloat) / 120);
			if(g < 0) g = 0;
			
			angleFloat += 120;
			if(angleFloat > 180) angleFloat -= 360;
			b = 1 - (Mathf.Abs (angleFloat) / 120);
			if(b < 0) b = 0;
			
			
			color = new Color(r, g, b);
			color = color * 2;

			bool shouldContinue = false;
			for(int j = 0; j < InputManager.Devices.Count; ++j){
				if(i == j) continue;
				if(color == GameManager.S.playerColors[j] && hasPushedA[j]){
					shouldContinue = true;
				}
			}
			if(shouldContinue) continue;
			
			players[i].playerBody.color = color;
			GameManager.S.playerColors[i] = color;

			Vector3 pos = colorWheels[i].transform.position + (Vector3)angleVec * 2.08f;
			selectionCircles[i].transform.position = pos;

			Vector3 dir = pos - colorWheels[i].transform.position;
			selectionCircles[i].transform.up = dir;




		}

		if(allHavePushedA){
			topText.text = "Press Start to Continue";
			topText.color = Color.green;
			
			if(InputManager.ActiveDevice.MenuWasPressed){
				Continue ();
			}
		}
		else{
			topText.text = "Press A to Confirm";
			topText.color = Color.white;
		}
	}

	void ChangeColor(int playerIndex, int dir){
		int currColorIndex = possibleColorSelections.IndexOf(players[playerIndex].playerBody.color);
		int newColorIndex = currColorIndex + dir;
		if(newColorIndex >= possibleColorSelections.Count) newColorIndex = 0;
		if(newColorIndex < 0) newColorIndex = possibleColorSelections.Count - 1;

		Color newColor = possibleColorSelections[newColorIndex];
		players[playerIndex].playerBody.color = newColor;

		GameManager.S.playerColors[playerIndex] = newColor;
	}
}
