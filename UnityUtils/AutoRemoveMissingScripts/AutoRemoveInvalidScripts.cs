using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 自动移除项目中的无效脚本
/// 功能分为两部分：
/// 1.移除项目Asset中的所有Prefab上的无效脚本
/// 2.移除当前打开的场景内的所有对象(包含隐藏对象)上的无效脚本
/// </summary>
public class AutoRemoveInvalidScripts
{
	[MenuItem("XMYDeTools/CleanUp/PrefabInvalidScripts")]
	private static void CleanUpPrefabsInvalidScirpts()
	{
		//获取Asset内的所有的预设体
		var allPrefabObjsGuid = AssetDatabase.FindAssets("t:Prefab");
		List<GameObject> allPrefabsObjs = new List<GameObject>();
		for(int i = 0;i < allPrefabObjsGuid.Length;i++)
		{
			allPrefabsObjs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(allPrefabObjsGuid[i])));
		}
		Debug.Log("Total Preab Count: " + allPrefabsObjs.Count);

		CleanUpPrefabMissingScripts(allPrefabsObjs.ToArray());
	}
	
	[MenuItem("XMYDeTools/CleanUp/SceneInvalidScripts")]
	private static void CleanUpSceneInvalidScripts()
	{
		var allGameObjects = GetAllObjectsInScene();
		Debug.Log("Total GameObject Count: " + allGameObjects.Count);
		CleanUpSceneMissingScripts(allGameObjects.ToArray());
	}

	/// <summary>
	/// 获取场景中的所有对象(包含显示的和隐藏的)
	/// </summary>
	/// <returns></returns>
	private static List<GameObject> GetAllObjectsInScene()
	{
		List<GameObject> objectsInScene = new List<GameObject>();

		Scene scene = SceneManager.GetActiveScene();
		var rootObjects = scene.GetRootGameObjects();

		for(int i = 0; i <rootObjects.Length; i++)
		{
			var transforms = rootObjects[i].GetComponentsInChildren<Transform>(true);
			for(int j = 0; j < transforms.Length; j++)
			{
				objectsInScene.Add(transforms[j].gameObject);
			}
		}

		return objectsInScene;
	}

	private static void CleanUpSceneMissingScripts(GameObject[] gameObjects)
	{
		for(int i = 0; i < gameObjects.Length; i++)
		{
			var go = gameObjects[i];
			var components = go.GetComponents<Component>();
			bool hasInvalidComp = false;

			if(components.Length == 0) continue;

			//判断是否有无效的components
			for (int j = 0; j < components.Length; j++)
			{
				if(components[j] == null)
				{
					hasInvalidComp = true;
					break;
				}
			}

			if(hasInvalidComp)
			{
				var serializedObject = new SerializedObject(go);
				var prop = serializedObject.FindProperty("m_Component");

				int r = 0;
				
				for(int k = 0; k < components.Length; k++)
				{
					if(components[k] == null)
					{
						Debug.Log(string.Format("{0} - Clean Invalid Script", go.name));
						prop.DeleteArrayElementAtIndex(k -r);
						r++;
					}
				}

				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(go);
			}
		}

		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		EditorSceneManager.SaveOpenScenes();
	}

	private static void CleanUpPrefabMissingScripts(GameObject[] gameObjects)
	{	
		for(int i = 0; i < gameObjects.Length; i++)
		{
			var go = gameObjects[i];
			var components = go.GetComponents<Component>();
			bool hasInvalidComp = false;

			if(components.Length == 0) continue;

			//判断是否有无效的components
			for (int j = 0; j < components.Length; j++)
			{
				if(components[j] == null)
				{
					hasInvalidComp = true;
					break;
				}
			}

			if(hasInvalidComp)
			{
				var gameObject = PrefabUtility.InstantiatePrefab(go) as GameObject;
				var serializedObject = new SerializedObject(gameObject);
				var prop = serializedObject.FindProperty("m_Component");

				int r = 0;
				
				for(int k = 0; k < components.Length; k++)
				{
					if(components[k] == null)
					{
						Debug.Log(string.Format("{0} - Clean Invalid Script", gameObject.name));
						prop.DeleteArrayElementAtIndex(k -r);
						r++;
					}
				}

				serializedObject.ApplyModifiedProperties();
				PrefabUtility.ReplacePrefab(gameObject, go);
				//PrefabUtility.CreatePrefab(AssetDatabase.GetAssetPath(go), gameObject);
				GameObject.DestroyImmediate(gameObject);
			}
		}
	}
}
