// Solar panels automatic orientation
// Inspiration: http://pkula.blogspot.com/2015/01/space-engineers-programming-horizontal.html    

// Uses 3 rotors for positioning. Carefully set up minimum and maximum safe angles!
// Example of angles:
//  - Rotor1 (at panels): min -105, max 1
//  - Rotor2 (middle): min -131, max 46
//  - Rotor3 (at station): min inf, max inf

int MAX_SPEED = 1; //rpm, is rounded to 3
int MAX_ROTATIONS = 30; // maximum relative time of rotation in one direction. Decrease for faster platform/torque
int MAX_OUTPUT = 85; //kW, 70 seems to work fine

// See init() method for block names.
IMySolarPanel panelLeftTop;
IMySolarPanel panelLeftBot;
IMySolarPanel panelRightTop;
IMySolarPanel panelRightBot;
IMyMotorStator motor1; 
IMyMotorStator motor2; 
IMyMotorStator motor3; 
IMyTextPanel textPanel;
IMyTimerBlock timer;
int timerGuardCount = 0;

void Main()
{
    init();
    
    int powerOutput = -1; 
    string state = loadState("0;0;0");
    string[] states = state.Split(new char[]{';'});
    int rotations1 = int.Parse(states[0]);
    int rotations2 = int.Parse(states[1]);
    int rotations3 = int.Parse(states[2]);
    
    powerOutput = 
        (GetPanelPower(panelLeftTop) + 
        GetPanelPower(panelRightTop) + 
        GetPanelPower(panelLeftBot) + 
        GetPanelPower(panelRightBot)) /
        4;
    
    if (powerOutput >= MAX_OUTPUT) {
        motor1.GetActionWithName("ResetVelocity").Apply(motor1); 
        motor2.GetActionWithName("ResetVelocity").Apply(motor2); 
        motor3.GetActionWithName("ResetVelocity").Apply(motor3);
        if (timerGuardCount < 5) {
            timer.GetActionWithName("OnOff_Off").Apply(timer);
        } else {
            timerGuardCount++;
        }
    } else {
        timerGuardCount = 0;
        rotations1 = RotatePanel(motor1, rotations1, MAX_SPEED);
        rotations2 = RotatePanel(motor2, rotations2, MAX_SPEED);
        rotations3 = RotatePanel(motor3, rotations3, MAX_SPEED);
    }
     
    string newState = rotations1.ToString() + ";" + rotations2.ToString() + ";" + rotations3.ToString();  
    saveState(newState);
    message("\n\nAverage output:\n" + powerOutput.ToString() + "kW\n\n\nDebug state:\nBefore:" + state + "\nAfter:" + newState);   
}
    
public int GetPanelPower(IMySolarPanel panel) 
{ 
    var _d = panel.DetailedInfo;  
    string _power = _d.Split(new string[] {"\n"}, StringSplitOptions.None)[1];
    System.Text.RegularExpressions.MatchCollection match = System.Text.RegularExpressions.Regex.Matches(_power, "\\d+([\\.,]\\d+)?");
    System.Collections.IEnumerator en = match.GetEnumerator();
    en.MoveNext();
    _power = en.Current.ToString(); // MAX output  
    int _powerOutput = Convert.ToInt32(Math.Round(Convert.ToDouble(_power)));   
    return _powerOutput; 
}

public int RotatePanel(IMyMotorStator motor, int rotations, int maxSpeed) {
    if (rotations <= 0 && rotations >= -MAX_ROTATIONS) {
        RotatePanelMinus(motor, maxSpeed);
        return rotations - 1;
    } else if (rotations < -MAX_ROTATIONS) {
        RotatePanelPlus(motor, maxSpeed);
        return 1;
    } else if (rotations > 0 && rotations <= MAX_ROTATIONS) {
        RotatePanelPlus(motor, maxSpeed);
        return rotations + 1;
    } else { // rotations > MAX_ROTATIONS
        RotatePanelMinus(motor, maxSpeed);
        return -1;
    }
}

public void RotatePanelPlus(IMyMotorStator motor, int maxSpeed)
{
   if (motor.Velocity < maxSpeed)  
              motor.GetActionWithName("IncreaseVelocity").Apply(motor);
}

public void RotatePanelMinus(IMyMotorStator motor, int maxSpeed)
{
   if (motor.Velocity > -maxSpeed)
              motor.GetActionWithName("DecreaseVelocity").Apply(motor);
}

void init() {
    if (panelLeftTop == null) {  
        panelLeftTop = GridTerminalSystem.GetBlockWithName("Solar Panel Left Top") as IMySolarPanel;
    } 
    if (panelLeftBot == null) {  
        panelLeftBot = GridTerminalSystem.GetBlockWithName("Solar Panel Left Bot") as IMySolarPanel;
    } 
    if (panelRightTop == null) {  
        panelRightTop = GridTerminalSystem.GetBlockWithName("Solar Panel Right Top") as IMySolarPanel;
    } 
    if (panelRightBot == null) {  
        panelRightBot = GridTerminalSystem.GetBlockWithName("Solar Panel Right Bot") as IMySolarPanel;
    }
    if (motor1 == null) { 
        motor1 = GridTerminalSystem.GetBlockWithName("Advanced Rotor Solar 1") as IMyMotorStator; 
    } 
    if (motor2 == null) { 
        motor2 = GridTerminalSystem.GetBlockWithName("Advanced Rotor Solar 2") as IMyMotorStator; 
    } 
    if (motor3 == null) { 
        motor3 = GridTerminalSystem.GetBlockWithName("Advanced Rotor Solar 3") as IMyMotorStator; 
    }
    if (timer == null) {
        timer = GridTerminalSystem.GetBlockWithName("Timer Block Solar Mover") as IMyTimerBlock;
    }
}

///================///
///    UTILITIES   ///
void message(string txt) {    
    if (textPanel == null) { 
        textPanel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName("LCD Panel Solar"); 
    } 
        
    if (textPanel != null) {
        textPanel.WritePublicText(txt); 
    }    
}  
    
void saveState(string value) {  
    Storage = value;
}  
    
string loadState(string defVal = "") {  
    if (Storage != null && !"".Equals(Storage)) {
        return Storage;
    } else {
        return defVal;
    }
}

///================///
