
# unity-navgen Example

Example of using [unity-navgen](https://github.com/idbrii/unity-navgen) based on the examples from [Unity's NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents).

  
  

![Example scene 7_dungeon](https://user-images.githubusercontent.com/43559/96374354-1c3ba600-1127-11eb-81dc-c7db7c3d6c63.png)

  

1. Open scene 7_dungeon.

1. Select Dungeon object.

1. Click "Create Disconnected Dungeon" to make a new dungeon.

1. Click "Select Nav Link Generator" to select the generator asset.

1. Click "Bake Create Interior Volumes" to insert volumes that prevent navmesh islands inside geometry. This step modifies the navmesh bake step.

1. Click "Bake NavMesh" to do normal navmesh bake.

1. Click "Bake Links" to create NavMeshLinks between edges of your navmesh. This step depends on the NavMesh bake step.

1. Click "Select NavMesh" to automatically select the scene's NavMeshSurface and see your resulting navmesh.

  

This example project does not contain unity-navgen. It imports it as a package in manifest.json:

  

"com.github.idbrii.unity-navgen": "https://github.com/idbrii/unity-navgen.git#latest-release",

# Runtime Uesage
Rebuild Navmesh and MeshLink:
`NavLinkGenerator_Runtime.RebuildAll();`

Rebuild Navmesh:
`NavLinkGenerator_Runtime.main.navMeshSurface.BuildNavMesh();`

Rebuild MeshLinks:
`NavLinkGenerator_Runtime.main.RebuildLinks();`