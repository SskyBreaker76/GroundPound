using SkySoft.IO;
using SkySoft.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkySoft.Steam;
using SkySoft;
using System;

public class FileWidget : ObjectEventHandler
{
    [Header("File Widget")]
    public bool IsFileValid = true;

    [Range(0, FileManager.MaxSaves - 1)] public int Target;
    public Text SlotIndex, LeftFileInfo, RightFileInfo;

    protected override void OnValidate()
    {
        Refresh();
        base.OnValidate();
    }

    public void Refresh()
    {
        SlotIndex.text = (Target).ToString("00");

        if (FileManager.SaveExists(Target))
        {
            IsFileValid = true;
            FileManager.ReadFromArchive<PlayerFile>(Target, "Entities", $"_Player_Local", Value =>
            {
                TimeSpan FileAge = TimeSpan.FromSeconds(Value.SaveAge);
                LeftFileInfo.text = $"{Value.Properties.Name} ( {SkyEngine.CommonTexts["stat.level"]} {Value.Level} )<size=12>\n{SkyEngine.Levels.GetDisplayName(Value.CurrentArea)}\n{FileAge.Hours.ToString("000")}:{FileAge.Minutes.ToString("00")}:{FileAge.Seconds.ToString("00")}</size>";
                RightFileInfo.text = $"{Value.Currency.Copper.Value} {(SkyEngine.CommonTexts.ContainsKey("item.crowns") ? SkyEngine.CommonTexts["item.crowns"] : "Crowns")}";
            }, ".entity");
        }
        else
        {
            IsFileValid = false;
            LeftFileInfo.text = $"<color=grey>{SkyEngine.CommonTexts["system.nofile"]}<size=12>\n\n---:--:--</size></color>";
            RightFileInfo.text = "";
        }
    }
}
