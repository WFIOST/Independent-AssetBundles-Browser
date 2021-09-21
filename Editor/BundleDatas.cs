using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bundle Data", menuName = "AssetBundleBuilder/BundleData", order = 0)]
public class BundleDatas : ScriptableObject {
	public string AssetBundlePrefix = "AssetBundle_";
	public string ManifestPrefix = "Manifest_";
	public AssetBundleBrowser.AssetBundleDataSource.ABinfo[] Bundles;
}
