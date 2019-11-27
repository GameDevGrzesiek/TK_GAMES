using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySettingsManager : Singleton<MySettingsManager>
{
    [Header("CommonScenes")]
    public static readonly string Init = "Init";
    public static readonly string MainMenu = "MainMenu";
}
