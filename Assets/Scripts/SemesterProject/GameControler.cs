using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum choices
{
	None, 
	DroneExpert, //1
    BirdExpert, //expert on specific bird (choice2)
    TestLocally, //test in backyard (choice id: 3)
    //OnSiteVisit, //nothern ireland lots of rain and WIND! in winter for example
    OnFieldTesting, //choice id: 4
    userTesting, //choice id: 5
    resevoirDirector, //choice id: 6
    shipIt, //choice id: 7
}

public class GameControler : MonoBehaviour
{
    //TODO perhaps make new script for missionStatementScene so its less cluttered
    //Intro page 
    public DialogueTrigger MissionStatementTriggerButton; //TODO try to trigger on awake using text for now try with button
    public Dialogue MissionStatementDialogue;
    public TextMeshProUGUI MissionStatementDialogueTextBox;
    //Final outcome dialogue box
    public DialogueTrigger finalDialogueTriggerButton; 
    public Dialogue finalOutcomeDialogue;
    public TextMeshProUGUI finalOutcomeDialogueTextBox;

    //Drone Specs
    private static int protoDroneSize = 20;
    private static double protoDroneWeight = 1.0;
    private static string protoDroneColor = "Blue";
    //TODO: Rename me
    private static string protoMaterial; //this is the 'skeleton' material, name of this thing tbd
    private static string protoPropellerMaterial = "Plastic";
    private static bool has_wetsuit = false;
    private static bool has_manual = false;

    private static bool has_foldable_propellers = false;
    public TextMeshProUGUI droneSpecsText;
    public static List<string> colorList = new List<string>{"white", "purple", "blue"};
    //public static int[] droneSizeRange = {20, 150}; //min and max size range
    //public static double[] droneWeightRange = {0.5, 10};

    //Main Tab Feedback text
    public DialogueTrigger EnterButton;
    public TextMeshProUGUI dialogueTextBox;
    public TextMeshProUGUI scrollBarText; //Contains text currently displayed in scrollBar

    //Array of locked choice and choice selection objects
    List<int> locked_choices = new List<int>(); //List of choices locked in by the players
    public DropSlot slot; //slot where choice is dropped into

    /* Tabs and dialogues -------------------------------------------------------*/
    //public DialogueManager dialogueManager;
    public PopUpScript popUp;
    public Button mainTab;
    private int mainTabIndexInLayout; //Used to make sure the "main" tab is always to the right of all tabs
    public TabController tabController;
    //Array of dialogues 
    [SerializeField] private List<Dialogue> choiceFeedbackDialogues;//set in unity directly
    private string[] finalOutcomeDialogueSentences = {"var 0", "var 1", 
    "var 2", "var 3", "var 4", "var 5", "var 6", "var 7", "var 8", "var 9", "var 10"};

    /*ACCEPT/REFUSE------------------------------------------------------------------*/
    public int[] numSubChoices; //maps Choices => int representing number of subchoices for this main choice. 
    //(Necessary to know when to start using accept/refuse buttons)
    public int latestChoiceId = 0;
    int acceptedSubChoiceNumber = 0; //represents the current subchoice withing the main choice
    bool accept = false; //record user's choice
    //These buttons spawn when player must make choice of accepting to refusing the proposed changes from the expert
    public Button acceptButton; 
    public Button refuseButton;

    /*Money and Time System ---------------------------------------------------------------*/
    public TextMeshProUGUI availableBalanceText;
    public TextMeshProUGUI remainingTimeText;
    static public float remainingTime = 1; //number of weeks remaning till project deadline 
    static public int availableBalance = 300; //Starting budget
    public int[] mainChoiceFinancialCosts; //Costs of each main choice, set in unity
    public float[] mainChoiceTimeCosts; //timeCosts of each main choice, set in unity
    // -----------------------------------------------------------------------------
    void Start()
    {   //Set on MissionStatement scene set up MissionStatementDialogue
        if(MissionStatementTriggerButton!= null) {
            MissionStatementTriggerButton.dialogueTextBox = MissionStatementDialogueTextBox;
            MissionStatementTriggerButton.dialogue = MissionStatementDialogue;
            MissionStatementTriggerButton.allowRestart = false;
        }

        //If on final scene setup final trigger with its dialogue
        if(finalDialogueTriggerButton!= null) {
            finalDialogueTriggerButton.dialogueTextBox = finalOutcomeDialogueTextBox;
            finalOutcomeDialogue = computeOutcomeDialogue();
            finalDialogueTriggerButton.dialogue = finalOutcomeDialogue;
            finalDialogueTriggerButton.allowRestart = false;
        }
      
        mainTabIndexInLayout = 1; 
        //Print balance and drone specs 
        remainingTimeText.text = "Time Left: \n" +  remainingTime.ToString("F1") +" Weeks"; 
        availableBalanceText.text = "Balance: " + availableBalance.ToString() +"$"; 
        refreshDroneSpecs();
    }
    
