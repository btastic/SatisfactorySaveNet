﻿using System.Collections.Generic;
namespace SatisfactorySaveNet.Abstracts.Model.Properties;

public class ArrayByteProperty : IArrayProperty
{
    public IList<sbyte> Values { get; set; } = [];
}
