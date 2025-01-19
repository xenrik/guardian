using Godot;

// Not using a test framework atm...
public partial class JsonFileWriterTest : Node {
    public override void _Ready() {
        base._Ready();

        JsonFileWriter writer = new("testfile.json", new ITypeMapper.Delegate(
            (value) => "typeId",
            (typeId) => typeof(string)
        ));

        TestClass value = new();
        value.namedProperty = "namedValue";
        value.stringProperty = "stringValue";
        value.vector3Property = new Vector3(1, 2, 3);

        writer.Write(value);
        writer.Close();
    }
}

public class TestClass : IPersistable {
    [Persist(name: "customName")]
    public string namedProperty;

    [Persist]
    public string stringProperty;

    [Persist]
    public string nullProperty;

    public string nonPersistedProperty = "defaultValue";

    [Persist]
    public Vector3 vector3Property;
}