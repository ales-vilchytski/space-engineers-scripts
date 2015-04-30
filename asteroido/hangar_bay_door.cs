String[] sensorNames = new String[] {   
    "Sensor Hangar Bay Door Out LEFT",    
    "Sensor Hangar Bay Door Out RIGHT",    
    "Sensor Hangar Bay Door In LEFT",    
    "Sensor Hangar Bay Door In RIGHT"    
};  
  
String[] groupsNamesForActiveStates = new String[]{    
    "Hangar Bay Door Spotlight Rotors",   
    "Hangar Bay Door Spotlights"   
}; 
 
string memoryBlock = "LCD Panel Hangar Bay Door Memory"; 
  
string pistonsGroupName = "Hangar Bay Door Pistons";  
int doorsTransitionSeconds = 16; //Approximate time of door closing/opening  
DateTime pistonsStartTime = new DateTime();  
  
void Main()    
{  
    IMyTerminalBlock guardBlock = GridTerminalSystem.GetBlockWithName(memoryBlock);
    bool shouldWork = guardBlock.IsWorking;

    //States { OPENING, CLOSING, IDLE }; 
    String currentState = loadState(memoryBlock, "IDLE"); 
    List<IMySensorBlock> sensors = new List<IMySensorBlock>();   
       
    for (int i = 0; i < sensorNames.Length; ++i) {   
        IMySensorBlock sensor =(IMySensorBlock) GridTerminalSystem.GetBlockWithName(sensorNames[i]);   
        if (sensor != null) {   
            sensors.Add(sensor);    
        } else {   
            warning("HBAY_DOOR: No sensor '" + sensorNames[i] + "'");   
        }   
    }  
      
    // Determine current state  
    bool somethingInRange = false;   
    for (int i = 0; i < sensors.Count; ++i) {   
        somethingInRange = somethingInRange || sensors[i].IsActive;   
    }
    bool doorsTransitionEnd = (DateTime.Now - pistonsStartTime).TotalSeconds > doorsTransitionSeconds; 
      
    if (somethingInRange) {  
        currentState = "OPENING"; 
    } else if (currentState != "OPENING" && doorsTransitionEnd) {  
        currentState = "IDLE";  
    } else if (currentState != "CLOSING") {  
        pistonsStartTime = DateTime.Now;   
        currentState = "CLOSING"; 
    } 
    saveState(memoryBlock, currentState); 
      
    // Toggle blocks for active states  
    if ((currentState == "OPENING" || currentState == "CLOSING") && shouldWork) {  
        for (int i = 0; i < groupsNamesForActiveStates.Length; ++i) {   
            ForEachBlockInGroup(groupsNamesForActiveStates[i], delegate(IMyTerminalBlock block) {   
                block.GetActionWithName("OnOff_On").Apply(block);   
            });  
        }
    } else {   
        for (int i = 0; i < groupsNamesForActiveStates.Length; ++i) {   
            ForEachBlockInGroup(groupsNamesForActiveStates[i], delegate(IMyTerminalBlock block) {   
                block.GetActionWithName("OnOff_Off").Apply(block);   
            });  
        }  
    }  
      
    // Toggle pistons  
    if (currentState == "OPENING" && shouldWork) {  
        ForEachBlockInGroup(pistonsGroupName, delegate(IMyTerminalBlock block) {  
            IMyPistonBase piston = block as IMyPistonBase;  
            while (piston.Velocity > -0.7) {  
                piston.GetActionWithName("DecreaseVelocity").Apply(block);  
            }  
        });  
    } else if (currentState == "CLOSING" && shouldWork) {  
        ForEachBlockInGroup(pistonsGroupName, delegate(IMyTerminalBlock block) {  
            IMyPistonBase piston = block as IMyPistonBase;  
            while (piston.Velocity < 0.7) {  
                piston.GetActionWithName("IncreaseVelocity").Apply(block);  
            }  
        });  
    }  
      
}  
  
///================///  
///    UTILITIES   ///  
  
void ForEachBlockInGroup(string name, Action<IMyTerminalBlock> action) {   
    IMyBlockGroup blockGroup = GridTerminalSystem.BlockGroups.Find(group =>   
        group.Name.Equals(name)   
    );   
       
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
