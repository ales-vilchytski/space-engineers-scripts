void Main()  
{  
    String[] sensors1Names = new String[] { "Timer Block 1_2m", "Timer Block 1_4m", "Timer Block 1_6m", "Timer Block 1_8m", "Timer Block 10m" };  
    String[] sensors2Names = new String[] { "Timer Block 2_2m", "Timer Block 2_4m", "Timer Block 2_6m", "Timer Block 2_8m", "Timer Block 10m" };  
     
    int MAX_VAL = 100500; 
    int meters1 = MAX_VAL; 
    bool sensor1enabled = false; 
    for (int i = 0; i < sensors1Names.Length; ++i) {  
        bool enabled = ((IMyFunctionalBlock) GridTerminalSystem.GetBlockWithName(sensors1Names[i])).Enabled;  
        int m = Convert.ToInt32(enabled) * (i + 1) * 2; 
        if (m != 0 && m < meters1) { 
            meters1 = m; 
            sensor1enabled = true; 
        } 
    } 
    if (!sensor1enabled) { 
        meters1 = 0; 
    } 
     
    int meters2 = MAX_VAL; 
    bool sensor2enabled = false; 
    for (int i = 0; i < sensors2Names.Length; ++i) {  
        bool enabled = ((IMyFunctionalBlock) GridTerminalSystem.GetBlockWithName(sensors2Names[i])).Enabled;  
        int m = Convert.ToInt32(enabled) * (i + 1) * 2; 
        if (m != 0 && m < meters2) { 
            meters2 = m; 
            sensor2enabled = true; 
        } 
    } 
    if (!sensor2enabled) { 
        meters2 = 0; 
    } 
      
    IMyPistonBase piston1 = (IMyPistonBase) GridTerminalSystem.GetBlockWithName("Piston 1");  
    IMyPistonBase piston2 = (IMyPistonBase) GridTerminalSystem.GetBlockWithName("Piston 2"); 
    
    IMyMotorStator rotorGrinders = (IMyMotorStator) GridTerminalSystem.GetBlockWithName("Rotor Grinder Gear");  
    
    
    while(meters1 - piston1.MaxLimit > 0.5F) {
        piston1.GetActionWithName("IncreaseUpperLimit").Apply(piston1); 
    } 
    while(meters2 - piston2.MaxLimit > 0.5F) {
        piston2.GetActionWithName("IncreaseUpperLimit").Apply(piston2); 
    }
    if (meters1 - piston1.MaxLimit <= 1.0F || meters2 - piston2.MaxLimit <= 1.0F) {
        rotorGrinders.GetActionWithName("OnOff_On").Apply(rotorGrinders);
    } else {
        rotorGrinders.GetActionWithName("OnOff_Off").Apply(rotorGrinders);
    }
}