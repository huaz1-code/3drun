using UnityEditor;
using UnityEditor.SceneManagement;

public class AddScenesToBuildSettings
{
    [MenuItem("Tools/Add Scenes to Build Settings")]
    public static void AddScenes()
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var buildScene = new EditorBuildSettingsScene(path, true);
            buildScenes.Add(buildScene);
            UnityEngine.Debug.Log($"Added scene: {path}");
        }
        
        EditorBuildSettings.scenes = buildScenes.ToArray();
        UnityEngine.Debug.Log($"Successfully added {buildScenes.Count} scenes to Build Settings!");
    }
}
