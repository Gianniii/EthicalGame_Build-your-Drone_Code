using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [SerializeField] private List<GameObject> tabs = new List<GameObject>();
    //For spawning of tab button
    public Button spawnButtonPrefab;
    public GameObject parentOfSpawn;
    private Button spawnedTabButton;

    //For spawning for tab body
    public GameObject parentOfBodySpawn; //to know where to place the tab in the hierarchy
    public GameObject spawnTabBodyPrefab;
    private GameObject spawnedTabBody; //new obj for spawned body
    //--
    //Now need to map button text to an index here...
    private string[] choiceFeedbackTexts = {"None", "Locked in 1", 
    "locked in 2", "Locked in 3", "Locked in 4", "Locked in 5", "Locked in 6"};
    private int choiceId = 0; //used to give spawnedTab a name

    //Called by the pressed button(with onClick() method), using as argument "tabBody" the body associated with the button
    public void onTabSwitch(GameObject tabBody) {
        tabBody.SetActive(true);  

        for(int i = 0; i < tabs.Count; i++) {
            if(tabs[i]!= tabBody){
                tabs[i].SetActive(false);
            }
        }
    }

    //could return the spawned tabs so we can modify them later if needed, for now just let them be.
    public void spawnTab(string buttonText, string tabContent) {
        //Spawn a button for the tab
        spawnedTabButton = GameObject.Instantiate(spawnButtonPrefab);
        spawnedTabButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        spawnedTabButton.name = "spawnedTabButton_"+ choiceId.ToString();
        spawnedTabButton.transform.SetParent(parentOfSpawn.transform, false);

        //Spawn new tab with 
        //Basically for custom content need to make a prefab, with a text field i can modify.. thats it. not soo hard.
        spawnedTabBody = GameObject.Instantiate(spawnTabBodyPrefab);
        spawnedTabBody.name = "spawnedTabBody_"+ choiceId.ToString();
        //spawnedTabBody.GetComponentInChildren<TextMeshProUGUI>();
        spawnedTabBody.transform.SetParent(parentOfBodySpawn.transform, false);
        //spawnedTabBody.GetComponentInChildren<TextMeshProUGUI>().text = tabContent; This should do it.

        //Trigger dialogue in textbox
        Dialogue dialogue = new Dialogue();
        //set dialogue text to tabContent
        string[] s = {tabContent};
        dialogue.sentences = s; //This wont work!! dialoguetext must remain stored somewhere, permanently
        //so dialogue manager must get dialogue depending on tab that is OPEN!! not tab spwaned!!
        //Sol would be to make the spwaned tab a trigger with given dialogue! and immedialty trigger without needing push again!
        //FindObjectOfType<DialogueManager>().StartDialogue(dialogue, spawnedTabBody.GetComponentInChildren<TextMeshProUGUI>());

        //Need to start the conversation in spawnedTabBody!!
        //TODO make the tab have a dialogue and call dialogueManager.StartDialogue with this dialogue!
        //last pb: need to change the textmeshprougui in dialoguemanager to text in current tab
        //FindObjectOfType<DialogueManager>().dialogue = tabContent; THIS WONT WORK, need to preserve content of ALL TABS!
        //could use an array in dialogueManager and StartDialogue takes index or name of tab!
        //FindObjectOfType<DialogueManager>().StartDialogue(dialogue);

        //Set content of new spawnedTab

        //spawnedTabBody.GetComponentInChildren<TextMeshProUGUI>().text = tabContent; //oh yeah.. obvvv

        //Modify the tab body that the spawned Button will call
        //override previous onTabSwitch call from onClick method
        //spawnedTabButton.onClick.AddListener(delegate { onTabSwitch(spawnedTabBody); }); //Need to give it ability open spawnedTabBody
        spawnedTabButton.onClick.AddListener(delegate { onTabSwitch(spawnedTabBody); });

        //Add new spawned tab to tabs array
        tabs.Add(spawnedTabBody);
        
    }
}
