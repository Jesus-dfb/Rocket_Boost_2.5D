using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class LevelPerspective
{
    [MenuItem("Tools/Rocket Boost/Ajustar vista 2.5D")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) return;
        LevelCamera follow = Object.FindAnyObjectByType<LevelCamera>();
        if (follow == null) return;
        Undo.RecordObject(follow.transform, "Perspective framing");
        Undo.RecordObject(follow.GetComponent<Camera>(), "Perspective camera");
        follow.SnapToTarget();
        Camera camera = follow.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.43f,.49f,.59f);
        RenderSettings.ambientEquatorColor = new Color(.25f,.29f,.35f);
        RenderSettings.ambientGroundColor = new Color(.12f,.14f,.18f);
        foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.type != LightType.Directional) continue;
            Undo.RecordObject(light, "Dimensional lighting");
            light.transform.rotation = Quaternion.Euler(45,-35,0);
            light.intensity = 1.35f;
            light.color = new Color(1f,.94f,.84f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .8f;
        }
        foreach (TextMesh text in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(text.transform, "Readable world signs");
            text.transform.rotation = follow.transform.rotation;
        }
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            view.in2DMode = false;
            view.LookAt(new Vector3(34,6,0), follow.transform.rotation, 40, false, true);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("LEVEL1_PERSPECTIVE_READY");
    }
}
