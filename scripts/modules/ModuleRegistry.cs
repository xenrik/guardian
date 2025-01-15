using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class ModuleRegistry : Resource {
    [Export]
    public ModuleRegistryElement[] Registry;
}