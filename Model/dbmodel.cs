using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class DbModel
{
    public delegate void ResultHandler(DataTable result, string message, Exception exception);

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

    public List<PartTypeEntry> PartTypes()
    {
        List<PartTypeEntry> parttypes = new List<PartTypeEntry>();
        //TODO: Oops... need to split to asynchronous. One func to get, and handler to consume. Move this to app or something, and use an async handler.
        parttypes.Add(new PartTypeEntry(0, "Test value"));
        return parttypes;
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
