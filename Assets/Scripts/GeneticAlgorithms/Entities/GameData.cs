using UnityEngine;

public class GameData: Object
{
    public int redirections_joystick;
    public int jumps;
    public int redirections_player;
    public float completedTime;
    public bool completed;

    public GameData()
    {
        
    }

    public GameData(int redirectionsJoystick, int jumps, int redirectionsPlayer,
        float completedTime, bool completed)
    {
        redirections_joystick = redirectionsJoystick;
        this.jumps = jumps;
        redirections_player = redirectionsPlayer;
        this.completedTime = completedTime;
        this.completed = completed;
    }
}
