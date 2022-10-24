using UnityEngine;
using UnityEditor;

using Niantic.ARDK.Configuration.Authentication;

#if (UNITY_EDITOR)
namespace Niantic.ARDK.Templates
{
  public class LightshipHelperWindow : EditorWindow
  {
    private string _lightshipKey = "";

    public static void ShowHelperWindow()
    {
      GetWindow<LightshipHelperWindow>("Configuration Helper");
    }

    void OnGUI()
    {
      GUILayout.Label("Setup Lightship", EditorStyles.boldLabel);

      ArdkAuthConfig auth = AssetDatabase.LoadAssetAtPath<ArdkAuthConfig>("Assets/LightShipHUB/Resources/ARDK/ArdkAuthConfig.asset");
      SerializedObject sObject = new SerializedObject(auth);
      SerializedProperty sProperty = sObject.FindProperty("_apiKey");

      _lightshipKey = EditorGUILayout.TextField("API Key", _lightshipKey);
      GUILayout.Label("Current API Key: " + sProperty.stringValue, EditorStyles.label);

      if (GUILayout.Button("Setup"))
      {
        LightshipCommon.Setup();

        if (!_lightshipKey.Equals(""))
        {
          sProperty.stringValue = _lightshipKey;
          sObject.ApplyModifiedProperties();
          EditorUtility.DisplayDialog("Lightship", "Lightship has been set correctly", "Ok");
        }
        else
        {
          EditorUtility.DisplayDialog("Lightship", "Insert a valid API Key and try again", "Ok");
        }
      }
    }
  }
}
#endif
