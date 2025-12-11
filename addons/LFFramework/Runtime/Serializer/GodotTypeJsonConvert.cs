using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GDLog;
using Godot;

namespace LF;

public class GodotTypeJsonConvert<[MustBeVariant] T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        GLog.Info(str);
        if (str == null)
        {
            return default;
        }
        
        var v = Json.ToNative(Json.ParseString(str));
        if (v.VariantType == Variant.Type.Nil)
        {
            return default;
        }
        return (T)v.Obj;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var v = Variant.From(value);
        if (v.VariantType == Variant.Type.Nil)
        {
            writer.WriteNullValue();
            return;
        }
        var json = Json.FromNative(v).AsString();
        writer.WriteStringValue(json); // godot 序列化的json会带双引号和C#写入的双引号重复了
    }
}