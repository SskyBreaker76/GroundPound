using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class BackupWizard : Editor
{
    private static string ToolPath => $"{new DirectoryInfo(Application.dataPath).Parent.FullName}\\BackupTool\\UnityBackupUtility.exe";

    [MenuItem("SkyEngine/Begin Backup System")]
    public static void StartBackup()
    {
        if (Process.GetProcessesByName("UnityBackupUtility").Length > 0)
        {
            EditorUtility.DisplayDialog("SkyEngine", "Backup System is already running!", "Okay");
        }
        else
        {
            Process.Start(ToolPath, $"Application.dataPath false true E:\\{Application.productName.Replace(" ", "_")}_BACKUP\\");
        }
    }

    [MenuItem("SkyEngine/Backup Now")]
    public static void InstantBackup()
    {
        if (Process.GetProcessesByName("UnityBackupUtility").Length > 0)
        {
            EditorUtility.DisplayDialog("SkyEngine", "Backup System is already running!", "Okay");
        }
        else
        {
            Process.Start(ToolPath, $"{Application.dataPath} true true E:\\{Application.productName.Replace(" ", "_")}_BACKUP\\");
        }
    }
}
