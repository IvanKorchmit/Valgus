#if(UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MenuNavigation))]
public class MenuHierarchyFixer : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Force create indices"))
        {
            MenuNavigation menu = (MenuNavigation)target;
            menu.SetIndex();
        }
    }
}
#endif