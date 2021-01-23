#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Explosive))]
public class ExplosiveUI : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Explode"))
        {
            if (Application.isPlaying)
            {
                Explosive expl = (Explosive)target;
                expl.Explode();
            }
        }
    }
}
#endif