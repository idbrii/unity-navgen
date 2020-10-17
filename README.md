# unity-navgen
Tools for working with Unity's NavMeshComponents and generating navmesh: link generation, mesh cleanup, etc

## NavLinkGenerator

NavLinkGenerator is an asset for generating
[NavMeshLinks](https://docs.unity3d.com/Manual/class-NavMeshLink.html) across
gaps in your navmesh. It also serves as the central hub for navgen.

NavLinkGenerator is a ScriptableObject -- so you need to create one to start
using it (Assets > Create > AI > NavLinkGenerator). The asset contains
settings and buttons for generating links.


# NavMeshAreas

You can assign enum values from `UnityEngine.AI.NavMeshAreas` to NavMeshAgent's
AreaMask and `UnityEngine.AI.NavMeshAreaIndex` to area indexes in
NavMeshSurface, NavMeshLink, NavMeshModifierVolume, etc. These enums are
automatically updated from the areas defined in Navigation (Window > AI >
Navigation).


# Alternatives

* [NavMeshLinks_AutoPlacer by eDmitriy](https://forum.unity.com/threads/navmesh-links-generator-for-navmeshcomponents.515143/)
* [Navmesh Cleaner](https://assetstore.unity.com/packages/tools/ai/navmesh-cleaner-151501) ([see here](http://answers.unity.com/answers/1781054/view.html) to use with NavMeshComponents)

# Credits

This project includes [UnityNavMeshAreas](https://github.com/jeffvella/UnityNavMeshAreas) Copyright (c) 2018 jeffvella.
