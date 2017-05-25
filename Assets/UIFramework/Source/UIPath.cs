using System;

/// <summary>
/// 一个Ui属性标志特性类
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class UIPath : Attribute
{
    public readonly string Path;
    public UIPath(string uIpath)
    {
        Path = uIpath;
    }
}
