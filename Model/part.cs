using System;
using System.Data;
using System.Collections.Generic;

public class Part
{
    public string Part_num { get; set; }
    public int Part_type_id { get; set; }
    public string Part_type { get; set; }
    public DataTable Attributes;
    public DataTable ExtendedAttributes;

    public Part(string Part_num, int Part_type_id, string Part_type, DataTable attributes, DataTable extendedAttributes)
    {
        this.Part_num = Part_num;
        this.Part_type_id = Part_type_id;
        this.Part_type = Part_type;
        this.Attributes = attributes;
        this.ExtendedAttributes = extendedAttributes;
    }

    public Part(Part other)
    {
        this.Part_num = other.Part_num;
        this.Part_type_id = other.Part_type_id;
        this.Part_type = other.Part_type;
        this.Attributes = other.Attributes.Copy();
        this.ExtendedAttributes = other.ExtendedAttributes.Copy();
    }

    public override String ToString()
    {
        string str = "{";
        DataRow row = null;

        str += Part_num;
        str += ", Type: " + Part_type;
        
        if (Attributes.Rows.Count > 0) {
            row = Attributes.Rows[0];
            foreach (DataColumn col in Attributes.Columns) {
                str += ", " + col.ColumnName + ": " + (string)row[col];
            }
        }

        if (ExtendedAttributes.Rows.Count > 0) {
            row = ExtendedAttributes.Rows[0];
            foreach (DataColumn col in ExtendedAttributes.Columns) {
                str += ", " + col.ColumnName + ": " + (string)row[col];
            }
        }

        str += "}";
        return str;
    }
}
