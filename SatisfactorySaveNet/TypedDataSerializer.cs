﻿using SatisfactorySaveNet.Abstracts;
using SatisfactorySaveNet.Abstracts.Model.Properties;
using SatisfactorySaveNet.Abstracts.Model.TypedData;
using System;
using System.IO;
using System.Linq;

namespace SatisfactorySaveNet;

public class TypedDataSerializer : ITypedDataSerializer
{
    public static readonly ITypedDataSerializer Instance = new TypedDataSerializer(VectorSerializer.Instance, StringSerializer.Instance, ObjectReferenceSerializer.Instance, HexSerializer.Instance);

    private readonly IVectorSerializer _vectorSerializer;
    private readonly IStringSerializer _stringSerializer;
    private readonly IPropertySerializer _propertySerializer;

    internal TypedDataSerializer(IVectorSerializer vectorSerializer, IStringSerializer stringSerializer, IPropertySerializer propertySerializer)
    {
        _vectorSerializer = vectorSerializer;
        _stringSerializer = stringSerializer;
        _propertySerializer = propertySerializer;
    }

    public TypedDataSerializer(IVectorSerializer vectorSerializer, IStringSerializer stringSerializer, IObjectReferenceSerializer objectReferenceSerializer, IHexSerializer hexSerializer)
    {
        _vectorSerializer = vectorSerializer;
        _stringSerializer = stringSerializer;
        _propertySerializer = new PropertySerializer(stringSerializer, objectReferenceSerializer, this, hexSerializer);
    }

    public ITypedData Deserialize(BinaryReader reader, string type, long endPosition)
    {
        return type switch
        {
            nameof(Box) => DeserializeBox(reader),
            nameof(FactoryCustomizationColorSlot) => DeserializeFactoryCustomizationColorSlot(reader, endPosition),
            nameof(FluidBox) => DeserializeFluidBox(reader),
            nameof(InventoryItem) => DeserializeInventoryItem(reader, endPosition),
            nameof(InventoryStack) => DeserializeInventoryStack(reader),
            nameof(LinearColor) => DeserializeLinearColor(reader),
            nameof(Quat) => DeserializeQuat(reader),
            nameof(RailroadTrackPosition) => DeserializeRailroadTrackPosition(reader),
            nameof(SpawnData) => DeserializeSpawnData(reader),
            nameof(Vector) => DeserializeVector(reader),
            //"" => DeserializeProperty(reader),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    //private ITypedData DeserializeProperty(BinaryReader reader)
    //{
    //    //ToDo: Das hier ist evtl. BS
    //    var value = _propertySerializer.DeserializeProperty(reader);
    //
    //    return new PropertyData
    //    {
    //        Value = value
    //    };
    //}

    private Vector DeserializeVector(BinaryReader reader)
    {
        var value = _vectorSerializer.DeserializeVec3(reader);

        return new Vector
        {
            Value = value
        };
    }

    private SpawnData DeserializeSpawnData(BinaryReader reader)
    {
        var properties = _propertySerializer.DeserializeProperties(reader).ToArray();

        return new SpawnData
        {
            Properties = properties
        };
    }

    private RailroadTrackPosition DeserializeRailroadTrackPosition(BinaryReader reader)
    {
        var levelName = _stringSerializer.Deserialize(reader);
        var pathName = _stringSerializer.Deserialize(reader);
        var offset = reader.ReadSingle();
        var forward = reader.ReadSingle();

        return new RailroadTrackPosition
        {
            LevelName = levelName,
            PathName = pathName,
            Offset = offset,
            Forward = forward
        };
    }

    private Quat DeserializeQuat(BinaryReader reader)
    {
        var value = _vectorSerializer.DeserializeQuaternion(reader);

        return new Quat
        {
            Value = value
        };
    }

    private LinearColor DeserializeLinearColor(BinaryReader reader)
    {
        var color = _vectorSerializer.DeserializeVec4(reader);

        return new LinearColor
        {
            Color = color
        };
    }

    private InventoryStack DeserializeInventoryStack(BinaryReader reader)
    {
        var properties = _propertySerializer.DeserializeProperties(reader).ToArray();

        return new InventoryStack
        {
            Properties = properties
        };
    }

    private InventoryItem DeserializeInventoryItem(BinaryReader reader, long endPosition)
    {
        var padding = reader.ReadInt32();
        var itemType = _stringSerializer.Deserialize(reader);
        var levelName = _stringSerializer.Deserialize(reader);
        var pathName = _stringSerializer.Deserialize(reader);
        Property? property = null;

        if (reader.BaseStream.Position != endPosition)
            property = _propertySerializer.DeserializeProperty(reader);

        return new InventoryItem
        {
            ItemType = itemType,
            LevelName = levelName,
            PathName = pathName,
            ExtraProperty = property
        };
    }

    private static FluidBox DeserializeFluidBox(BinaryReader reader)
    {
        var value = reader.ReadSingle();

        return new FluidBox
        {
            Value = value
        };
    }

    private FactoryCustomizationColorSlot DeserializeFactoryCustomizationColorSlot(BinaryReader reader, long endPosition)
    {
        var properties = _propertySerializer.DeserializeProperties(reader).ToArray();

        return new FactoryCustomizationColorSlot
        {
            Properties = properties
        };
    }

    private Box DeserializeBox(BinaryReader reader)
    {
        var min = _vectorSerializer.DeserializeVec3(reader);
        var max = _vectorSerializer.DeserializeVec3(reader);
        var isValid = reader.ReadSByte() != 0;

        return new Box
        {
            Min = min,
            Max = max,
            IsValid = isValid
        };
    }
}
