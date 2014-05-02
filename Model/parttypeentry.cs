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

    public override String ToString()
    {
        return name;
    }
}
