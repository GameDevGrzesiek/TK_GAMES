using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utilities
{
    public enum SceneDropMode
    {
        None,
        OnlyLast,
        TillInit
    }

    public enum GlobalGameState
    {
        None,    
        Starting,
        Loading, 
        MainMenu 
    }

    public enum PauseMenuChoice
    {
        Resume,
        Restart,
        ExitToMenu
    }
}
