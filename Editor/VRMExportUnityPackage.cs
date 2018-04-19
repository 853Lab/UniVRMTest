﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;


namespace VRM
{
    public static class VRMExportUnityPackage
    {
        static string GetDesktop()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/VRM";
        }

        const string DATE_FORMAT = "yyyyMMdd";
        const string PREFIX = "UniVRM";

        static string GetPath(string prefix)
        {
            var folder = GetDesktop();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //var date = DateTime.Today.ToString(DATE_FORMAT);

            var path = string.Format("{0}/{1}-{2}.unitypackage",
                folder,
                prefix,
                VRMVersion.VERSION
                ).Replace("\\", "/");

            return path;
        }

        static IEnumerable<string> EnumerateFiles(string path)
        {
            if (Path.GetFileName(path).StartsWith(".git"))
            {
                yield break;
            }

            if (Directory.Exists(path))
            {
                foreach(var child in Directory.GetFileSystemEntries(path))
                {
                    foreach(var x in EnumerateFiles(child))
                    {
                        yield return x;
                    }
                }
            }
            else
            {
                if (Path.GetExtension(path).ToLower() != ".meta")
                {
                    yield return path.Replace("\\", "/");
                }
            }
        }

#if VRM_DEVELOP
        [MenuItem("VRM/Export unitypackage")]
#endif
        public static void CreateUnityPackage()
        {
            // まずビルドする
            var levels = new string[] { "Assets/VRM.Samples/Scenes/VRMRuntimeLoaderSample.unity" };
            var buildPath = Path.GetFullPath(Application.dataPath + "/../build/build.exe");
            Debug.LogFormat("{0}", buildPath);
            var build=BuildPipeline.BuildPlayer(levels, 
                buildPath, 
                BuildTarget.StandaloneWindows, 
                BuildOptions.None
                );
#if UNITY_2018_1_OR_NEWER
            var iSuccess = build.summary.result != BuildResult.Succeeded;
#else
            var iSuccess = !string.IsNullOrEmpty(build);
#endif

            var path = GetPath(PREFIX);
            if (File.Exists(path))
            {
                Debug.LogErrorFormat("{0} is already exists", path);
                return;
            }

            // 本体
            AssetDatabase.ExportPackage(EnumerateFiles("Assets/VRM").ToArray()
                , path, ExportPackageOptions.Interactive);

            // サンプル
            AssetDatabase.ExportPackage(EnumerateFiles("Assets/VRM.Samples").Concat(EnumerateFiles("Assets/StreamingAssets")).ToArray()
                , GetPath(PREFIX+"-RuntimeLoaderSample"), ExportPackageOptions.Interactive);

            Debug.LogFormat("exported: {0}", path);
        }
    }
}