    //TODO could merge several of these outcomes "per spec" into one feedback from a researcher.
    
    private Dialogue computeOutcomeDialogue(){
        finalOutcomeDialogueSentences[0] =  "FeedbackTextVAR1.";
         
        if(protoDroneColor.Equals("Blue")) {
             finalOutcomeDialogueSentences[1] = "The color of drone is unfortunate because its color blends in with that of" + 
             " the sky, i often lose some time trying to find it in the sky.";
        } else if(protoDroneColor.Equals("White")) {
             finalOutcomeDialogueSentences[1] = "The white color of the drone is easy to spot in the sky however some birds" + 
             " have attacked the drone, maybe because white is seen as aggressive by some birds." ;    
        } else if(protoDroneColor.Equals("Purple")){
             finalOutcomeDialogueSentences[1] = "I like that you made the drone purple, most birds are not threatened "
             + "by this color and the drone remains clearly visible to the operator." ;  
        }

        if(protoDroneSize <= 30) {
            finalOutcomeDialogueSentences[2] = "The size of the drone is small and easy to carry!, however on windy days it" 
            + "its not as stable as previous drones.";
        } else if(protoDroneSize >= 30) {
            finalOutcomeDialogueSentences[2] = "The drone is pretty big and unable to fit in my bag, perhaps a carrying case " + 
            "would be usefull";
        }
        
        if(protoDroneWeight < 1.0) {
            finalOutcomeDialogueSentences[3] = "Drone was light and easy to carry, but short flying time meant it felt like a lot of " + 
            "work for the brief footage. Although we did get some really great data we couldn’t have got otherwise!\n";
        }else if(protoDroneWeight > 1.5) {
            finalOutcomeDialogueSentences[3] = "Long flying time from the big battery was great improvement from our last drone,"
        + " and the drone was stable in the wind."
         + " But overall the drone was too heavy to carry. Most of our researchers are under 160 cm, so the combined weight and size"
         + " made it very difficult to hike with it over wild terrain for 3h. We didn’t take it out very often";
        }else {
            finalOutcomeDialogueSentences[3] = "This drone is capable of flying and observing birds for about 30 minutes, "+
            "it is a slight improvement from our previous drone and the stability of the drone is about the same."; 
        }

        if(has_wetsuit) {   
            finalOutcomeDialogueSentences[4] = "The previous drone we used did not have a wet suit, so we are very satisfied" 
            + " to now we are able to conduct our bird observation even in the rough Scottish weather";
        } else {
            finalOutcomeDialogueSentences[4] = "Unfortunately that just our the previous drone we had, we are not able to use" + 
            " it under rainy conditions.";            
        }

        if(has_manual) {   
            finalOutcomeDialogueSentences[5] = "Including the drone manual was super useful, allowing new people to pick it up quickly. " 
            + "Although terminology was a bit technical, so they added in some of their own definitions to make it more accessible";
        } else {
            finalOutcomeDialogueSentences[5] = "Hard to get started using the drone. Mostly only the 2 PhD student researchers were willing to invest time getting competent, "
            + "we will see if those starting next year also will.";            
        } 

        if(protoPropellerMaterial == "Plastic"){
            finalOutcomeDialogueSentences[6] = "The plastic propellers of the drone make alot of noise, and on occasion seems to scare off" + 
            " or disturb some of the birds, however the flexibility of the propellers makes the drone less harmful in case of a collision"
            +" with a bird.";
        } else if(protoPropellerMaterial == "Carbon Fiber"){
            finalOutcomeDialogueSentences[6] = "Quiet drones appear not to bother birds at all, however the carbon fiber propellers are much harder than"+
            " plastic ones, and we need to be really careful flying it too to the birds as the propeller could seriously injur a curious or aggressive bird.";            
        } else if(protoPropellerMaterial == "Wood"){
            finalOutcomeDialogueSentences[6] = "The wooden propellers are very silent and appear not to bother birds at all, "+
            " however they are harder then plastic ones, so i am always afraid of injuring a bird who might fly to close."; 
        }
        
        if(has_foldable_propellers) {
            finalOutcomeDialogueSentences[7] = "The foldable propellers were a nice upgrade, the drone is ";
        } else {
            finalOutcomeDialogueSentences[7] = "The drone is quite big and cumbersome to carry over long distances, perhaps" +  
            " using foldable propellers would made it easier to carry in a smaller bag.";
        }     

        //TODO FIRST: AUTO-STOP if run out of cash or time....
        //at final summary (overall) eval paragraph based on all vars
        //IF RLY BAD => SCARE AWAY BIRDS WHO ABANDON EGGS.. I.E IF Heavy + carbon fiber proppellers
        Dialogue outcomeDialogue = new Dialogue();
        outcomeDialogue.sentences = finalOutcomeDialogueSentences;
        return outcomeDialogue;
    }
    
