using UnityEditor;

public static class SceneCamHelper
{
    [MenuItem(itemName: "Edit/Camera/SetPivot")]
    private static void SetCam()
    {
        SceneView.lastActiveSceneView.FrameSelected();
        SceneView.lastActiveSceneView.size = 2.5f;
    }
}