# Unity Asset Bundle Browser tool

This tool enables the user to view and edit the configuration of asset bundles for their Unity project.  It will block editing that would create invalid bundles, and inform you of any issues with existing bundles.  It also provides basic build functionality.

This tool is intended to replace the current workflow of selecting assets and setting their asset bundle manually in the inspector.  It can be dropped into any Unity project with a version of 5.6 or greater.  It will create a new menu item in *Window->AssetBundle Browser*.  

## Full Documentation
#### Official Released Features
See [the official manual page](https://docs.unity3d.com/Manual/AssetBundles-Browser.html) or view the included [project manual page](Documentation/com.unity.assetbundlebrowser.md)

# What is this fork?

Unity's AssetBundle system is specifically designed so that all the assetbundles rely on each other, aka dependencies. Unity's assetbundle system inherently makes it near impossible to ensure that assetbundles remain independent without many workarounds and careful planning.

As a mod maker for the game Hotdogs, Horseshoes, and Handgrenades, this caused several issues. This meant that making two separate mods using the same unity project was damn near impossible. So of course, I made a solution.

Independent AssetBundles Brower, or IABB, circumvents this system by calling the build pipeline once per Assetbundle, building one Assetbundle at a time, ensuring they will remain independent.

## What are the downsides?

IABB uses its own system to define what is built in an Assetbundle.

The main parts are the Assetbundle's name, its root folders (everything inside the root folder, and its subfolders are put in the assetbundle), selecting to grab dependencies, and excluding items based on name (will also exclude folders of the name) and exclude dependencies based on name. (dependencies are not excluded based on item exclusions)
