using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using System.Linq;

public class ConfigureXR
{
    [MenuItem("Tools/Configure XR for Meta Quest")]
    public static void ConfigureProjectForMetaQuest()
    {
        Debug.Log("Starting XR configuration for Meta Quest...");

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching build target to Android...");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("Failed to switch build target to Android.");
                return;
            }
            Debug.Log("Successfully switched build target to Android.");
        }
        else
        {
            Debug.Log("Build target is already Android.");
        }

        var androidBuildTargetGroup = BuildTargetGroup.Android;
        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(androidBuildTargetGroup);

        if (generalSettings == null)
        {
            Debug.LogError($"Failed to get XRGeneralSettings for BuildTargetGroup '{androidBuildTargetGroup}'. Creating new settings.");
             XRGeneralSettingsPerBuildTarget.CreateDefaultBuildTargetSettings(androidBuildTargetGroup, BuildTarget.Android, typeof(XRGeneralSettings));
             generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(androidBuildTargetGroup);
             if (generalSettings == null) {
                 Debug.LogError("Failed to create or get XRGeneralSettings after attempt. Aborting.");
                 return;
             }
        }

        var xrManagerSettings = generalSettings.AssignedSettings;
        if (xrManagerSettings == null)
        {
            Debug.Log("XRManagerSettings not assigned. Assigning default...");
            xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            generalSettings.AssignedSettings = xrManagerSettings;
            EditorUtility.SetDirty(generalSettings);
        }

        var openXrLoaderType = typeof(OpenXRLoader);
        bool isOpenXrLoaderAssigned = xrManagerSettings.activeLoaders.Any(loader => loader.GetType() == openXrLoaderType);

        if (!isOpenXrLoaderAssigned)
        {
            Debug.Log("OpenXRLoader not found in active loaders. Attempting to add it.");

            string[] guids = AssetDatabase.FindAssets($"t:{openXrLoaderType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogError($"Could not find {openXrLoaderType.Name} asset. Make sure the OpenXR package is installed correctly.");
                 return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var openXrLoaderInstance = AssetDatabase.LoadAssetAtPath<OpenXRLoader>(path);

            if (openXrLoaderInstance == null)
            {
                 Debug.LogError($"Failed to load OpenXRLoader asset from path: {path}");
                 return;
            }

            var currentLoaders = xrManagerSettings.activeLoaders.ToList();
            currentLoaders.Add(openXrLoaderInstance); // Add OpenXR loader


            if (xrManagerSettings.TrySetLoaders(currentLoaders))
            {
                Debug.Log("Successfully assigned OpenXRLoader.");
                EditorUtility.SetDirty(xrManagerSettings);
            }
            else
            {
                Debug.LogError("Failed to assign OpenXRLoader using TrySetLoaders.");
                Debug.Log($"Current active loaders count before attempt: {xrManagerSettings.activeLoaders.Count}");
                foreach(var l in xrManagerSettings.activeLoaders) Debug.Log($" - {l.name} ({l.GetType()})");
                Debug.Log($"Loaders attempted to set count: {currentLoaders.Count}");
                 foreach(var l in currentLoaders) Debug.Log($" - {l.name} ({l.GetType()})");
                return; // Stop if assignment fails
            }
        }
        else
        {
            Debug.Log("OpenXRLoader is already assigned.");
        }

        var featureSetGuids = AssetDatabase.FindAssets("t:OpenXRFeatureSetManager");
        if (featureSetGuids.Length > 0)
        {
            string featureSetPath = AssetDatabase.GUIDToAssetPath(featureSetGuids[0]);
            var featureSetManager = AssetDatabase.LoadAssetAtPath<OpenXRFeatureSetManager>(featureSetPath);
            if (featureSetManager != null)
            {
                 var featureSet = featureSetManager.GetFeatureSetWithId("com.unity.openxr.featureset.metaquest"); // Example ID, might need verification
                 if (featureSet != null) {
                     featureSet.enabled = true;
                     Debug.Log("Enabled Meta Quest Feature Set.");
                     EditorUtility.SetDirty(featureSetManager); // Or featureSet? Check API
                 } else {
                     Debug.LogWarning("Meta Quest Feature Set not found. Skipping feature configuration.");
                 }

                 var touchProfileGuids = AssetDatabase.FindAssets("t:MetaQuestTouchControllerProfile"); // Verify class name
                 if (touchProfileGuids.Length > 0) {
                     string touchProfilePath = AssetDatabase.GUIDToAssetPath(touchProfileGuids[0]);
                     var touchProfileFeature = AssetDatabase.LoadAssetAtPath<OpenXRFeature>(touchProfilePath); // Base class is OpenXRFeature
                     if (touchProfileFeature != null) {
                         OpenXRSettings openxrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(androidBuildTargetGroup);
                         if (openxrSettings != null) {
                             bool featureEnabled = openxrSettings.features.Any(f => f != null && f.GetType() == touchProfileFeature.GetType());
                             if (!featureEnabled) {
                                 Debug.Log("Meta Quest Touch Controller Profile found but enabling via script needs more specific API knowledge.");
                             } else {
                                 Debug.Log("Meta Quest Touch Controller Profile already enabled.");
                             }
                         } else {
                             Debug.LogWarning("Could not get OpenXRSettings for Android. Skipping feature configuration.");
                         }
                     } else {
                         Debug.LogWarning("Could not load Meta Quest Touch Controller Profile feature asset.");
                     }
                 } else {
                     Debug.LogWarning("Meta Quest Touch Controller Profile feature asset not found.");
                 }

            } else {
                 Debug.LogWarning("Could not load OpenXRFeatureSetManager asset.");
            }
        } else {
             Debug.LogWarning("OpenXRFeatureSetManager asset not found. Skipping feature configuration.");
        }


        AssetDatabase.SaveAssets();
        Debug.Log("XR configuration finished.");
    }
}
