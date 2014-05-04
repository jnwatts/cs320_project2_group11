using System;
using System.Data;
using System.Collections.Generic;

public class PartEntry
{
    public string Part_num { get; set; }
    public int Part_type_id { get; set; }
    public string Part_type { get; set; }
    public Dictionary<string, string> Attributes;
    public Dictionary<string, string> ExtendedAttributes;

    public PartEntry(string Part_num, int Part_type_id, string Part_type, DataRow attributesRow, DataRow extendedAttributesRow)
    {
        Attributes = new Dictionary<string, string>();
        ExtendedAttributes = new Dictionary<string, string>();

        this.Part_num = Part_num;
        this.Part_type_id = Part_type_id;
        this.Part_type = Part_type;
        Util.FillAttributes(attributesRow, Attributes);
        if (extendedAttributesRow != null) {
            Util.FillAttributes(extendedAttributesRow, ExtendedAttributes);
        }
    }

    public PartEntry(PartEntry other)
    {
        this.Part_num = other.Part_num;
        this.Part_type_id = other.Part_type_id;
        this.Part_type = other.Part_type;
        this.Attributes = new Dictionary<string, string>(other.Attributes);
        this.ExtendedAttributes = new Dictionary<string, string>(other.ExtendedAttributes);
    }

    public DataTable AttributesDataTable()
    {
        return Util.AttributesToDataTable("Attributes", Attributes);
    }

    public DataTable ExtendedAttributesDataTable()
    {
        return Util.AttributesToDataTable("ExtendedAttributes", ExtendedAttributes);
    }

    public override String ToString()
    {
        string str = "{" + Part_num;
        foreach (KeyValuePair<string, string> entry in Attributes) {
            str += ", " + entry.Key + ": " + entry.Value;
        }
        foreach (KeyValuePair<string, string> entry in ExtendedAttributes) {
            str += ", " + entry.Key + ": " + entry.Value;
        }
        str += "}";
        return str;
    }
}
