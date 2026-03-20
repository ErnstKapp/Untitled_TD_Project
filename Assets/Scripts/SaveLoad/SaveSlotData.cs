using System;
using System.Collections.Generic;

[Serializable]
public class SaveSlotData
{
    public int version = 1;
    public int metaCurrency;
    public string lastCompletedLevel;
    public List<string> completedScenes = new List<string>();
}

