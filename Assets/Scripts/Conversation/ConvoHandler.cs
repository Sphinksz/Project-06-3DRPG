using System;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Conversation
{
    public class ConvoHandler : MonoBehaviour
    {
	    private GameObject _player;
	    [SerializeField] private List<string> storyVariables;
	    [SerializeField] private Canvas conversationCanvas;
	    [SerializeField] private Canvas choicesCanvas;
	    [SerializeField] private Canvas convoCanvas;
	    [SerializeField] private GameObject convoPrefab;
	    [SerializeField] private Text textPrefab;
	    [SerializeField] private Button buttonPrefab;
	    [SerializeField] private AudioSource audioController;
	    [SerializeField] private GameObject questInProgressFlag;
	    [SerializeField] private GameObject questAvailableFlag;
	    [SerializeField] private Story _story;
	    [SerializeField] private bool hasQuest;

	    private readonly List<GameObject> spawnedMarkers = new();
	    private GameObject objectToSpawn;
	    public static event Action<Story> OnCreateStory;

	    /* Observev variable
	     * _inkStory.ObserveVariable ("health", (string varName, object newValue) => {
    SetHealthInUI((int)newValue);
});
	     */
	    
	    // EXTERNAL FUNCTIONS FOR INKY
	    
	    
	    private void PlaySound(string audioName)
	    {
		    if (!audioController) return;
		    var audioClip = Resources.Load<AudioClip>(audioName);
		    audioController.PlayOneShot(audioClip);
	    }

	    private void GiveItem(string itemName, int count)
	    {
		    
	    }
	    
	    private void RemoveMarkers()
	    {
		    for (var i = spawnedMarkers.Count - 1; i >= 0; i--)
		    {
			    Destroy(spawnedMarkers[i]);
		    }
	    }

	    private void RemoveMarker(string markerName)
	    {
		    var markerToDelete = spawnedMarkers.Find(x => x.name == markerName);
		    if (markerToDelete == null) return;
		    var markerToDestroy = markerToDelete.gameObject;
		    spawnedMarkers.Remove(markerToDelete);
		    Destroy(markerToDestroy);
	    }
	    
	    private void PlaceMarker(string marker)
	    {
		    RemoveMarkers();
		    var spawnPosition = transform.position + new Vector3(0, 3.0f, 0);
		    var objName = "";
		    switch (marker)
		    {
			    case "questInProgressMarker": objectToSpawn = questInProgressFlag;
				    objName = "questInProgressMarker"; break;
			    case "questAvailableMarker": objectToSpawn = questAvailableFlag; 
				    objName = "questAvailableMarker"; break;
			    default: break;
		    }
		    if (!objectToSpawn) return;
		    var spawnedFlag = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
		    spawnedFlag.transform.SetParent(gameObject.GetComponentInChildren<Canvas>().transform);
		    spawnedFlag.name = objName;
		    spawnedMarkers.Add(spawnedFlag);
	    }

	    private void BindEvents()
	    {
		    _story.BindExternalFunction("playSound", (string audioName) => PlaySound(audioName));
		    _story.BindExternalFunction("placeMarker", (string flagType) => PlaceMarker(flagType));
		    _story.BindExternalFunction("removeMarkers", RemoveMarkers);
		    _story.BindExternalFunction("giveItem", (string flagType, int count) => GiveItem(flagType, count));
		    _story.BindExternalFunction("closeConvo", CloseConvoScreen);
		    _story.BindExternalFunction("addQuestLogText", (string questText) => AddToQuestLog(questText));
	    }

	    // END EXTERNALS
	    
		public Story CreateStory(TextAsset jsonStory, GameObject npc)
		{
			_story = new Story(jsonStory.text);
			OnCreateStory?.Invoke(_story);
			SetNpcName();
			BindEvents();
			return _story;
		}
		
		public void StartCharacterDialog(string knot)
		{
			if (_story == null) return;
			_story.ChoosePathString(knot);
			RefreshView();
		}

		private void HandleMarkdown(TextMeshProUGUI objectText, List<string> tags)
		{
			if (_story == null) return;
			if (tags == null) return;
			foreach (var tag in tags)
			{
				if (tag == "action")
				{
					objectText.fontStyle = FontStyles.Italic;
				}
			}
		}

		public void AddToQuestLog(string questText)
		{
			if (_story == null) return;
			//Player.instance.AddQuestToLog("kill", questText, _story, "0/10", "rat",10, "killRats");
			//public void AddQuestToLog(string type, string quest, Story pStory, string pProgText, string target, int killCount, string questName)
			//Player.instance.AddQuestToLog(questText, story, "");
		}
		
		private void RefreshView () {
			// Remove all the UI on screen
			RemoveChildren();
			// Read all the content until we can't continue anymore
			while (_story.canContinue) {
				// Continue gets the next line of the story
				var text = _story.Continue ();
				// This removes any white space from the text.
				text = text.Trim();
				// Display the text on screen!
				if (text.Length != 0)
				{
					var tags = _story.currentTags;
					foreach (var tag in tags)
					{
						Debug.Log(tag);
					}
					CreateContentView(text);
				}
			}

			// Display all the choices, if there are any!
			if(_story.currentChoices.Count > 0)
			{
				foreach (var choice in _story.currentChoices)
				{
					var tags = choice.tags;
					var button = CreateChoiceView (choice.text.Trim (), tags);
					var choice1 = choice;
					button.onClick.AddListener (delegate {
						OnClickChoiceButton (choice1);
					});
				}
			}
			// If we've read all the content and there's no choices, the story is finished!
			else {
				var choice = CreateChoiceView("End Conversation", new List<string>());
				choice.onClick.AddListener(RemoveChildren);
				choice.onClick.AddListener(CloseConvoScreen);
			}
		}

		public void CloseConvoScreen()
		{
			conversationCanvas.gameObject.SetActive(false);
		}
		
		private void OnClickChoiceButton (Choice choice) {
			_story.ChooseChoiceIndex (choice.index);
			_story.Continue();
			RefreshView();
		}
		
		private void CreateContentView (string text) {
			var convoObject = Instantiate (convoPrefab, convoCanvas.transform, false);
			var storyText = convoObject.GetComponent<TextMeshProUGUI>();
			storyText.text = text;
			HandleMarkdown(storyText, _story.currentTags);
		}
		
		private Button CreateChoiceView (string text, List<string> tags) {

			var choice = Instantiate (buttonPrefab, choicesCanvas.transform, false);
			var choiceText = choice.GetComponentInChildren<TextMeshProUGUI> ();
			choiceText.text = text;
			HandleMarkdown(choiceText, tags);
			return choice;
		}
		
		private void RemoveChildren () {
			var choiceschildCount = choicesCanvas.transform.childCount;
			var convochildCount = convoCanvas.transform.childCount;
			for (var i = choiceschildCount - 1; i >= 0; --i)
			{
				var child = choicesCanvas.transform.GetChild(i).gameObject;
				Destroy(child);
			}
			
			for (var i = convochildCount - 1; i >= 0; --i)
			{
				var child = convoCanvas.transform.GetChild(i).gameObject;
				Destroy(child);
			}
		}

		public void SetNpcName()
		{
			var textObj = gameObject.GetComponentInChildren<TextMeshProUGUI>();
			textObj.text = GetNpcName().ToString();
		}

		public void CheckForQuest()
		{
			hasQuest = (bool) GetVariablesState("hasQuest");
			if (hasQuest) { PlaceMarker("questAvailableMarker"); }
		}
		
		public object GetNpcName()
		{
			return GetVariablesState("name");
		}
		
		private void SetStoryVariables()
		{
			foreach (var x in _story.variablesState)
			{
				storyVariables.Add(x);
			}
		}
		
		public void SetInkStoryVariable(string variable, object value, bool log = true )
		{
			if (_story == null || !_story.variablesState.GlobalVariableExistsWithName(variable))
			{
				return;
			}

			if( log )
			{
				Debug.Log( $"[Ink] Set variable: {variable} = {value}" );
			}

			_story.variablesState[variable] = value;

		}
		
		public object GetVariablesState(string variable)
		{
			return _story.variablesState[variable];
		}
	}
}