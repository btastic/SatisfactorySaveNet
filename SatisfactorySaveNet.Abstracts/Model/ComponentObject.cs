﻿using SatisfactorySaveNet.Abstracts.Model.ExtraData;
using SatisfactorySaveNet.Abstracts.Model.Properties;

namespace SatisfactorySaveNet.Abstracts.Model;

public class ComponentObject
{
    public const int TypeID = 0;

    public virtual int Type => TypeID;
    public string TypePath { get; set; } = string.Empty;
    public string RootObject { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;

    public string ParentActorName { get; set; } = string.Empty;

    public IList<Property> Properties { get; set; } = Array.Empty<Property>();
    public IExtraData? ExtraData { get; set; }
    public int? EntitySaveVersion { get; set; }
}