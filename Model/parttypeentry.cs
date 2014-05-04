using System;
using System.Data;

public class PartTypeEntry
{
    public int typeId { get; set; }
    public String name { get; set; }

    public PartTypeEntry(int typeId, String name)
    {
        this.typeId = typeId;
        this.name = name;
    }

    public PartTypeEntry(PartTypeEntry other)
    {
        this.typeId = other.typeId;
        this.name = other.name;
    }

    public override String ToString()
    {
        return name;
    }
}
