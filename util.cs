using System;
using System.Data;

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
}
