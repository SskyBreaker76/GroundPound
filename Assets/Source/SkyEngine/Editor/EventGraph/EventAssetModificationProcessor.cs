using UnityEditor;

public class EventAssetModificationProcessor : AssetModificationProcessor
{
    public static string[] OnWillSaveAssets(string[] Paths)
    {
        if (EventTreeEditor.ActiveEditor && EventTreeEditor.ActiveEditor.hasUnsavedChanges)
        {
            EventTreeEditor.ActiveEditor.SaveChanges();
        }

        return Paths;
    }
}
