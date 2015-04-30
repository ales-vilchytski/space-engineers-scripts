void Main()  
{      
    int MAX_BLOCKS = 8 + 1; 
 
    IMyTextPanel panel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName("Programmable Block 1 Panel");  
    int current = 0; 
    Int32.TryParse(panel.GetPublicText(), out current); 
    if (current == 0) current = 1; 
 
    for (int i = 1; i < current; ++i) { 
        IMyFunctionalBlock merge = (IMyFunctionalBlock) GridTerminalSystem.GetBlockWithName("Merge Block " + i.ToString()); 
 
        if (merge != null) { 
            merge.GetActionWithName("OnOff_On").Apply(merge); 
        } 
    } 
     
     
    IMyFunctionalBlock merge1 = (IMyFunctionalBlock) GridTerminalSystem.GetBlockWithName("Merge Block " + current.ToString()); 
    if (merge1 != null) { 
        merge1.GetActionWithName("OnOff_Off").Apply(merge1); 
        ++current; 
    } else { 
        current = 0; 
    } 
     
    if (current >= MAX_BLOCKS) { 
        current = 0; 
    } 
     
    panel.WritePublicText(current.ToString());  
}