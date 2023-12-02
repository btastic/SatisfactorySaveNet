﻿namespace SatisfactorySaveNet.Abstracts.Model.Properties;

public class ByteProperty : Property
{
    public string Type { get; set; } = string.Empty;
    public sbyte? ByteData { get; set; }
    public string? StringData { get; set; }
}