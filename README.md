# unity-navgen
Tools for working with [Unity's
NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) and
generating navmesh: link generation, mesh cleanup, etc


Default NavMesh Generation                                                                                                                         | Using NavLinkGenerator's Bake Links and Interior Volumes
:-------------------------:                                                                                                                        | :-------------------------:
![Default navmesh generation](https://user-images.githubusercontent.com/43559/96373807-b0a40980-1123-11eb-8bec-a5921c9819f7.png)                   | ![Hiding cube platform to show navmesh island inside](https://user-images.githubusercontent.com/43559/96373809-b13ca000-1123-11eb-9431-e30834db4af1.png)
![After running Bake Links in NavLinkGenerator](https://user-images.githubusercontent.com/43559/96373808-b13ca000-1123-11eb-9e11-d1b2cb41cfba.png) | ![Hiding cube platform to show there's no navmesh island inside](https://user-images.githubusercontent.com/43559/96373810-b1d53680-1123-11eb-94e3-2c61b481973b.png)

## NavLinkGenerator

NavLinkGenerator is an asset for generating
[NavMeshLinks](https://docs.unity3d.com/Manual/class-NavMeshLink.html) across
gaps in your navmesh. It also serves as the central hub for navgen.

![NavLinkGenerator](https://user-images.githubusercontent.com/43559/96361844-081f8680-10de-11eb-86ea-23157153d05e.png)

NavLinkGenerator is a ScriptableObject -- so you need to create one to start
using it (Assets > Create > Navigation > NavLinkGenerator). The asset contains
settings and buttons for generating links.


# NavNonWalkableCollection

The "Create Interior Volumes" button in NavLinkGenerator creates a
NavNonWalkableCollection which tracks the volumes so they can be rebuilt.
Remove a volume from this component's list to prevent it from being modified.


# NavMeshAreas

You can assign enum values from `UnityEngine.AI.NavMeshAreas` to NavMeshAgent's
AreaMask and `UnityEngine.AI.NavMeshAreaIndex` to area indexes in
NavMeshSurface, NavMeshLink, NavMeshModifierVolume, etc. These enums are
automatically updated from the areas defined in Navigation (Window > AI >
Navigation).

[NavMeshAreas generates two enums
](https://github.com/idbrii/unity-navgen/blob/16d4ba6c16228d7f7b9fe7a91ff8b8a837ba842c/Runtime/NavMeshAreas/NavMeshAreas.cs#L19-L32)
that look something like this:

```cs
// NavMeshAgent uses AreaMask.
[Flags]
public enum NavMeshAreas
{
    None = 0,
    Walkable = 1, NotWalkable = 2, Jump = 4, Climb = 8, Blocked = 16, Hole = 32, Edge = 64, Fall = 128, New1 = 256, Stuff = 512, 
    All = ~0,
}

// NavMeshSurface, NavMeshLink, NavMeshModifierVolume, etc. use indexes.
public enum NavMeshAreaIndex
{
    Walkable = 0, NotWalkable = 1, Jump = 2, Climb = 3, Blocked = 4, Hole = 5, Edge = 6, Fall = 7, New1 = 8, Stuff = 9, 
}
```


# Example
See the [example branch](https://github.com/idbrii/unity-navgen/tree/example) for a demonstration project.


# Installation

1. Install Unity [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents.git) from github.
2. Copy the code to your project or add a dependency to your manifest.json to install as a package:

    "com.github.idbrii.unity-navgen": "https://github.com/idbrii/unity-navgen.git#latest-release",


# Alternatives

* [NavMeshLinks_AutoPlacer by eDmitriy](https://forum.unity.com/threads/navmesh-links-generator-for-navmeshcomponents.515143/)
* [Navmesh Cleaner](https://assetstore.unity.com/packages/tools/ai/navmesh-cleaner-151501) ([see here](http://answers.unity.com/answers/1781054/view.html) to use with NavMeshComponents)

# Credits

This project includes [UnityNavMeshAreas](https://github.com/jeffvella/UnityNavMeshAreas) Copyright (c) 2018 jeffvella.