    /**
    Manages what happens any time a user lock's in a choice.
    This is triggered any time a user lock's in a choice by clicking the "enter" button.
    */
    public void lockInChoice() {
        //TODO cant lock in choice until finished previous choice.
        acceptedSubChoiceNumber = 0; //reset subChoice index
        //Get choice locked in choice id and text
        DragDrop lastChoice = slot.droppedChoice;
        latestChoiceId = lastChoice.choice_id;
        string choiceCardText = lastChoice.GetComponentInChildren<TextMeshProUGUI>().text;
        
        //reset choice card to its original location
        lastChoice.transform.position = lastChoice.original_position;

        //Get cost of choice and check if have avaible funds
        float mainChoiceTimeCost = mainChoiceTimeCosts[latestChoiceId];
        int mainChoiceFinancialCost = mainChoiceFinancialCosts[latestChoiceId];
        //If sufficient resources
        if(checkIfEnoughResources(mainChoiceTimeCost, mainChoiceFinancialCost)){
            
            locked_choices.Add(latestChoiceId); // Add to List of locked choices TODO CAN REMOVE THIS PROBABLY LATER

            tabController.spawnTab(latestChoiceId, choiceCardText, choiceFeedbackDialogues[latestChoiceId]);
            //Main Tab always displayed the to the right of all other tabs
            mainTab.transform.SetSiblingIndex(++mainTabIndexInLayout);

            //Update game paramaters and UI
            updateMainTabText(choiceFeedbackDialogues[latestChoiceId]); //update text in main tab
            EnterButton.TriggerDialogueMainTab(); //trigger dialogue in MainTab

            //update available time and balance due to locking in this main choice
            updateAvailableBalanceAndTimeForMainChoice(latestChoiceId);
        } else {
            popUp.display();
        }
        //Check if game ended, then activate final scene.
        if(latestChoiceId == (int)choices.shipIt){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+ 1);
        }
    }

    /**
    Called by DialogueManager when subChoice is accepted or refused
    */
    public void incrementSubChoiceNum() {
        acceptedSubChoiceNumber++;
    }

    ///
    /// Input: cost of currently selected subchoice
    /// Output: Boolean indicating if have enough resources
    ///
    private bool checkIfEnoughResources(float timeCost, int financialCost){
        //if not enough resources return false
        if(availableBalance < financialCost || remainingTime < timeCost){
            return false;
        }
        return true;
    }
    /**
    This method is called whenever a subChoice is accepted
    Updates drone ranges according to accepted subChoice
    Called whenever choice is accepted
    */
    public void updateDroneRangesAndResources(){
        //Debug.Log("Calls updateDronRanges");
        //Debug.Log(latestChoiceId);
        //TODO here check if availableBalance and RemaningTime is sufficient for this subchoice
        //Once we know what choice was made, need to check if have enough funs before executing changes.
        //If not enough DONT EXECUTE!! + inform user somehow that not enough funds!

        float timeCost= (float)0.0;
        int financialCost = 0;

        if(latestChoiceId == (int)choices.DroneExpert){
            if(acceptedSubChoiceNumber == 0){
                timeCost= (float)0.5;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    protoDroneColor = "White";
                } else {
                    Debug.Log("Not enough resources, drone expert choice 0");
                    popUp.display();
                    return;
                }
                
            }
            if(acceptedSubChoiceNumber == 1){
                timeCost =(float)2.0;
                financialCost = 0;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    has_manual = true;
                } else {
                    Debug.Log("Not enough resources, drone expert choice 0");
                    popUp.display();
                    return;
                }
            }
        }
        if(latestChoiceId == (int)choices.BirdExpert){
            //Bird expert ultimately suggest not to make it white
            if(acceptedSubChoiceNumber == 0) {
                timeCost = (float)0.5;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    protoDroneColor = "Purple";
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            } else if(acceptedSubChoiceNumber == 1) {
                timeCost = (float)0.5;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    protoPropellerMaterial = "Carbon Fiber";
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            }
        }
        if(latestChoiceId == (int)choices.TestLocally){
            if(acceptedSubChoiceNumber == 0) {
                timeCost = (float)2.0;
                financialCost = 100;
                if(checkIfEnoughResources(timeCost,financialCost)){
                    protoDroneWeight += 0.5;
                    protoDroneSize += 10;
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            }
        }
        if(latestChoiceId == (int)choices.OnFieldTesting){
            if(acceptedSubChoiceNumber == 0) {
                timeCost = (float)2.0;
                financialCost = 100;
                if(checkIfEnoughResources(timeCost,financialCost)){
                    has_wetsuit = true;
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            } else if(acceptedSubChoiceNumber == 1) {
                timeCost = (float)1.0;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost,financialCost)){
                    protoDroneWeight += 0.5;
                    protoDroneSize += 10;
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            }
        }

        if(latestChoiceId == (int)choices.userTesting){
            if(acceptedSubChoiceNumber == 0) { //Foldable propellers
                timeCost = (float)0.0;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    has_foldable_propellers = true;
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            } else if(acceptedSubChoiceNumber == 1) { // WHAT IS THIS?
                timeCost = (float)1.0;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost,financialCost)){

                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            }
        }

        if(latestChoiceId == (int)choices.resevoirDirector){
            if(acceptedSubChoiceNumber == 0) { //take director insight and use wood propeller
                timeCost = (float)0.0;
                financialCost = 50;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    protoPropellerMaterial = "Wood";
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            } else if(acceptedSubChoiceNumber == 1) { //switch to carbon fiber if not already?(reapeat same pro&cons as last time)
                timeCost = (float)0.0;
                financialCost = 1;
                if(checkIfEnoughResources(timeCost, financialCost)){
                    //Do what??
                } else {
                    popUp.display();
                    Debug.Log("Not enough resources, drone expert choice 0");
                    return;
                }
            }
        }
        //display updates
        updateAvailableBalanceAndTimeForSubChoices(timeCost, financialCost);
        refreshDroneSpecs();
        
    }
    //Contains logic calculate the next text to display in scrollBar
    //Each choice locked in has a according display text
    private void updateMainTabText(Dialogue new_dialogue){
        EnterButton.dialogueTextBox = this.dialogueTextBox;
        EnterButton.dialogue = new_dialogue;
    }

    //updates the interface showing teh drone specs 
    private void refreshDroneSpecs(){
        StringBuilder sb = new StringBuilder("", 400);
        sb.AppendFormat("Prototype drone specs: \n");
        sb.AppendFormat("Color: " + protoDroneColor + " \n");
        sb.AppendFormat("Weight [kg]: " + string.Format("{0:F1}", protoDroneWeight) + " \n");
        sb.AppendFormat("Size [cm]: " + string.Format("{0:F1}", protoDroneSize) + " \n");
        sb.AppendFormat("Material: " + protoMaterial + " \n");
        sb.AppendFormat("Propeller Material: " + protoPropellerMaterial + "\n");
        sb.AppendFormat("-----Extra Features----- \n");
        if(has_wetsuit){
            sb.AppendFormat("Wet suit available\n"); 
        } 
        if(has_manual){
            sb.AppendFormat("Drone Manual available\n"); 
        } 
        if(has_foldable_propellers){
            sb.AppendFormat("Foldable Propellers\n"); 
        } 
        droneSpecsText.text = sb.ToString();
    }

    //Update available time and balance for subchoices
    private void updateAvailableBalanceAndTimeForSubChoices(float timeCost, int financialCost) {
        remainingTime-= timeCost;
        availableBalance -= financialCost;
        remainingTimeText.text = "Time Left: \n" + remainingTime.ToString("F1") +" Weeks"; 
        availableBalanceText.text = "Balance: " + availableBalance.ToString() +"$"; 
        
    }
    //Update available balance and time for main choices
    private void updateAvailableBalanceAndTimeForMainChoice(int locked_choice_id) {
        Debug.Log("availableBalance choice" + locked_choice_id.ToString());
        Debug.Log("cost of choice" + mainChoiceFinancialCosts[locked_choice_id].ToString());
        remainingTime-= mainChoiceTimeCosts[locked_choice_id];
        availableBalance -= mainChoiceFinancialCosts[locked_choice_id];
        remainingTimeText.text = "Time Left: \n" + remainingTime.ToString("F1") +" Weeks"; 
        availableBalanceText.text = "Balance: " + availableBalance.ToString() +"$"; 
        
    }
}
