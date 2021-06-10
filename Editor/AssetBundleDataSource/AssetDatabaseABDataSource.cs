using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleBrowser.AssetBundleDataSource
{
	class ABinfo
	{
		public string AssetBundleName = "";
		public List<string> RootFolders = new List<string>();
		public List<string> FilesToInclude = new List<string>();
		public List<string> FilesToExclude = new List<string>();
		public bool GetDependencies = true;
		public List<string> DependenciesToExclude = new List<string>();
	}

    internal class AssetDatabaseABDataSource : ABDataSource
    {
        public static List<ABDataSource> CreateDataSources()
        {
            var op = new AssetDatabaseABDataSource();
            var retList = new List<ABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string Name {
            get {
                return "Default";
            }
        }

        public string ProviderName {
            get {
                return "Built-in";
            }
        }

        public string[] GetAssetPathsFromAssetBundle (string assetBundleName) {
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
        }

        public string GetAssetBundleName(string assetPath) {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null) {
                return string.Empty;
            }
            var bundleName = importer.assetBundleName;
            if (importer.assetBundleVariant.Length > 0) {
                bundleName = bundleName + "." + importer.assetBundleVariant;
            }
            return bundleName;
        }

        public string GetImplicitAssetBundleName(string assetPath) {
            return AssetDatabase.GetImplicitAssetBundleName (assetPath);
        }

        public string[] GetAllAssetBundleNames() {
            return AssetDatabase.GetAllAssetBundleNames ();
        }

        public bool IsReadOnly() {
            return false;
        }

        public void SetAssetBundleNameAndVariant (string assetPath, string bundleName, string variantName) {
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, variantName);
        }

        public void RemoveUnusedAssetBundleNames() {
            AssetDatabase.RemoveUnusedAssetBundleNames ();
        }

        public bool CanSpecifyBuildTarget { 
            get { return true; } 
        }
        public bool CanSpecifyBuildOutputDirectory { 
            get { return true; } 
        }

        public bool CanSpecifyBuildOptions { 
            get { return true; } 
        }

		public bool BuildAssetBundles(ABBuildInfo info) {
			if (info == null)
			{
				Debug.Log("Error in build");
				return false;
			}
            
            
            //manual input of assetbundle locs. fuck around with this to change what abs to build
			ABinfo[] ABs = {
			new ABinfo(){
				AssetBundleName = "pccg_ammo",
				RootFolders = new List<string> { "Assets/guns/customguns/Ammo/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_attachments",
				RootFolders = new List<string> { "Assets/guns/customguns/Attachments/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_carbines",
				RootFolders = new List<string> { "Assets/guns/customguns/Carbines/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_explosives",
				RootFolders = new List<string> { "Assets/guns/customguns/ExplosiveDevices/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_machineguns",
				RootFolders = new List<string> { "Assets/guns/customguns/MG/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_pistols",
				RootFolders = new List<string> { "Assets/guns/customguns/Pistols/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_rifles",
				RootFolders = new List<string> { "Assets/guns/customguns/Rifles/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_shotguns",
				RootFolders = new List<string> { "Assets/guns/customguns/Shotguns/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_smgs",
				RootFolders = new List<string> { "Assets/guns/customguns/SMGs/" }
			},

			new ABinfo(){
				AssetBundleName = "pccg_other",
				RootFolders = new List<string> {
					"Assets/guns/customguns/akSuperMag/",
					"Assets/guns/customguns/akUnderFolder/",
					"Assets/guns/customguns/g43/",
					"Assets/guns/customguns/LeeEnfieldBrenMag/",
					"Assets/guns/customguns/m1gO/",
					"Assets/guns/customguns/Magazines/",
					"Assets/guns/customguns/mas38/",
					"Assets/guns/customguns/ThompsonNHG/",
					"Assets/guns/customguns/turtsSaiga/"
				}
			}

			};

			AssetBundleBuild[] abb = new AssetBundleBuild[1];
			for (int i = 0; i < ABs.Length; i++)
			{
				abb[0] = GenerateAssetBundleAssetList(ABs[i]);
				var buildManifest = BuildPipeline.BuildAssetBundles(info.outputDirectory, abb, info.options, info.buildTarget);
				if (buildManifest == null)
				{
					Debug.Log("Error in build");
					return false;
				}

				foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
				{
					if (info.onBuild != null)
					{
						info.onBuild(assetBundleName);
					}
				}
			}
			

            return true;
        }

		public static AssetBundleBuild GenerateAssetBundleAssetList(ABinfo info)
		{
			AssetBundleBuild abb = new AssetBundleBuild();
			abb.assetBundleName = info.AssetBundleName;

			//get assets start!
			string[] assets = new string[0];
			string[] assetstemp = new string[0];

			//get all assets under root dirs
			for (int i = 0; i < info.RootFolders.Count; i++)
			{
				assets = assets.Concat(Directory.GetFiles(info.RootFolders[i], "*.*", SearchOption.AllDirectories)).ToArray();//get assets
			}

			assets = assets.Where(tag => !tag.Contains(".meta")).ToArray(); //remove all meta files
			assets = assets.Where(tag => !tag.Contains(".lock")).ToArray(); //remove all lock files


			//remove all excluded files
			for (int i = 0; i < info.FilesToExclude.Count; i++)
			{
				assets = assets.Where(tag => !tag.Contains(info.FilesToExclude[i])).ToArray();
			}


			//keep only included files
			assetstemp = new string[0];
			if (info.FilesToInclude.Count != 0)
			{
				for (int i = 0; i < info.FilesToInclude.Count; i++)
				{
					assetstemp = assetstemp.Concat(assets.Where(tag => tag.Contains(info.FilesToInclude[i]))).ToArray();
				}

				assets = assetstemp.Distinct().ToArray(); //remove any potential duplicates
			}

			//get dependencies!
			if (info.GetDependencies)
			{
				assetstemp = new string[0]; //reset assetstemp
				for (int i = 0; i < assets.Length; i++)
				{
					assetstemp = assetstemp.Concat(AssetDatabase.GetDependencies(assets[i])).ToArray();
				}

				//remove all excluded dependencies
				for (int i = 0; i < info.DependenciesToExclude.Count; i++)
				{
					assetstemp = assetstemp.Where(tag => !tag.Contains(info.DependenciesToExclude[i])).ToArray();
				}

				assets = assets.Concat(assetstemp).Distinct().ToArray(); //add assetstemp into assets, remove duplicates
			}

			//cleanup
			//standardize all dirs (prevents considering dir/file.ext different from dir\file.ext)
			for (int i = 0; i < assets.Length; i++)
			{
				assets[i] = assets[i].Replace(@"\".ToCharArray()[0], '/'); //WHY DOESN'T @'\' WORK I HATE ESCAPE CHARS
			}
			

			assets = assets.Distinct().ToArray(); //remove any potential duplicates
			assets = assets.Where(tag => !tag.Contains(".meta")).ToArray(); //remove all meta files
			assets = assets.Where(tag => !tag.Contains(".lock")).ToArray(); //remove all lock files
			assets = assets.Where(tag => !tag.Contains(".cs")).ToArray(); //remove all cs files

			//getting assets is finished!
			abb.assetNames = assets; //load it up into the ABB

			return abb; //send it off
		}
    }
}
