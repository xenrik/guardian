using Godot;

// Not using a test framework atm...
public partial class JsonFileWriterTest : Node {
    public override void _Ready() {
        base._Ready();
        var mapper = new ITypeMapper.Delegate(
            (value) => "testClass",
            (typeId) => typeof(TestClass)
        );
        JsonFileWriter writer = new("testfile.json", mapper);

        TestClass value = new();
        value.namedProperty = "namedValue";
        value.stringProperty = "stringValue";
        value.intProperty = 123;
        value.boolProperty = true;
        value.vector3Property = new Vector3(1, 2, 3);

        writer.Write(value);
        writer.Close();

        var file = FileAccess.Open("user://testfile.json", FileAccess.ModeFlags.Read);
        Assert.AreEqual("""
{
"type":"testClass",
"customName":"namedValue",
"stringProperty":"stringValue",
"intProperty":123,
"boolProperty":true,
"vector3Property":"(1, 2, 3)"
}
""".StripNewLines(), file.GetAsText());

        JsonFileReader reader = new("testfile.json", mapper);
        TestClass loadedValue = (TestClass)reader.Read();

        Assert.AreEqual(value.namedProperty, loadedValue.namedProperty);
        Assert.AreEqual(value.stringProperty, loadedValue.stringProperty);
        Assert.AreEqual(value.intProperty, loadedValue.intProperty);
        Assert.AreEqual(value.boolProperty, loadedValue.boolProperty);
        Assert.AreEqual(value.vector3Property, loadedValue.vector3Property);
    }
}

public class TestClass : IPersistable {
    [Persist(name: "customName")]
    public string namedProperty;

    [Persist]
    public string nullProperty;

    public string nonPersistedProperty = "defaultValue";

    [Persist]
    public string stringProperty;

    [Persist]
    public int intProperty;

    [Persist]
    public bool boolProperty;

    [Persist]
    public Vector3 vector3Property;
}