﻿using System.Collections.Generic;
namespace SatisfactorySaveNet.Abstracts.Model.Properties;

public class ArrayIntProperty : IArrayProperty
{
    public IList<int> Values { get; set; } = [];
}
