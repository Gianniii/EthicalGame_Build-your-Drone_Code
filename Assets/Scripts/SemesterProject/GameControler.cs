using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//TODO

/*
4)Work on Story and make interface much nicer. Thats it..
5)Writte report and document code
*/

//General ideas: 
/*
    Most birds dont care about color, except birds of pray will attack
    Could include video of birds attack drone or something during testing! or just a picture
    multi-colored reflective tape/ or lights discourage bird attacks, aswell as loud sounds(or maybe cat?xd)
    //Googly eyes seem to work aswell..juju
*/

//TODO use the choices enum instead of ints
public enum choices
{
	None, 
	DroneExpert,
    BirdExpert, //expert on specific bird
    OnSiteVisit, //nothern ireland lots of rain and WIND! in winter for example
    OnFieldTesting, 
    macroTesting,
    shipIt,
}
public class GameControler : MonoBehaviour
{
    public int budget = 10000; 
    public TextMeshProUGUI droneSpecsText;
    public static List<string> colorList = new List<string>{"white", "purple", "blue"};
    public static List<string> materialList = new List<string>{"carbon fiber", "mat2", "mat3"};
    public static int[] droneSizeRange = {20, 150}; //min and max size range
    public static int[] droneWeightRange = {500, 5000};
    //public int droneDevelopmentCost = 0;
    //public int droneManufacturingCost = 0;
    //public int droneProductionCost = 0;
    //public int dronePrice = 0;
    public TextMeshProUGUI scrollBarText; //Contains text currently displayed in scrollBar
    List<int> locked_choices = new List<int>(); //List of choices locked in by the players
    public int choice_index = 0; //how many choices have been made
    public DropSlot slot; //slot where choice is dropped into

    /* Tabs and dialogues -------------------------------------------------------*/
    public Button mainTab;
    private int mainTabIndexInLayout;
    public TabController tabController;
    //Array of dialogues 
    [SerializeField] private List<Dialogue> choiceFeedbackDialogues;//set in unity directly
    
    //TODO modify main tab to use dialogues also!!!
    private string[] choiceFeedbackTexts = {"None", "Locked in 1", 
    "locked in 2", "Locked in 3", "Locked in 4", "Locked in 5", "Locked in 6"};

    /*Money System ---------------------------------------------------------------*/
    public TextMeshProUGUI availableBalanceText;
    static public int availableBalance = 10000; //set in unity and updated through code
    public int[] costs; //Costs of each choice, set in unity

    // -----------------------------------------------------------------------------
    void Start()
    {   
        mainTabIndexInLayout = 1;
        availableBalanceText.text = "Balance: " + availableBalance.ToString() +"$"; 
        refreshDroneSpecs();
    }
    
    public void lockInChoice() {
        //reset choice to its original location
        DragDrop lastChoice = slot.droppedChoice;
        int locked_choice_id = lastChoice.choice_id;
        string choiceText = lastChoice.GetComponentInChildren<TextMeshProUGUI>().text;
        
        //reset choice card to its original location
        lastChoice.transform.position = lastChoice.original_position;

        locked_choices.Add(locked_choice_id); // Add to List of locked choices 
        tabController.spawnTab(locked_choice_id, choiceText, choiceFeedbackDialogues[locked_choice_id]);
        debug_print_list_content();

        //Main Tab always displayed the to the right of all other tabs
        mainTab.transform.SetSiblingIndex(++mainTabIndexInLayout);

        //Update game paramaters and UI
        updateScrollBarText(locked_choice_id); //update text in main tab
        updateDroneRanges(locked_choice_id);
        refreshDroneSpecs();
        updateAvailableBalance(locked_choice_id);

        //Check if game ended, then activate final scene.
        if(locked_choice_id == (int)choices.shipIt){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+ 1);
        }
    }
    //Contains all game loics
    //current ranges: Color/Material, Weight/Size/ Camera
    private void updateDroneRanges(int choice_id){
        //TODO will need to code logic for different "orderings" aswell.... defo need to brainstorm this.
        if(choice_id == (int)choices.BirdExpert){
            //Bird expert ultimately suggest not to make it white
            colorList.RemoveAt(0); // 0 white color index
        }
        if(choice_id == (int)choices.DroneExpert){
            colorList.RemoveAt(2); // 0 blue color index
        }
        if(choice_id == (int)choices.OnSiteVisit){
            droneSizeRange[0] = 30;
            droneSizeRange[1] = 60;
        }
        if(choice_id == (int)choices.OnFieldTesting){
            
        }

    }

    private void updateAvailableBalance(int locked_choice_id) {
        Debug.Log("availableBalance choice" + locked_choice_id.ToString());
        Debug.Log("cost of choice" + costs[locked_choice_id].ToString());
        availableBalance -= costs[locked_choice_id];
        availableBalanceText.text = "Balance: " + availableBalance.ToString() +"$"; 
        //put a END button with no cost.
    }

    //Prints list contents for debugging
    private void debug_print_list_content() {
        string result = "List contents: ";
        foreach (var item in locked_choices)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result);
    }

    //Contains logic calculate the next text to display in scrollBar
    //Each choice locked in has a according display text
    private void updateScrollBarText(int locked_choice){
        Debug.Log(locked_choice);
        scrollBarText.text = choiceFeedbackTexts[locked_choice];
    }

    //updates the interface showing teh drone specs 
    private void refreshDroneSpecs(){
        StringBuilder sb = new StringBuilder("", 400);
        sb.AppendFormat("Potential Drone specs: \n");
        
        //COLOR RANGE
        sb.AppendFormat("Color: [");
        for(int i = 0; i < colorList.Count; i++) {
            sb.AppendFormat(colorList[i]);
            if(i < colorList.Count -1) {
                sb.AppendFormat(", ");
            }
        }
        sb.AppendFormat("]\n");

        //SIZE RANGE
        sb.AppendFormat("Size [cm]: [" + droneSizeRange[0].ToString() + " " + droneSizeRange[1].ToString() + "]\n");
        sb.AppendFormat("Weight [g]: [" + droneWeightRange[0].ToString() + " " + droneWeightRange[1].ToString() + "]\n");
        sb.AppendFormat("Material: ");
        for(int i = 0; i < materialList.Count; i++) {
            sb.AppendFormat(materialList[i]);
            if(i < materialList.Count -1) {
                sb.AppendFormat(", ");
            }
        }
        sb.AppendFormat("]\n");

        /*sb.AppendFormat("Weight :{1}\nSize: {2}\nMaterial: {3}\nPrice: {4}\n" + 
            "Development Cost: {5}\nManufacturing Cost: {6}\nProduction Cost: {7}", droneColor, 
            droneWeight, droneSize, droneMaterial, dronePrice.ToString(),
            droneDevelopmentCost.ToString(), droneManufacturingCost.ToString(), droneProductionCost.ToString());*/

        droneSpecsText.text = sb.ToString();
    }
}
