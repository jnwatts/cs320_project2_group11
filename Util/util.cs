using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;

public class Util
{
    private Util()
    {
        // Not accessible
    }

    public static string GetColumnString(DataTable dt, string name, string defaultValue)
    {
        string val = defaultValue;
        DataRow[] rows = dt.Select("Name = '" + name + "'");
        if (rows.Length > 0) {
            val = (string)rows[0]["Value"];
        }
        return val;
    }

    public static bool GetColumnBool(DataTable dt, string name, bool defaultValue)
    {
        bool val = defaultValue;
        DataRow[] rows = dt.Select("Name = '" + name + "'");
        if (rows.Length > 0) {
            bool success = false;
            val = Boolean.TryParse((string)rows[0]["Value"], out success);
            if (!success)
                val = defaultValue;
        }
        return val;
    }

    public static void FixTableLayoutStyles(TableLayoutPanel panel) {
        // Work around weird bug w/ last row/column's SizeType.AutoSize growing but not shrinking
        panel.ColumnStyles.Clear();
        for (int i = 0; i < panel.ColumnCount; ++i) {
            if (i < panel.ColumnCount - 1)
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            else
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100.0f));
        }

        panel.RowStyles.Clear();
        for (int i = 0; i < panel.RowCount; ++i) {
            if (i < panel.RowCount - 1)
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            else
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100.0f));
        }
    }

    public static void FillAttributes(DataRow row, Dictionary<string, string> attributes)
    {
        attributes.Clear();
        foreach(DataColumn col in row.Table.Columns) {
            if (col.ColumnName == "Part_num") {
                continue;
            }
            if (col.DataType != System.Type.GetType("System.String")) {
                Console.WriteLine("Warning: Ignoring column {0} due to non-string data-type: {1}", col, col.DataType);
                continue;
            }
            attributes[col.ColumnName] = (row.IsNull(col) ? null : (string)row[col]);
        }
    }

    public static DataTable AttributesToDataTable(string tableName, Dictionary<string, string> attributes)
    {
        DataTable dt = new DataTable();
        dt.TableName = tableName;
        foreach (KeyValuePair<string, string> entry in attributes) {
            dt.Columns.Add(new DataColumn(entry.Key, typeof(string)));
        }
        DataRow row = dt.NewRow();
        foreach (KeyValuePair<string, string> entry in attributes) {
            row[entry.Key] = entry.Value;
        }
        dt.Rows.Add(row);
        return dt;
    }

    public static int PartNumberInteger(string Part_num)
    {
        return int.Parse(Part_num.Replace("D3-", ""));
    }

    public static string PartNumberString(int Part_num)
    {
        return "D3-" + Part_num.ToString("D7");
    }

    public static string SqlCommandToString(IDbCommand cmd)
    {
        string str;
        if (cmd == null) {
            str = "null";
        } else {
            str = cmd.CommandText;
            foreach (IDataParameter p in cmd.Parameters) {
                if (p.DbType == DbType.String) {
                    str = str.Replace(p.ParameterName, string.Format("'{0}'", p.Value));
                } else {
                    str = str.Replace(p.ParameterName, p.Value.ToString());
                }
            }
        }
        return str;
    }
}
