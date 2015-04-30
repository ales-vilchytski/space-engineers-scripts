Dictionary<string, string> sensorsAndDoors = new Dictionary<string, string>()
{
    { "Sensor Cargo Bay Door", "Cargo Bay Ways H Doors" },
    { "Sensor Elevator 1 lvl Door", "Elevator 1 lvl H Doors" },
    { "Sensor Assemblies Door", "Assemblies H Door" },
    { "Sensor Refineries Door", "Refineries H Door" },
    { "Sensor Tech Room Door", "Door Tech Room 1" },
    { "Sensor External Ways", "Door External Ways" },
    { "Sensor External Tech Room", "Door External Tech Room" },
    { "Sensor Hangar External Ways", "Door Hangar External Ways" },
    { "Sensor Cargo External", "Door Cargo External" },
    { "Sensor External Cargo", "Door External Cargo" }
};

string memoryBlock = "LCD Panel Doors Memory";

void Main()     
{ 
    IMyTerminalBlock guardBlock = GridTerminalSystem.GetBlockWithName(memoryBlock);
    bool shouldWork = guardBlock.IsWorking;
    
    if (!shouldWork) {
        return;
    }
    
    Action<IMyTerminalBlock> openDoors = delegate(IMyTerminalBlock door) {  
            ((IMyDoor) door).GetActionWithName("Open_On").Apply(door); 
        }; 
    Action<IMyTerminalBlock> closeDoors = delegate(IMyTerminalBlock door) {  
            ((IMyDoor) door).GetActionWithName("Open_Off").Apply(door); 
        }; 
    
    IEnumerator<string> sensorNames = sensorsAndDoors.Keys.GetEnumerator();
    while (sensorNames.MoveNext()) {
        string sensorName = sensorNames.Current;
        string doorName = sensorsAndDoors[sensorName];
    
        IMySensorBlock doorSensor = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(sensorName); 
        IMyBlockGroup doors = GridTerminalSystem.BlockGroups.Find(group => group.Name.Equals(doorName));
        IMyDoor door = (IMyDoor) GridTerminalSystem.GetBlockWithName(doorName);
        
        if (doorSensor.IsActive) {
            if (doors == null) {
                openDoors(door);
            } else {
                ForEachBlockInGroup(doors, openDoors);
            }
        } else {
            if (doors == null) {
                closeDoors(door);
            } else {
                ForEachBlockInGroup(doors, closeDoors);
            }
        }
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
