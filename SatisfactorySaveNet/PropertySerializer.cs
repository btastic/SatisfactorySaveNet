﻿using SatisfactorySaveNet.Abstracts;
using SatisfactorySaveNet.Abstracts.Maths.Vector;
using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;
using SatisfactorySaveNet.Abstracts.Model.TypedData;
using SatisfactorySaveNet.Abstracts.Model.Union;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SatisfactorySaveNet;

public class PropertySerializer : IPropertySerializer
{
    public static readonly IPropertySerializer Instance = new PropertySerializer(StringSerializer.Instance, ObjectReferenceSerializer.Instance, VectorSerializer.Instance, HexSerializer.Instance);

    private readonly IStringSerializer _stringSerializer;
    private readonly IObjectReferenceSerializer _objectReferenceSerializer;
    private readonly ITypedDataSerializer _typedDataSerializer;
    private readonly IHexSerializer _hexSerializer;

    internal PropertySerializer(IStringSerializer stringSerializer, IObjectReferenceSerializer objectReferenceSerializer, ITypedDataSerializer typedDataSerializer, IHexSerializer hexSerializer)
    {
        _stringSerializer = stringSerializer;
        _objectReferenceSerializer = objectReferenceSerializer;
        _typedDataSerializer = typedDataSerializer;
        _hexSerializer = hexSerializer;
    }

    public PropertySerializer(IStringSerializer stringSerializer, IObjectReferenceSerializer objectReferenceSerializer, IVectorSerializer vectorSerializer, IHexSerializer hexSerializer)
    {
        _stringSerializer = stringSerializer;
        _objectReferenceSerializer = objectReferenceSerializer;
        _typedDataSerializer = new TypedDataSerializer(vectorSerializer, stringSerializer, this);
        _hexSerializer = hexSerializer;
    }

    public IEnumerable<Property> DeserializeProperties(BinaryReader reader, Header? header = null)
    {
        Property? property;
        while ((property = DeserializeProperty(reader, header)) != null)
            yield return property;
    }

