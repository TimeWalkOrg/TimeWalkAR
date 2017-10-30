using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace MORPH3D {
    [InitializeOnLoad]
    public class MCSUpgrade : Editor
    {
        public static string pathToInstallScript = "Assets/MORPH3D/Content/Packages/Editor/MCSUpgrader.cs";
        public static string pathToPackages = "Assets/MORPH3D/Content/Packages";
        public static string pathToPackagesCore = "Assets/MORPH3D/Content/Packages/Core.unitypackage";
        public static string pathToPackagesFemale = "Assets/MORPH3D/Content/Packages/M3DFemale.unitypackage";
        public static string pathToPackagesMale = "Assets/MORPH3D/Content/Packages/M3DMale.unitypackage";
        protected static string pathToConfig = "Assets/MORPH3D/MCSUpgrade.json";

        protected static MCSUpgradeWindow window = null;

        [Serializable]
        public struct UpgradeMeta
        {
            [SerializeField]
            public bool disabled;
            [SerializeField]
            public bool hideOneDotSixPopup;
            [SerializeField]
            public string[] installed;
        }

        public static UpgradeMeta GetMeta()
        {
            UpgradeMeta? meta = null;
            if (File.Exists(pathToConfig))
            {
                try
                {
                    string raw = File.ReadAllText(pathToConfig);
                    meta = JsonUtility.FromJson<UpgradeMeta>(raw);
                } catch 
                {

                }
            }

            if(meta == null)
            {
                meta = new UpgradeMeta();
            }

            return (UpgradeMeta)meta;
        }

        public static void SaveMeta(UpgradeMeta meta)
        {
            try
            {
                string raw = JsonUtility.ToJson(meta);
                File.WriteAllText(pathToConfig, raw);
            } catch (Exception e)
            {
                UnityEngine.Debug.LogError("Unable to save meta configuration for upgrade");
                UnityEngine.Debug.LogException(e);
            }
        }
    
        static MCSUpgrade()
        {
            //Have we shown the popup for 1.0->1.6?
            UpgradeMeta meta = GetMeta();

            bool shouldShowPopup = !meta.disabled && !meta.hideOneDotSixPopup;

            if(meta.installed != null && meta.installed.Length != 3)
            {
                //we might have pending packages
                string[] paths = Directory.GetFiles(pathToPackages, "*.unitypackage", SearchOption.TopDirectoryOnly);

                if(meta.installed == null)
                {
                    meta.installed = new string[] { };
                }

                List<string> pending = new List<string>();

                for(int i = 0; i < paths.Length; i++)
                {
                    string basePath = Application.dataPath;
                    string path = paths[i].Replace(@"\",@"/").Replace(basePath,"");

                    bool newPackage = true;


                    for(int j = 0; j < meta.installed.Length; j++)
                    {
                        if (meta.installed[j].Equals(path))
                        {
                            newPackage = false;
                            break;
                        }
                    }

                    if (newPackage)
                    {
                        UnityEngine.Debug.Log("Need to install: " + path);
                        pending.Add(path);
                    }

                }

                if (pending.Count > 0)
                {
                    window = MCSUpgradeWindow.Instance;
                    window.packageQueue = pending;
                    window.deleteQueue = new List<string>();
                }
            }


            if (shouldShowPopup)
            {
                window = MCSUpgradeWindow.Instance;
            }
        }

        public static void AcceptInstall()
        {
            UpgradeMeta meta = GetMeta();
            meta.hideOneDotSixPopup = true;
            SaveMeta(meta);

            window.packageQueue = new List<string>();
            window.deleteQueue = new List<string>();

            window.packageQueue.Add(pathToPackagesCore);
            window.packageQueue.Add(pathToPackagesFemale);
            window.packageQueue.Add(pathToPackagesMale);
        }

        public static bool? InstallPackage(string path)
        {
            if (!path.EndsWith(".unitypackage") || !File.Exists(path) || !path.Contains(pathToPackages))
            {
                return null;
            }

            UnityEngine.Debug.Log("MCSUpgrade installing: " + path);
            try
            {
                AssetDatabase.ImportPackage(path, false);
                //UnityEngine.Debug.Log("Triggering refresh");
            } catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }

            return true;
        }

        [MenuItem("MORPH 3D/Show 1.0 to 1.6 Upgrade Window")]
        public static void MenuItemShowUpgradeWindow()
        {
           window = MCSUpgradeWindow.Instance;
        }

    }

    public class MCSUpgradeWindow : EditorWindow
    {
        public List<string> packageQueue = null;
        public List<string> deleteQueue = null;
        protected bool refresh = false;
        Int32 lastTime = 0;

        public void Update()
        {

            if (refresh)
            {
                refresh = false;
                lastTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                AssetDatabase.Refresh(ImportAssetOptions.Default | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
                return;
            }

            if( (packageQueue == null || packageQueue.Count <= 0) && (deleteQueue == null || deleteQueue.Count <= 0))
            {
                return;
            }

            Int32 currentTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            Int32 delta = currentTime - lastTime;

            if (delta < 5)
            {
                //wait for later
                return;
            }

            lastTime = currentTime;

            if (packageQueue.Count > 0)
            {
                string path = packageQueue[0];
                packageQueue.RemoveAt(0);
                bool? result = MCSUpgrade.InstallPackage(path);
                if (result == false)
                {
                    UnityEngine.Debug.LogError("Aborting package install as: " + path + " failed to install");
                    packageQueue.Clear();
                }
                else
                {
                    if(result == true)
                    {
                        MCSUpgrade.UpgradeMeta meta = MCSUpgrade.GetMeta();
                        if(meta.installed == null)
                        {
                            meta.installed = new string[] { };
                        }
                        List<string> installed = new List<string>(meta.installed);
                        installed.Add(path);
                        meta.installed = installed.ToArray();
                        MCSUpgrade.SaveMeta(meta);
                    }

                    deleteQueue.Add(path);
                }
                refresh = true;

                //wait until later
                return;
            }

            if (deleteQueue.Count > 0)
            {

                try
                {
                    foreach (string path in deleteQueue)
                    {
                        if (!path.EndsWith(".unitypackage") || !File.Exists(path) || !path.Contains(MCSUpgrade.pathToPackages))
                        {
                            UnityEngine.Debug.Log("Skipping: " + path);
                            continue;
                        }
                        UnityEngine.Debug.Log("Removing install file: " + path);
                        AssetDatabase.DeleteAsset(path);
                    }

                } catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Unable to remove all install packages");
                    UnityEngine.Debug.LogException(e);
                }

                deleteQueue.Clear();

                if (File.Exists(MCSUpgrade.pathToInstallScript))
                {
                    UnityEngine.Debug.Log("Removing installer script");
                    AssetDatabase.DeleteAsset(MCSUpgrade.pathToInstallScript);
                }


                Close();
            }

        }

        public void OnGUI()
        {
            GUILayout.Label(
                "Upgrading from 1.0 to 1.6\n"
                + "\n\n"
                + "Please note, if you're upgrading from 1.0 to 1.6\n"
                + "this content and code is *NOT* compatible with 1.0 of MCS!\n"
                + "\n"
                + "Your project may break.\n"
                + "\n"
                + "For more info, visit https://morph3d.com"
                + "\n\n"
            );

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.padding.top = 10;
            style.padding.bottom = 10;
            style.margin.top = 50;

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Cancel, do NOT install",style))
            {
                Close();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Install, I'm ready to upgrade.\nDo not warn me again.",style))
            {
                MCSUpgrade.AcceptInstall();
                return;
            }
            EditorGUILayout.EndHorizontal();
        }

        private static MCSUpgradeWindow _instance;
        public static MCSUpgradeWindow Instance
        {
            get {
                if (_instance == null)
                {
                    Rect rect = new Rect(50, 50, 600, 600);
                    _instance = (MCSUpgradeWindow)GetWindowWithRect(typeof(MCSUpgradeWindow),rect, false, "1.0 to 1.6 Upgrade");
                }
                return _instance;
            }
        }
        public static void RepaintWindow()
        {
            if (_instance != null)
                _instance.Repaint();
        }

        void OnEnable()
        {
            _instance = this;
        }
        void OnDisable()
        {
            _instance = null;
        }
    }
}