using System;
using System.Data;

//TODO: For now, I'm being lazy. In the future, this should be fleshed out to fully implement IList
public class PartCollection /* : IList */
{
    DataTable parts;

    public PartCollection()
    {
        parts = new DataTable("Parts");
    }

    public PartCollection(DataTable result)
    {
        parts = new DataTable("Parts");

        foreach (DataRow srcRow in result.Rows) {
            DataRow dstRow = parts.NewRow();
            foreach (DataColumn col in result.Columns) {
                if (!parts.Columns.Contains(col.ColumnName)) {
                    parts.Columns.Add(col.ColumnName, col.DataType);
                }
                dstRow[col.ColumnName] = srcRow[col.ColumnName];
            }
            parts.Rows.Add(dstRow);
        }
        parts.AcceptChanges();
    }

    public void Add(Part part)
    {
        DataRow row = parts.NewRow();
        foreach (DataColumn col in part.Attributes.Columns) {
            if (!parts.Columns.Contains(col.ColumnName)) {
                parts.Columns.Add(col.ColumnName, col.DataType);
            }
            row[col.ColumnName] = part.Attributes.Rows[0][col.ColumnName];
        }
        parts.Rows.Add(row);
        parts.AcceptChanges();
    }

    public void Remove(Part part)
    {
        DataRow[] rows = parts.Select("Part_num LIKE " + part.Part_num);
        foreach (DataRow row in rows) {
            row.Delete();
        }
        parts.AcceptChanges();
    }

    public DataTable DataSource
    {
        get {
            return parts;
        }
    }
}
