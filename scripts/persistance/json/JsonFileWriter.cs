using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

public class JsonFileWriter : IWriter {
    private string filename;
    private ITypeMapper mapper;

    private bool wroteElement;

    private FileAccess file;

    public JsonFileWriter(string filename, ITypeMapper mapper) {
        this.filename = filename;
        this.mapper = mapper;

        DirAccess.MakeDirRecursiveAbsolute(System.IO.Path.GetDirectoryName(filename));
        file = FileAccess.Open("user://" + filename, FileAccess.ModeFlags.Write);
    }

    public void Close() {
        file.Close();
        file = null;
    }

    public void Write(IPersistable value) {
        if (file == null) {
            throw new Exception("Writer is closed");
        } else if (wroteElement) {
            throw new Exception("JsonFileWriter cannot be used to write multiple values");
        }

        string json = ToJson(value);
        file.StoreString(json);
        wroteElement = true;
    }

    private string ToJson(IPersistable value) {
        // Search for annotated fields and properties
        var type = value.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        StringBuilder buffer = new("{");
        buffer.Append($"\"type\":\"{mapper.TypeToId(value)}\"");

        foreach (var element in fields.Cast<MemberInfo>().Concat(properties)) {
            PersistAttribute attr = (PersistAttribute)element.GetCustomAttribute(typeof(PersistAttribute), false);
            if (attr == null) {
                continue;
            }

            string propertyName = attr.Name ?? element.Name;
            if (propertyName == null) {
                throw new Exception("Could not get property name for element: " + element);
            }

            object propertyValue;
            if (element is FieldInfo) {
                propertyValue = ((FieldInfo)element).GetValue(value);
            } else {
                propertyValue = ((PropertyInfo)element).GetValue(value);
            }
            if (propertyValue == null) {
                continue;
            }

            string propertyJson;
            if (propertyValue is IPersistable) {
                propertyJson = ToJson((IPersistable)propertyValue);
                // } else if (propertyValue is string) {
                //     propertyJson = Json.Stringify((string)propertyValue);
            } else {
                Variant? variantValue = ToVariant(propertyValue);
                if (variantValue == null) {
                    throw new Exception($"Unsupported property type: {propertyValue.GetType()} for property: {element.Name}");
                }

                propertyJson = Json.Stringify((Variant)variantValue);
            }

            buffer.Append($",\"{propertyName}\":{propertyJson}");
        }

        return buffer.Append('}').ToString();
    }

    private Variant? ToVariant(object value) {
        var valueType = value.GetType();
        foreach (var method in typeof(Variant).GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            if (!method.Name.StartsWith("CreateFrom")) {
                continue;
            }

            var variantType = method.GetParameters()[0].ParameterType;
            if (variantType.IsAssignableFrom(valueType)) {
                return (Variant)method.Invoke(null, new object[] { value });
            }
        }

        return null;
    }
}