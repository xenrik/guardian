# Model Scene
- Create a scene "<module name>", root type is Rigidbody
- Import the blender object
- Move the Mesh and CollisionShape onto the root node

# Editor Version
- Create a scene with the format "<module name>-editor", root type is Node3D
- Add EditorModule script
- Drag in the model scene
- Add snap collision points
    - Layer: Snap
    - Add Anchors
- Link Signals to script
    - Body Collision
    - Snap Collisions