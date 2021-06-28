# UnityNavMeshAreas
Self-updating Areas enum for NavMesh in Unity
 
 - Contains an **Enum called NavMeshAreas** so that you don't have to mess around with strings anymore.
 - The file **re-writes itself** whenever scripts compile or assets are saved in the editor.
 - Includes a **PropertyDrawer** so that the insepector will use a multiselect flags dropdown.
 - Contains a helper 'AreaMask' class to make working with flags easier.

###### Usage Examples
```
Agent.areaMask = (int)NavMeshAreas.Walkable;

var mask1 = (AreaMask)(NavMeshAreas.Walkable | NavMeshAreas.Climb);
var mask2 = mask1.Add(NavMeshAreas.Edge);
var mask3 = mask2.Remove(NavMeshAreas.Climb);
```

Unity Version: 2018.3
