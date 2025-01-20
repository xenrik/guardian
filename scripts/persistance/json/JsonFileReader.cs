using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Reflection;

public class JsonFileReader : IReader {
    private string filename;
    private ITypeMapper mapper;

    private bool readElement;

    private FileAccess file;

    public JsonFileReader(string filename, ITypeMapper mapper) {
        this.filename = filename;
        this.mapper = mapper;

        file = FileAccess.Open("user://" + filename, FileAccess.ModeFlags.Read);
    }

    public void Close() {
        file.Close();
        file = null;
    }

    public IPersistable Read() {
        if (file == null) {
            throw new Exception("Reader is closed");
        } else if (readElement) {
            throw new Exception("JsonFileReader cannot be used to read multiple values");
        }

        readElement = true;
        var parsed = Json.ParseString(file.GetAsText());
        if (parsed.Obj == null) {
            throw new Exception("Failed to read file as json: " + filename);
        } else if (parsed.Obj is Dictionary dict) {
            return ParseObject(dict);
        } else {
            throw new Exception("Unsupported type: " + parsed.Obj + " found when parsing: " + filename);
        }
    }

    private IPersistable ParseObject(Dictionary dict) {
        if (!dict.ContainsKey("type")) {
            throw new Exception("Missing type");
        }

        string typeId = (string)dict["type"];
        Type type = mapper.IdToType(typeId);
        if (type == null) {
            throw new Exception("Unknown type id: " + typeId);
        }

        IPersistable value = (IPersistable)Activator.CreateInstance(type);

        // Search for annotated fields and properties
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var element in fields.Cast<MemberInfo>().Concat(properties)) {
            PersistAttribute attr = (PersistAttribute)element.GetCustomAttribute(typeof(PersistAttribute), false);
            if (attr == null) {
                continue;
            }

            string propertyName = attr.Name ?? element.Name;
            if (propertyName == null) {
                throw new Exception("Could not get property name for element: " + element);
            } else if (!dict.ContainsKey(propertyName)) {
                continue;
            }

            Variant parsedValue = dict[propertyName];
            object propertyValue = null;
            Type propertyType = (element is FieldInfo) ? ((FieldInfo)element).FieldType : ((PropertyInfo)element).PropertyType;
            if (propertyType.IsAssignableTo(typeof(IPersistable))) {
                if (parsedValue.Obj is Dictionary propertyDict) {
                    propertyValue = ParseObject(propertyDict);
                } else {
                    throw new Exception("Could not parse property: " + propertyName + " - unexpected type: " + parsedValue.Obj.GetType());
                }
            } else {
                try {
                    propertyValue = Convert.ChangeType(parsedValue.Obj, propertyType);
                } catch (Exception ex) {
                    throw new Exception("Exception while converting property: " + propertyName, ex);
                }
            }

            try {
                if (element is FieldInfo) {
                    ((FieldInfo)element).SetValue(value, propertyValue);
                } else {
                    ((PropertyInfo)element).SetValue(value, propertyValue);
                }
            } catch (Exception ex) {
                throw new Exception("Exception while setting property: " + propertyName, ex);
            }
        }

        return value;
    }
}