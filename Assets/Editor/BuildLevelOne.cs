using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildLevelOne
{
    private static Transform geometry, scenery, gameplay;
    private static Material dark, steel, rock, cyan, amber, mint;
    private const string Destination = "Assets/Scenes/Level1.unity";

    [MenuItem("Tools/Rocket Boost/Crear Level1 - Puerto orbital")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before building the level.");
        if (File.Exists(Destination)) throw new InvalidOperationException("Level1 already exists; it has not been overwritten.");
        // Clone the configured scene objects, including overrides on the player and the barrel.
        var playerSource = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
        if (playerSource == null) throw new InvalidOperationException("Open SampleScene first.");
        GameObject player = UnityEngine.Object.Instantiate(playerSource.gameObject);
        GameObject launchSource = GameObject.FindGameObjectsWithTag("LaunchPad").FirstOrDefault();
        GameObject landingSource = GameObject.FindGameObjectsWithTag("LandingPad").FirstOrDefault();
        GameObject launch = launchSource != null ? UnityEngine.Object.Instantiate(launchSource) : null;
        GameObject landing = landingSource != null ? UnityEngine.Object.Instantiate(landingSource) : null;
        var explosiveSource = UnityEngine.Object.FindAnyObjectByType<ExplosiveContact>();
        GameObject barrel = explosiveSource != null ? UnityEngine.Object.Instantiate(explosiveSource.gameObject) : null;
        Scene source = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        SceneManager.MoveGameObjectToScene(player, scene);
        if (launch != null) SceneManager.MoveGameObjectToScene(launch, scene);
        if (landing != null) SceneManager.MoveGameObjectToScene(landing, scene);
        if (barrel != null) SceneManager.MoveGameObjectToScene(barrel, scene);
        geometry = new GameObject("01 - Route and obstacles").transform;
        scenery = new GameObject("02 - Orbital outpost scenery").transform;
        gameplay = new GameObject("03 - Enemies and supplies").transform;
        Directory.CreateDirectory("Assets/Material/Level1");
        dark = Mat("Basalt", new Color(.045f,.065f,.10f));
        steel = Mat("Station steel", new Color(.16f,.23f,.30f));
        rock = Mat("Moon rock", new Color(.28f,.32f,.40f));
        cyan = Mat("Navigation cyan", new Color(.04f,.72f,1f), true);
        amber = Mat("Hazard amber", new Color(1f,.35f,.06f), true);
        mint = Mat("Arrival mint", new Color(.12f,1f,.6f), true);
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.34f,.41f,.55f);
        RenderSettings.fog = false;
        var sun = new GameObject("Cold moonlight").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.6f;
        sun.color = new Color(.78f,.86f,1f); sun.transform.rotation = Quaternion.Euler(35,-25,0);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 9.5f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.012f,.022f,.052f);
        camera.transform.position = new Vector3(14,8,-32); camera.transform.rotation = Quaternion.identity;
        camera.farClipPlane = 150;
        var followCamera = camera.gameObject.AddComponent<LevelCamera>();
        followCamera.SetTarget(player.transform);
        player.name = "Player"; player.transform.position = new Vector3(3,3.8f,0); player.transform.rotation = Quaternion.identity;
        followCamera.SnapToTarget();
        // Broad landing surfaces sit above their supports, clear of decorative meshes.
        Pad(launch, "LaunchPad", new Vector3(3,1.1f,0), cyan);
        GameObject finish = Pad(landing, "LandingPad", new Vector3(65,1.1f,0), mint);
        finish.AddComponent<LandingRestart>();
        Box("Launch foundation", new Vector3(3,-.3f,0), new Vector3(9,1.2f,5), dark, geometry);
        Box("Arrival foundation", new Vector3(65,-.3f,0), new Vector3(10,1.2f,5), dark, geometry);
        // Alternating low and high hazards create an easy-to-read slalom.
        Obstacle("01 - Basalt ridge", new Vector3(14,1.8f,0), new Vector3(3,4.4f,4), rock);
        Obstacle("02 - Hanging station gate", new Vector3(22,14,0), new Vector3(3,7,4), steel);
        GameObject lift = Obstacle("03 - Moving cargo lift", new Vector3(28,3.5f,0), new Vector3(3.5f,.65f,3), steel);
        Motion(lift, LevelObstacleMotion.Motion.Move, new Vector3(0,4,0), 6);
        GameObject rotor = Obstacle("04 - Rotating antenna", new Vector3(36,8,0), new Vector3(7,.4f,2), steel);
        Motion(rotor, LevelObstacleMotion.Motion.Rotate, Vector3.zero, 5);
        Box("Rotor warning hub", new Vector3(36,8,-1.15f), new Vector3(.85f,.85f,.3f), amber, rotor.transform);
        GameObject pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pulse.name = "05 - Expanding reactor"; pulse.transform.SetParent(geometry);
        pulse.transform.position = new Vector3(45,5,0); pulse.transform.localScale = Vector3.one * 2;
        pulse.GetComponent<Renderer>().sharedMaterial = amber; pulse.tag = "Explosion";
        Motion(pulse, LevelObstacleMotion.Motion.Scale, Vector3.zero, 5.5f);
        Obstacle("06 - Final overhead bulkhead", new Vector3(55,14,0), new Vector3(3,6,4), steel);
        Box("Cargo shelf", new Vector3(50,1.6f,0), new Vector3(5,.6f,4), steel, geometry);
        if (barrel != null) { barrel.name = "Explosive barrel - optional hazard"; barrel.transform.SetParent(gameplay); barrel.transform.position = new Vector3(50,2.6f,0); }
        Enemy(new Vector3(19,6.5f,0), new Vector3(0,2,0));
        Enemy(new Vector3(53,7,0), new Vector3(2,0,0));
        Pickup("AmmoPowerUp", new Vector3(11,7,0));
        Pickup("HealthPowerUp", new Vector3(25,7,0));
        Pickup("AmmoPowerUp", new Vector3(40,12,0));
        Pickup("HealthPowerUp", new Vector3(58,6,0));
        // Imported rocky silhouettes and station models stay behind the gameplay plane.
        for (int i=0; i<15; i++)
        {
            float x = -5 + i*5.5f;
            Model("Assets/Sci-Fi-Assets/Models/Rock_Cliff_Env_01.fbx", new Vector3(x,-1,6+i%3), 5+i%4, rock);
        }
        Model("Assets/Sci-Fi-Assets/Models/Building_17.fbx", new Vector3(21,0,10), 11, null);
        Model("Assets/Sci-Fi-Assets/Models/Building_02.fbx", new Vector3(57,0,10), 9, null);
        Model("Assets/Sci-Fi-Assets/Models/Crate_02_2_Glow.fbx", new Vector3(8,0,5), 2, null);
        for (int i=0; i<19; i++)
        {
            Box("Route beacon", new Vector3(1+i*3.6f,.15f,-2.5f), new Vector3(.45f,.12f,.25f), cyan, scenery);
        }
        // Navigation graphics and signs are scene assets, editable from the Inspector.
        Sign("01 / DEPARTURE", new Vector3(3,5.8f,1), cyan, .12f);
        Sign("02 / REFINERY", new Vector3(29,15.7f,1), cyan, .12f);
        Sign("03 / DOCKING", new Vector3(60,15.7f,1), mint, .12f);
        Sign("LAND HERE", new Vector3(65,5,1), mint, .1f);
        Sign(">  >  >", new Vector3(8,10,2), cyan, .12f);
        Sign(">  >  >", new Vector3(47,12,2), cyan, .12f);
        // Deterministic star field, plus a large distant planet.
        var random = new System.Random(17);
        for (int i=0; i<80; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UnityEngine.Object.DestroyImmediate(star.GetComponent<Collider>());
            star.name = "Distant star"; star.transform.SetParent(scenery);
            star.transform.position = new Vector3(-12+(float)random.NextDouble()*95, 3+(float)random.NextDouble()*25, 25);
            star.transform.localScale = Vector3.one * (.035f+(float)random.NextDouble()*.065f);
            star.GetComponent<Renderer>().sharedMaterial = cyan;
        }
        GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        UnityEngine.Object.DestroyImmediate(planet.GetComponent<Collider>());
        planet.name = "Distant moon"; planet.transform.SetParent(scenery);
        planet.transform.position = new Vector3(49,16,32); planet.transform.localScale = Vector3.one*12;
        planet.GetComponent<Renderer>().sharedMaterial = rock;
        var boundary = Box("Recovery boundary", new Vector3(34,-4,0), new Vector3(90,2,14), dark, geometry);
        boundary.GetComponent<Renderer>().enabled = false; boundary.GetComponent<BoxCollider>().isTrigger = true;
        boundary.AddComponent<LevelBoundary>();
        var backWall = Box("Left safety wall",new Vector3(-3,9,0),new Vector3(1,28,8),dark,geometry);
        var farWall = Box("Right safety wall",new Vector3(72,9,0),new Vector3(1,28,8),dark,geometry);
        backWall.GetComponent<Renderer>().enabled = false; farWall.GetComponent<Renderer>().enabled = false;
        var roof = Box("Flight ceiling",new Vector3(34,21,0),new Vector3(90,1,8),dark,geometry);
        roof.GetComponent<Renderer>().enabled = false;
        GameObject hud = Spawn("Assets/Prefabs/PlayerHUD.prefab", Vector3.zero, null);
        if (hud != null)
            foreach (Text text in hud.GetComponentsInChildren<Text>(true)) text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, Destination);
        var builds = EditorBuildSettings.scenes.ToList();
        if (!builds.Any(s => s.path == Destination)) builds.Add(new EditorBuildSettingsScene(Destination,true));
        EditorBuildSettings.scenes = builds.ToArray();
        EditorSceneManager.CloseScene(source, true);
        Selection.activeGameObject = geometry.gameObject;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.in2DMode = false;
            SceneView.lastActiveSceneView.LookAt(new Vector3(34,7,0), camera.transform.rotation, 38, false, true);
        }
        Debug.Log("LEVEL1_READY: Assets/Scenes/Level1.unity");
    }

    private static Material Mat(string name, Color color, bool glow=false)
    {
        string path = "Assets/Material/Level1/"+name+".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material,path); }
        material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",.3f);
        if (glow) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor",color*1.7f); }
        return material;
    }
    private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.transform.position=position; go.transform.localScale=scale;
        go.transform.SetParent(parent,true); go.GetComponent<Renderer>().sharedMaterial=material;
        return go;
    }
    private static GameObject Obstacle(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go=Box(name,position,scale,material,geometry); go.tag="Explosion";
        GameObject stripe=Box("Hazard stripe",position+new Vector3(0,0,-scale.z/2-.025f),new Vector3(scale.x,.12f,.05f),amber,go.transform);
        UnityEngine.Object.DestroyImmediate(stripe.GetComponent<Collider>());
        return go;
    }
    private static void Motion(GameObject go, LevelObstacleMotion.Motion type, Vector3 travel, float period)
    {
        var body=go.AddComponent<Rigidbody>(); body.isKinematic=true; body.useGravity=false; body.interpolation=RigidbodyInterpolation.Interpolate;
        var motion=go.AddComponent<LevelObstacleMotion>(); motion.motion=type; motion.travel=travel; motion.period=period;
    }
    private static GameObject Spawn(string path, Vector3 position, Transform parent)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(asset==null) return null;
        var go=(GameObject)PrefabUtility.InstantiatePrefab(asset); go.transform.position=position; go.transform.SetParent(parent,true); return go;
    }
    private static GameObject Pad(GameObject existing, string tag, Vector3 position, Material trim)
    {
        GameObject go=existing;
        if(go==null) go=Box(tag,position,new Vector3(5,.5f,4),steel,geometry);
        go.name=tag; go.tag=tag; go.transform.position=position;
        go.transform.SetParent(geometry,true);
        Box(tag+" light",position+new Vector3(0,.8f,-2),new Vector3(4,.12f,.12f),trim,scenery);
        return go;
    }
    private static void Enemy(Vector3 position, Vector3 travel)
    {
        var go=Spawn("Assets/Prefabs/FloatingEnemy.prefab",position,gameplay);
        if(go==null) return;
        var movement=new SerializedObject(go.GetComponent<EnemyMovement>());
        movement.FindProperty("movement").vector3Value=travel; movement.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void Pickup(string name, Vector3 position) => Spawn("Assets/Prefabs/"+name+".prefab",position,gameplay);
    private static void Model(string path, Vector3 position, float height, Material material)
    {
        GameObject go=Spawn(path,Vector3.zero,scenery); if(go==null)return;
        foreach(var collider in go.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
        Renderer[] renderers=go.GetComponentsInChildren<Renderer>(); if(renderers.Length==0)return;
        Bounds bounds=renderers[0].bounds; foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
        float factor=height/Mathf.Max(.01f,bounds.size.y); go.transform.localScale*=factor;
        go.transform.position=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*factor;
        if(material!=null)foreach(var renderer in renderers)renderer.sharedMaterial=material;
    }
    private static void Sign(string value,Vector3 position,Material tint,float size)
    {
        var go=new GameObject(value);go.transform.SetParent(scenery);go.transform.position=position;
        var text=go.AddComponent<TextMesh>();text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize=64;text.characterSize=size;text.anchor=TextAnchor.MiddleCenter;text.color=tint.GetColor("_BaseColor");
        go.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
    }
}