    [return: NotNullIfNotNull(nameof(type))]
    public Property? DeserializeProperty(BinaryReader reader, Header? header = null, string? type = null)
    {
        var propertyName = string.Empty;

        if (type == null)
        {
            propertyName = _stringSerializer.Deserialize(reader);

            if (string.Equals(propertyName, "None", StringComparison.Ordinal))
                return null;

            type = _stringSerializer.Deserialize(reader);
        }

        return type switch
        {
            nameof(ArrayProperty) => DeserializeArrayProperty(reader),
            nameof(BoolProperty) => DeserializeBoolProperty(reader),
            nameof(ByteProperty) => DeserializeByteProperty(reader),
            nameof(EnumProperty) => DeserializeEnumProperty(reader),
            nameof(FloatProperty) => DeserializeFloatProperty(reader),
            nameof(IntProperty) => DeserializeIntProperty(reader),
            nameof(Int64Property) => DeserializeInt64Property(reader),
            nameof(MapProperty) => header == null ? throw new ArgumentNullException(nameof(header)) : DeserializeMapProperty(reader, header, propertyName, type),
            nameof(NameProperty) => DeserializeNameProperty(reader),
            nameof(ObjectProperty) => DeserializeObjectProperty(reader),
            nameof(SetProperty) => DeserializeSetProperty(reader),
            nameof(StrProperty) => DeserializeStrProperty(reader),
            nameof(StructProperty) => DeserializeStructProperty(reader),
            nameof(TextProperty) => DeserializeTextProperty(reader),
            nameof(UInt32Property) => DeserializeUInt32Property(reader),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private IArrayProperty DeserializeArrayProperty(BinaryReader reader, string type, int count)
    {
        return type switch
        {
            //nameof(ArrayProperty) => DeserializeArrayProperty(reader, count),
            nameof(StructProperty) => DeserializeArrayStructProperty(reader, count),
            nameof(BoolProperty) => DeserializeBoolArrayProperty(reader, count),
            nameof(ByteProperty) => DeserializeArrayByteProperty(reader, count),
            nameof(EnumProperty) => DeserializeArrayEnumProperty(reader, count),
            nameof(FloatProperty) => DeserializeArrayFloatProperty(reader, count),
            nameof(IntProperty) => DeserializeArrayIntProperty(reader, count),
            nameof(Int64Property) => DeserializeArrayInt64Property(reader, count),
            //nameof(MapProperty) => DeserializeMapProperty(reader, count),
            //nameof(NameProperty) => DeserializeNameProperty(reader, count),
            nameof(ObjectProperty) => DeserializeArrayObjectProperty(reader, count),
            //nameof(SetProperty) => DeserializeSetProperty(reader, count),
            nameof(StrProperty) => DeserializeArrayStrProperty(reader, count),
            //nameof(StructProperty) => DeserializeStructProperty(reader, count),
            nameof(TextProperty) => DeserializeArrayTextProperty(reader, count),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private ArrayTextProperty DeserializeArrayTextProperty(BinaryReader reader, int count)
    {
        var flags = reader.ReadInt32();
        var historyType = reader.ReadSByte();

        string? nameSpace = null;
        string? key = null;
        string? value = null;
        TextProperty? sourceFmt = null;

        switch (historyType)
        {
            case 0:
                nameSpace = _stringSerializer.Deserialize(reader);
                key = _stringSerializer.Deserialize(reader);
                value = _stringSerializer.Deserialize(reader);
                break;
            case 1:
            case 3:
                sourceFmt = DeserializeTextProperty(reader);
                var argumentsCount = reader.ReadInt32();

                for (var x = 0; x < argumentsCount; x++)
                {

                }
                break;
        }

        for (var x = 0; x < count; x++)
        {
        }

        return new ArrayTextProperty
        {
        };
    }

    private ArrayStrProperty DeserializeArrayStrProperty(BinaryReader reader, int count)
    {
        var values = new string[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = _stringSerializer.Deserialize(reader);
        }

        return new ArrayStrProperty
        {
            Values = values
        };
    }

    private ArrayEnumProperty DeserializeArrayEnumProperty(BinaryReader reader, int count)
    {
        var values = new string[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = _stringSerializer.Deserialize(reader);
        }

        return new ArrayEnumProperty
        {
            Values = values
        };
    }

    private static ArrayFloatProperty DeserializeArrayFloatProperty(BinaryReader reader, int count)
    {
        var values = new float[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = reader.ReadSingle();
        }

        return new ArrayFloatProperty
        {
            Values = values
        };
    }

    private static ArrayInt64Property DeserializeArrayInt64Property(BinaryReader reader, int count)
    {
        var values = new long[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = reader.ReadInt64();
        }

        return new ArrayInt64Property
        {
            Values = values
        };
    }

    private static ArrayIntProperty DeserializeArrayIntProperty(BinaryReader reader, int count)
    {
        var values = new int[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = reader.ReadInt32();
        }

        return new ArrayIntProperty
        {
            Values = values
        };
    }

    private static ArrayBoolProperty DeserializeBoolArrayProperty(BinaryReader reader, int count)
    {
        var values = new bool[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = reader.ReadSByte() != 0;
        }

        return new ArrayBoolProperty
        {
            Values = values
        };
    }

    private static ArrayByteProperty DeserializeArrayByteProperty(BinaryReader reader, int count)
    {
        var values = new sbyte[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = reader.ReadSByte();
        }

        return new ArrayByteProperty
        {
            Values = values
        };
    }

    private ArrayObjectProperty DeserializeArrayObjectProperty(BinaryReader reader, int count)
    {
        var values = new ObjectReference[count];

        for (var x = 0; x < count; x++)
        {
            values[x] = _objectReferenceSerializer.Deserialize(reader);
        }

        return new ArrayObjectProperty
        {
            Values = values
        };
    }

    private ArrayProperty DeserializeArrayProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var type = _stringSerializer.Deserialize(reader);
        var padding = reader.ReadSByte();
        var length = reader.ReadInt32();

        var property = DeserializeArrayProperty(reader, type, length);

        return new ArrayProperty(property)
        {
            Index = index,
            Type = type
        };
    }

    private TextProperty DeserializeTextProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var flags = reader.ReadInt32();
        var historyType = reader.ReadSByte();
        var isCultureInvariant = reader.ReadInt32() != 0;
        var value = _stringSerializer.Deserialize(reader);

        return new TextProperty
        {
            Index = index,
            Flags = flags,
            HistoryType = historyType,
            IsCultureInvariant = isCultureInvariant,
            Value = value,
        };
    }

    private ArrayStructProperty DeserializeArrayStructProperty(BinaryReader reader, int length)
    {
        var propertyName = _stringSerializer.Deserialize(reader);
        var propertyType = _stringSerializer.Deserialize(reader);

        var binarySize = reader.ReadInt32();
        var startPosition = reader.BaseStream.Position;
        var padding1 = reader.ReadInt32();
        var elementType = _stringSerializer.Deserialize(reader);
        var uuid1 = reader.ReadInt32();
        var uuid2 = reader.ReadInt32();
        var uuid3 = reader.ReadInt32();
        var uuid4 = reader.ReadInt32();
        var padding2 = reader.ReadSByte();

        var endPosition = startPosition + binarySize;

        var values = new ITypedData[length];

        for (var x = 0; x < length; x++)
        {
            values[x] = _typedDataSerializer.Deserialize(reader, elementType, endPosition);
        }

        var property = new ArrayStructProperty
        {
            PropertyName = propertyName,
            PropertyType = propertyType,
            ElementType = elementType,
            UUID = (uuid1, uuid2, uuid3, uuid4),
            Values = values
        };

        return property;
    }

    private static BoolProperty DeserializeBoolProperty(BinaryReader reader)
    {
        var padding = reader.ReadInt32();
        var index = reader.ReadInt32();
        var value = reader.ReadSByte() != 0;
        var padding2 = reader.ReadSByte();

        var property = new BoolProperty
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private ByteProperty DeserializeByteProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var type = _stringSerializer.Deserialize(reader);
        var padding = reader.ReadSByte();

        sbyte? byteData = null;
        string? stringData = null;

        if (string.Equals(type, "None", StringComparison.Ordinal))
            byteData = reader.ReadSByte();
        else
            stringData = _stringSerializer.Deserialize(reader);

        var property = new ByteProperty
        {
            Index = index,
            Type = type,
            ByteData = byteData,
            StringData = stringData
        };

        return property;
    }

    private EnumProperty DeserializeEnumProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var type = _stringSerializer.Deserialize(reader);
        var padding = reader.ReadSByte();
        var value = _stringSerializer.Deserialize(reader);

        var property = new EnumProperty
        {
            Index = index,
            Type = type,
            Value = value
        };

        return property;
    }

    private static FloatProperty DeserializeFloatProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = reader.ReadSingle();

        var property = new FloatProperty
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private static IntProperty DeserializeIntProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = reader.ReadInt32();

        var property = new IntProperty
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private static UInt32Property DeserializeUInt32Property(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = reader.ReadUInt32();

        var property = new UInt32Property
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private static Int64Property DeserializeInt64Property(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = reader.ReadInt64();

        var property = new Int64Property
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private MapProperty DeserializeMapProperty(BinaryReader reader, Header header, string? propertyName = null, string? type = null)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var keyType = _stringSerializer.Deserialize(reader);
        var valueType = _stringSerializer.Deserialize(reader);
        var padding = reader.ReadSByte();
        var modeType = reader.ReadInt32();

        if (modeType == 2)
        {
            var disc1 = _stringSerializer.Deserialize(reader);
            var disc2 = _stringSerializer.Deserialize(reader);
        }
        else if (modeType == 3)
        {
            var disc1 = _hexSerializer.Deserialize(reader, 9);
            var disc2 = _stringSerializer.Deserialize(reader);
            var disc3 = _stringSerializer.Deserialize(reader);
        }

        var count = reader.ReadInt32();

        var property = new MapProperty(new Dictionary<Property, Property>(count))
        {
            Index = index,
            KeyType = keyType,
            ValueType = valueType,
            ModeType = modeType
        };

        var dict = new Dictionary<Union, Union>(count);
        object key = null!;

        for (var i = 0; i < count; i++)
        {
            switch (property.KeyType)
            {
                case nameof(StructProperty):
                    if (string.Equals(propertyName, "Destroyed_Foliage_Transform", StringComparison.Ordinal))
                    {
                        key = header.SaveVersion >= 41
                            ? new Vector3D(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble())
                            : new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;
                    }
                    else if (string.Equals(type, "/BuildGunUtilities/BGU_Subsystem.BGU_Subsystem_C", StringComparison.Ordinal))
                    {
                        key = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }
                    else if (string.Equals(propertyName, "mSaveData", StringComparison.Ordinal) || string.Equals(propertyName, "mUnresolvedSaveData", StringComparison.Ordinal))
                    {
                        key = new Vector3I(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    }
                    break;
            }
            //var key = DeserializeProperty(reader, keyType);
            //var value = DeserializeProperty(reader, valueType);

            //if (value == null)
            //    continue;
            //
            //property.Elements.Add(key, value);
        }

        return property;
    }

    private NameProperty DeserializeNameProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = _stringSerializer.Deserialize(reader);

        var property = new NameProperty
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private ObjectProperty DeserializeObjectProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = _objectReferenceSerializer.Deserialize(reader);

        var property = new ObjectProperty(value)
        {
            Index = index
        };

        return property;
    }

    private SetProperty DeserializeSetProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var type = _stringSerializer.Deserialize(reader);
        var padding = reader.ReadSByte();
        var padding2 = reader.ReadInt32();
        var count = reader.ReadInt32();

        var property = new SetProperty
        {
            Index = index,
            Type = type,
            Elements = new List<Property>(count)
        };

        for (var i = 0; i < count; i++)
        {
            var value = DeserializeProperty(reader);

            if (value == null)
                continue;

            property.Elements.Add(value);
        }

        return property;
    }

    private StrProperty DeserializeStrProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var index = reader.ReadInt32();
        var padding = reader.ReadSByte();
        var value = _stringSerializer.Deserialize(reader);

        var property = new StrProperty
        {
            Index = index,
            Value = value
        };

        return property;
    }

    private StructProperty DeserializeStructProperty(BinaryReader reader)
    {
        var binarySize = reader.ReadInt32();
        var startPosition = reader.BaseStream.Position;
        var index = reader.ReadInt32();
        var type = _stringSerializer.Deserialize(reader);
        var padding1 = reader.ReadInt64();
        var padding2 = reader.ReadInt64();
        var padding3 = reader.ReadSByte();
        var typedData = _typedDataSerializer.Deserialize(reader, type, startPosition + binarySize);

        var property = new StructProperty(typedData)
        {
            Index = index,
            Type = type
        };

        return property;
    }
}
