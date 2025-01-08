# Game Scene
- Create a scene "<module name>"
- Import the blender object
- Move the Mesh and CollisionShape onto the root node

# Editor Version
- Create a scene with the format "<module name>-editor", root type is Node3D
- Add EditorModule script
- Add StaticBody3D
	- Drag in the model as a child
	- Move the Mesh and CollisionShape onto the StaticBody3D
	- Link Mouse Entered/Exited to EditorModule script
	- Add snap collision points
		- Layer: Snap
		- Link Area Shape Entered/Exited to EditorModule script
		- Add Anchors
	
