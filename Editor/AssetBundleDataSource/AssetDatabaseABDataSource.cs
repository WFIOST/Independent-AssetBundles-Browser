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
	[System.Serializable]
	public class ABinfo
	{
		[Tooltip("Asset bundle name. Builds to folder using [AssetBundlePrefix][AssetBundleName]")]
		public string AssetBundleName = "";
		[Tooltip("If disabled, this will not build.")]
		public bool setToBuild = true;
		[Tooltip("If disabled, this will not build a Data bundle.")]
		public bool splitLateAndData = true;
		[Tooltip("Root folders. All subfolders and subfiles will be added as assets to the bundle.")]
		public List<string> RootFolders = new List<string>();
		[Tooltip("Specific files to include, if needed.")]
		public List<string> FilesToInclude = new List<string>();
		[Tooltip("Specific files to exclude, if needed.")]
		public List<string> FilesToExclude = new List<string>();
		[Tooltip("If enabled, this will grab dependencies of each item. FilesToExclude does not affect Dependencies.")]
		public bool GetDependencies = true;
		[Tooltip("Works like the RootFolders, but excludes only dependencies.")]
		public List<string> DependenciesToExclude = new List<string>();
		//[HideInInspector]
		public bool isData;
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

		public string Name
		{
			get
			{
				return "Default";
			}
		}

		public string ProviderName
		{
			get
			{
				return "Built-in";
			}
		}

		public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
		{
			return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
		}

		public string GetAssetBundleName(string assetPath)
		{
			var importer = AssetImporter.GetAtPath(assetPath);
			if (importer == null)
			{
				return string.Empty;
			}
			var bundleName = importer.assetBundleName;
			if (importer.assetBundleVariant.Length > 0)
			{
				bundleName = bundleName + "." + importer.assetBundleVariant;
			}
			return bundleName;
		}

		public string GetImplicitAssetBundleName(string assetPath)
		{
			return AssetDatabase.GetImplicitAssetBundleName(assetPath);
		}

		public string[] GetAllAssetBundleNames()
		{
			return AssetDatabase.GetAllAssetBundleNames();
		}

		public bool IsReadOnly()
		{
			return false;
		}

		public void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variantName)
		{
			AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, variantName);
		}

		public void RemoveUnusedAssetBundleNames()
		{
			AssetDatabase.RemoveUnusedAssetBundleNames();
		}

		public bool CanSpecifyBuildTarget
		{
			get { return true; }
		}
		public bool CanSpecifyBuildOutputDirectory
		{
			get { return true; }
		}

		public bool CanSpecifyBuildOptions
		{
			get { return true; }
		}


		public bool BuildAssetBundles(ABBuildInfo info, BundleDatas BDs)
		{
			if (info == null)
			{
				Debug.Log("Error in build");
				return false;
			}
			if (BDs == null)
			{
				Debug.Log("Bundle data is null!");
				return false;
			}

			ABinfo[] ABs = new ABinfo[0];

			ABs = BDs.Bundles;

			AssetBundleBuild[] abb = new AssetBundleBuild[1];
			for (int i = 0; i < ABs.Length; i++)
			{
				//get/build the late bundles
				if (ABs[i] == null) { continue; }
				if (!ABs[i].setToBuild) { continue; }
				if (ABs[i].splitLateAndData)
				{
					Debug.Log("Generating Data bundle");
					abb[0] = GenerateAssetBundleData(ABs[i], BDs);
					BuildBundle(abb[0], info, BDs);
				}
				Debug.Log("Generating Late bundle");
				abb[0] = GenerateAssetBundleAssetList(ABs[i], BDs);
				BuildBundle(abb[0], info, BDs);

				//get/build the data bundles
				//abb[0] = GenerateAssetBundleData(abb[0], BDs);
				//BuildBundle(abb[0], info, BDs);
			}

			/*//get/build the data bundles
			abb = new AssetBundleBuild[1];
			for (int i = 0; i < ABs.Length; i++)
			{
				if (!ABs[i].setToBuild) { continue; }
				if (!ABs[i].splitLateAndData) { continue; }
				
				if (abb[0].assetNames.Length == 0) { continue; }
				
			}*/
			return true;
		}

		public static AssetBundleBuild GenerateAssetBundleAssetList(ABinfo info, BundleDatas BDs)
		{
			AssetBundleBuild abb = new AssetBundleBuild();

			string abn = "";
			if (info.splitLateAndData) abn += "late_";
			abn += BDs.AssetBundlePrefix + info.AssetBundleName;

			abb.assetBundleName = abn;

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
			assets = assets.Where(tag => !tag.Contains(".dll")).ToArray(); //remove all dll files



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
			assets = assets.Where(tag => !tag.Contains(".dll")).ToArray(); //remove all dll files

			//getting assets is finished!
			abb.assetNames = assets; //load it up into the ABB

			return abb; //send it off
		}

		public static AssetBundleBuild GenerateAssetBundleData(ABinfo info, BundleDatas BDs)
		{
			var ab = GenerateAssetBundleAssetList(info, BDs);
			string[] newAssets = ab.assetNames.Where(tag => tag.Contains(".asset")).ToArray();
			//var pngs = ab.assetNames.Where(tag => tag.Contains("_ISpic")).ToArray();
			var pngs = ab.assetNames.Where(tag => tag.Contains(".png")).ToArray();
			//cook out any pngs named "basecolour", "alloy", and "normal"
			//literally just basecolour variations
			pngs = pngs.Where(tag => !tag.ToLower().Contains("basecolour")).ToArray();
			pngs = pngs.Where(tag => !tag.ToLower().Contains("base colour")).ToArray();
			pngs = pngs.Where(tag => !tag.ToLower().Contains("basecolor")).ToArray(); //coloUr ftw
			pngs = pngs.Where(tag => !tag.ToLower().Contains("base color")).ToArray();
			//end of basecolour variations
			pngs = pngs.Where(tag => !tag.ToLower().Contains("alloy")).ToArray();
			pngs = pngs.Where(tag => !tag.ToLower().Contains("normal")).ToArray();
			newAssets = newAssets.Concat(pngs).ToArray();
			ab.assetNames = newAssets;
			Debug.Log(newAssets.Length + " items to add to bundle!");
			ab.assetBundleName = ab.assetBundleName.Remove(0, 5);
			return ab;
		}

		public static bool BuildBundle(AssetBundleBuild abb, ABBuildInfo info, BundleDatas BDs)
		{
			Debug.Log("Building Bundle " + abb.assetBundleName);
			var buildManifest = BuildPipeline.BuildAssetBundles(info.outputDirectory, new AssetBundleBuild[] { abb }, info.options, info.buildTarget);
			if (buildManifest == null)
			{
				Debug.LogError("Error in build");
				return false;
			}

			foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
			{
				if (info.onBuild != null)
				{
					info.onBuild(assetBundleName);
				}
			}

			//change to manifest prefix
			var mname = abb.assetBundleName + ".manifest";
			var newmname = mname.Replace(BDs.AssetBundlePrefix, BDs.ManifestPrefix).ToLower();
			newmname = newmname.Replace("late_", "mlate_"); //remove late from the name, to "manifest late"
			var mpath = Path.Combine(info.outputDirectory, mname);
			if (File.Exists(mpath))
			{
				if (File.Exists(Path.Combine(info.outputDirectory, newmname)))
				{
					File.Delete(Path.Combine(info.outputDirectory, newmname));
				}
				File.Move(Path.Combine(info.outputDirectory, mname), Path.Combine(info.outputDirectory, newmname));
			}

			return true;
		}
	}
}
