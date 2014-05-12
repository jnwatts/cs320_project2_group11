using System;
using System.Data;

public class PartType
{
    public int typeId { get; set; }
    public String name { get; set; }

    public PartType(int typeId, String name)
    {
        this.typeId = typeId;
        this.name = name;
    }

    public PartType(PartType other)
    {
        this.typeId = other.typeId;
        this.name = other.name;
    }

    public override String ToString()
    {
        return name;
    }
}
