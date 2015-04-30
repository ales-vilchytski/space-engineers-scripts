bool closingStarted = false;
DateTime closingStart = new DateTime();
string memoryBlock = "LCD Panel Airlock Doors Memory"; 
int doorsTransitionSeconds = 17;

void activateWarnings() {
    IMySoundBlock soundBlock = (IMySoundBlock) GridTerminalSystem.GetBlockWithName("Sound Block Airlock");
    soundBlock.GetActionWithName("PlaySound").Apply(soundBlock);
    
    ForEachBlockInGroup("Airlock Warn Blocks", delegate(IMyTerminalBlock block) {
        block.GetActionWithName("OnOff_On").Apply(block); 
    });
}

void deActivateWarnings() {
    IMySoundBlock soundBlock = (IMySoundBlock) GridTerminalSystem.GetBlockWithName("Sound Block Airlock");
    soundBlock.GetActionWithName("StopSound").Apply(soundBlock);
    
    ForEachBlockInGroup("Airlock Warn Blocks", delegate(IMyTerminalBlock block) {
        block.GetActionWithName("OnOff_Off").Apply(block); 
    });
}

void Main()     
{ 
    IMyTerminalBlock guardBlock = GridTerminalSystem.GetBlockWithName(memoryBlock);
    if (!guardBlock.IsWorking) {
        return;
    }

    IMySensorBlock hangarSensor = (IMySensorBlock) GridTerminalSystem.GetBlockWithName("Sensor Hangar Bay H Door"); 
    IMySensorBlock airlockSensor = (IMySensorBlock) GridTerminalSystem.GetBlockWithName("Sensor Airlock"); 
    IMySensorBlock cargoSensor = (IMySensorBlock) GridTerminalSystem.GetBlockWithName("Sensor Cargo Bay H Door"); 
 
    IMyBlockGroup hangarDoors = GridTerminalSystem.BlockGroups.Find(group => group.Name.Equals("Hangar Bay H Doors")); 
    IMyBlockGroup cargoDoors = GridTerminalSystem.BlockGroups.Find(group => group.Name.Equals("Cargo Bay H Doors")); 
    IMyDoor hangarDoor = (IMyDoor) hangarDoors.Blocks[0]; 
    IMyDoor cargoDoor = (IMyDoor) cargoDoors.Blocks[0]; 
     
    //Targets: { HANGAR_TO_CARGO, CARGO_TO_HANGAR, NONE } 
    //States { OPEN_HANGAR, CLOSE_HANGAR, OPEN_CARGO, CLOSE_CARGO, IDLE }; 
    string[] mem = loadState(memoryBlock, "HANGAR_TO_CARGO,OPEN_HANGAR").Split(new Char[]{','}); 
    string currentTarget = mem[0]; 
    string currentState = mem[1];
    bool doorsTransitionEnd = (DateTime.Now - closingStart).TotalSeconds > doorsTransitionSeconds; 
     
    // IMPORTANT: This algorithm doesn't process cases when there are more than 1 player/ship in airlock. 
    if (currentTarget == "HANGAR_TO_CARGO") { 
        if (hangarSensor.IsActive) { 
            currentState = "OPEN_HANGAR"; 
        } else if (airlockSensor.IsActive) { 
            if (!hangarDoor.Open && closingStarted && doorsTransitionEnd) {
                currentState = "OPEN_CARGO";
            } else {
                currentState = "CLOSE_HANGAR";
                if (!closingStarted) {
                    closingStarted = true;
                    closingStart = new DateTime();
                }
            }
        } else if (cargoSensor.IsActive) {
            currentState = "IDLE"; 
            currentTarget = "NONE";
            closingStarted = false;
        } else { 
            currentState = "IDLE";
            closingStarted = false;
        } 
    } else if (currentTarget == "CARGO_TO_HANGAR") { 
        if (cargoSensor.IsActive) { 
            currentState = "OPEN_CARGO"; 
        } else if (airlockSensor.IsActive) { 
            if (!cargoDoor.Open && closingStarted && doorsTransitionEnd) { 
                currentState = "OPEN_HANGAR";
            } else { 
                currentState = "CLOSE_CARGO";
                if (!closingStarted) {
                    closingStarted = true;
                    closingStart = new DateTime();
                }
            }
        } else if (hangarSensor.IsActive) { 
            currentState = "IDLE";
            currentTarget = "NONE";
            closingStarted = false;
        } else { 
            currentState = "IDLE";
            closingStarted = false;
        } 
    }
    
    if (currentTarget == "NONE") { 
        if (cargoSensor.IsActive) { 
            currentTarget = "CARGO_TO_HANGAR"; 
        } else if (hangarSensor.IsActive) { 
            currentTarget = "HANGAR_TO_CARGO"; 
        }
    }
    saveState(memoryBlock, currentTarget + "," + currentState);
     
    Action<IMyTerminalBlock> openDoors = delegate(IMyTerminalBlock door) {  
            ((IMyDoor) door).GetActionWithName("Open_On").Apply(door); 
        }; 
    Action<IMyTerminalBlock> closeDoors = delegate(IMyTerminalBlock door) {  
            ((IMyDoor) door).GetActionWithName("Open_Off").Apply(door); 
        }; 
     
    if (currentState == "OPEN_HANGAR") {
        ForEachBlockInGroup(hangarDoors, openDoors);
        if (currentTarget == "CARGO_TO_HANGAR") {
            activateWarnings();
        }
    } else if (currentState == "OPEN_CARGO") {
        ForEachBlockInGroup(cargoDoors, openDoors);
        if (currentTarget == "HANGAR_TO_CARGO") {
            activateWarnings();
        }
    } else if (currentState == "CLOSE_HANGAR") {
        ForEachBlockInGroup(hangarDoors, closeDoors);
        deActivateWarnings();
    } else if (currentState == "CLOSE_CARGO") {
        ForEachBlockInGroup(cargoDoors, closeDoors);
        deActivateWarnings();
    } else if (currentState == "IDLE") {
        deActivateWarnings();
    }
}   
   
///================///   
///    UTILITIES   ///   
   
void ForEachBlockInGroup(string name, Action<IMyTerminalBlock> action) {    
    IMyBlockGroup blockGroup = GridTerminalSystem.BlockGroups.Find(group =>    
        group.Name.Equals(name)    
    );    
     
    ForEachBlockInGroup(blockGroup, action);    
} 
 
void ForEachBlockInGroup(IMyBlockGroup blockGroup, Action<IMyTerminalBlock> action) { 
    blockGroup.Blocks.ForEach(action); 
} 
    
void debug(string txt) {    
    message("Debug", txt);    
}    
    
void warning(string txt) {    
    message("Warning", txt);    
}    
    
void message(string panelName, string txt) {    
    int MAX_TEXT_LENGTH = 255;    
    
    IMyTextPanel panel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(panelName); 
    if (panel != null) { 
        string currentText = panel.GetPublicText();    
        string message = (currentText.Length > MAX_TEXT_LENGTH) ? (txt) : (currentText + "\n" + txt);    
        panel.WritePublicText(message); 
    }    
}  
  
void saveState(string panelName, string value) {  
    IMyTextPanel panel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(panelName);  
    panel.WritePublicText(value);  
}  
  
string loadState(string panelName, string defVal = "") {  
    IMyTextPanel panel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(panelName);  
    string value = panel.GetPublicText();  
    if (!value.Equals("")) {  
        return panel.GetPublicText();  
    } else { 
        return defVal;  
    }  
}  
   
///================///   
