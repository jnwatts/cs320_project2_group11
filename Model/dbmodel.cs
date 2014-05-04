using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class DbModel
{
    public delegate void ResultHandler(DataTable result, string message, Exception exception);
    public delegate void PartTypesUpdatedHandler(List<PartTypeEntry> partTypes);

    public string Username { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string Database { get; set; }
    public bool Pooling { get; set; }

    private MySqlConnection _con = null;

    public DbModel()
    {
        Username = "";
        Password = "";
        Hostname = "";
        Database = "";
        Pooling = false;
    }

    public void GetPartTypes(PartTypesUpdatedHandler handler)
    {
        string sql = "SELECT Part_type_id, Type FROM Part_types;";
        string errMsg = Execute(sql, delegate(DataTable result, string message, Exception exception) {
            List<PartTypeEntry> partTypes = new List<PartTypeEntry>();
            foreach (DataRow row in result.Rows) {
                PartTypeEntry entry = new PartTypeEntry((int)row["Part_type_id"], (string)row["Type"]);
                partTypes.Add(entry);
            }
            handler(partTypes);
        });
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public void GetParts(PartTypeEntry partType, ResultHandler handler)
    {
        string sql = "SELECT * FROM Parts AS P NATURAL LEFT JOIN " + partType.name + "_attributes AS A NATURAL LEFT JOIN Part_types AS T WHERE T.Part_type_id = " + partType.typeId;
        string errMsg = Execute(sql, handler);
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execut query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public string Execute(string sql, ResultHandler handler)
    {
        string result = null;
        string message = null;
        DataTable dt = null;
        Exception exception = null;
        if ((result = Connect()) != null) {
            return result;
        }

        try {
            MySqlDataAdapter da = new MySqlDataAdapter(sql, _con);
            dt = new DataTable();
            da.Fill(dt);
            da.Dispose();
            da = null;
            message = String.Format("Returned {0} rows", dt.Rows.Count);
        } catch (Exception e) {
            result = e.Message;
            message = result;
            exception = e;
        }
        handler(dt, message, exception);
        return result;
    }

    private string Connect()
    {
        if (_con == null) {
            try {
                string connectionString =
                    "Server=" + Hostname + ";" +
                    "Database=" + Database + ";" +
                    "User ID=" + Username + ";" +
                    "Password=" + Password + ";" +
                    "Pooling=" + (Pooling ? "true" : "false");
                _con = new MySqlConnection(connectionString);
                _con.Open();
            } catch (Exception e) {
                return e.Message;
            }
        }
        return null;
    }
}