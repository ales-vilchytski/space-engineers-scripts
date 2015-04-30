void Main() 
{     
    IMyTextPanel panel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName("Hangar Banner Time"); 
  
    panel.WritePublicText("  " + DateTime.Now.ToString("HH:mm")); 
     
} 