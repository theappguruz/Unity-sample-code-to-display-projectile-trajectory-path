// #define ENABLE_UNLOAD_MANAGER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class tk2dEditorSpriteDataUnloader : MonoBehaviour {

	public static void Register(tk2dSpriteCollectionData data) {
#if ENABLE_UNLOAD_MANAGER && UNITY_EDITOR
		Inst.RegisterImpl(data);
#endif
	}

	public static void Unregister(tk2dSpriteCollectionData data) {
#if ENABLE_UNLOAD_MANAGER && UNITY_EDITOR
		Inst.RegisterImpl(data);
#endif
	}

#if ENABLE_UNLOAD_MANAGER && UNITY_EDITOR

	[System.Serializable]
	class TransientData {
		public TransientData( tk2dSpriteCollectionData data ) {
			name = data.spriteCollectionName;
			dataWeakRef = new System.WeakReference(data);
			if (data.needMaterialInstance) {
				createdMaterials = data.materialInsts;
				createdTextures = data.textureInsts;
			}
		}

		public string name = "";
		public tk2dSpriteCollectionData dataRef = null;
		public System.WeakReference dataWeakRef = null;
		public Texture2D[] createdTextures = new Texture2D[0];
		public Material[] createdMaterials = new Material[0];
		public void Purge() {
			foreach (Material m in createdMaterials) {
				if (m != null) {
					Object.DestroyImmediate(m);
				}
			}
			foreach (Texture2D t in createdTextures) {
				if (t != null) {
					Object.DestroyImmediate(t);
				}
			}

			createdMaterials = new Material[0];
			createdTextures = new Texture2D[0];
		}
	}

	static tk2dEditorSpriteDataUnloader _inst = null;	
	static tk2dEditorSpriteDataUnloader Inst {
		get {
			if (_inst == null) {
				tk2dEditorSpriteDataUnloader[] allInsts = Resources.FindObjectsOfTypeAll(typeof(tk2dEditorSpriteDataUnloader)) as tk2dEditorSpriteDataUnloader[];
				_inst = (allInsts.Length > 0) ? allInsts[0] : null;
				if (_inst == null) {
					GameObject go = new GameObject("@tk2dEditorSpriteDataUnloader");
					go.hideFlags = HideFlags.HideAndDontSave;
					_inst = go.AddComponent<tk2dEditorSpriteDataUnloader>();
				}
			}
			return _inst;
		}
	}

	[SerializeField] List<TransientData> transientData = new List<TransientData>();
	[SerializeField] int watching = 0;

	void OnEnable() {
		for (int i = 0; i < transientData.Count; ++i) {
			if (transientData[i].dataRef != null) {
				transientData[i].dataWeakRef = new System.WeakReference( transientData[i].dataRef );
				transientData[i].dataRef = null;
			}
		}

		UnityEditor.EditorApplication.update += EditorUpdate;
	}

	void OnDisable() {
		for (int i = 0; i < transientData.Count; ++i) {
			if (transientData[i].dataWeakRef != null && transientData[i].dataWeakRef.IsAlive) {
				transientData[i].dataRef = transientData[i].dataWeakRef.Target as tk2dSpriteCollectionData;
				transientData[i].dataWeakRef = null;
			}
		}

		UnityEditor.EditorApplication.update -= EditorUpdate;
	}

	void RegisterImpl(tk2dSpriteCollectionData data) {
		for (int i = 0; i < transientData.Count; ++i) {
			if (transientData[i].dataWeakRef != null && transientData[i].dataWeakRef.IsAlive && transientData[i].dataWeakRef.Target == data) {
				Debug.Log("tk2dEditorSpriteDataUnloader.Register: Already in list");
				return;
			}
		}
		transientData.Add(new TransientData(data));
	}

	void UnregisterImpl(tk2dSpriteCollectionData data) {
		for (int i = 0; i < transientData.Count; ++i) {
			if (transientData[i].dataWeakRef != null && transientData[i].dataWeakRef.IsAlive && transientData[i].dataWeakRef.Target == data) {
				transientData.RemoveAt(i);
				return;
			}
		}
		Debug.Log("tk2dEditorSpriteDataUnloader.Register: Not already registered");
	}

	public int materialsInFlight = 0; 

	void EditorUpdate() {
		int unloadedKeys = 0;

		for (int i = 0; i < transientData.Count; ++i) {
			if (transientData[i].dataWeakRef == null) {
				if (transientData[i] != null) {
					transientData[i].Purge();
				}
				transientData.RemoveAt(i);
				--i;
				unloadedKeys++;
			}
			else if (!transientData[i].dataWeakRef.IsAlive) {
				if (transientData[i] != null) {
					transientData[i].Purge();
				}
				transientData.RemoveAt(i);
				--i;
				unloadedKeys++;
			}
		}

		// if (unloadedKeys > 0) {
		// 	Debug.Log("Unloaded " + unloadedKeys + " sprite collections");
		// }

		watching = transientData.Count;
		materialsInFlight = (Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[]).Length;
	}

#endif
}
