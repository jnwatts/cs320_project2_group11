using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class DbModel
{
    public delegate void ResultHandler(DataTable result, string message, Exception exception);
    public delegate void PartTypesUpdatedHandler(List<PartTypeEntry> partTypes);
    public delegate void PartUpdatedHandler(PartEntry partEntry);

    public string Username { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string Database { get; set; }
    public bool Pooling { get; set; }

    private MySqlConnection _con = null;
    private List<PartTypeEntry> _partTypes = null;

    public DbModel()
    {
        Username = "";
        Password = "";
        Hostname = "";
        Database = "";
        Pooling = false;

        _partTypes = new List<PartTypeEntry>();
    }

    public void GetPartTypes(PartTypesUpdatedHandler handler)
    {
        string sql = "SELECT Part_type_id, Type FROM Part_types;";
        string errMsg = Execute(sql, delegate(DataTable result, string message, Exception exception) {
            _partTypes.Clear();
            List<PartTypeEntry> partTypes = new List<PartTypeEntry>();
            foreach (DataRow row in result.Rows) {
                PartTypeEntry entry = new PartTypeEntry((int)row["Part_type_id"], (string)row["Type"]);
                partTypes.Add(entry);
                _partTypes.Add(new PartTypeEntry(entry));
            }
            handler(partTypes);
        });
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public string GetPartType(int Part_type_id) {
        string type = "";
        _partTypes.ForEach(delegate (PartTypeEntry entry) {
            if (entry.typeId == Part_type_id) {
                type = entry.name;
            }
        });
        return type;
    }

    public void GetParts(PartTypeEntry partType, ResultHandler handler)
    {
        string sql = "SELECT * FROM Parts AS P NATURAL LEFT JOIN " + partType.name + "_attributes AS A NATURAL LEFT JOIN Part_types AS T WHERE T.Part_type_id = " + partType.typeId;
        string errMsg = Execute(sql, handler);
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execut query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public void GetPart(string Part_num, PartUpdatedHandler handler)
    {
        string sql = "SELECT * FROM Parts WHERE Part_num = '" + Part_num + "'";
        string errMsg = Execute(sql, delegate(DataTable attributeResult, string message, Exception exception) {
            if (attributeResult.Rows.Count > 0) {
                int typeId = (int)attributeResult.Rows[0]["Part_type_id"];
                string typeName = GetPartType(typeId);
                sql = "SELECT * FROM " + typeName + "_attributes WHERE Part_num = '" + Part_num + "'";
                errMsg = Execute(sql, delegate(DataTable extendedResult, string extendedMessage, Exception extendedException) {
                    PartEntry partEntry = new PartEntry(Part_num, typeId, typeName, attributeResult, extendedResult);
                    handler(partEntry);
                });
                if (errMsg != null) {
                    Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
                    errMsg = null;
                }
            } else {
                // Part doesn't exist, call handler with null
                handler(null);
            }
        });
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public void NewPart(PartTypeEntry partType)
    {
        string sql = "SELECT Part_num FROM Parts WHERE Part_type_id = " + partType.typeId + " ORDER BY Part_num DESC LIMIT 1";
        string errMsg = Execute(sql, delegate(DataTable partNumResult, string message, Exception exception) {
            if (partNumResult.Rows.Count > 0) {
                DataRow row = partNumResult.Rows[0];
                string Part_num = Util.PartNumberString(Util.PartNumberInteger((string)row["Part_num"]) + 1);
#if DEBUG
                Console.WriteLine("New part num: {0}", Part_num);
#endif
                sql = "INSERT INTO Parts (Part_num, Part_type_id) VALUES ('" + Part_num + "', " + partType.typeId + ")";
                errMsg = Execute(sql, null);
                if (errMsg != null) {
                    Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
                    errMsg = null;
                }
            }
        });
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public void UpdatePart(PartEntry part)
    {
        DataTable dt = null;
        DataRow row = null;
        MySqlCommand cmd = null;
        int modifiedColumns = 0;

        cmd = new MySqlCommand();
        cmd.Connection = _con;

        // Update parts table
        dt = part.Attributes;
        row = dt.Rows[0];
        modifiedColumns = 0;
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText = "UPDATE Parts SET";
        foreach (DataColumn col in dt.Columns) {
            if (col.DataType != typeof(string)) {
                // For now, all attributes are strings. Ignore numerics.
                continue;
            } else if (col.ColumnName == "Part_num") {
                // Don't change Part_num
                continue;
            }
            string original = (string)row[col, DataRowVersion.Original];
            string current = (string)row[col, DataRowVersion.Current];
            if (original != current) {
                cmd.CommandText += string.Format("{1}{0} = @{0}", col.ColumnName, (modifiedColumns++ > 0 ? ", " : " "));
                cmd.Parameters.AddWithValue(string.Format("@{0}", col.ColumnName), current);
            }
        }
        // Only finish the update if there was anything to update ;-)
        if (cmd.Parameters.Count > 0) {
            cmd.CommandText += " WHERE Part_num = @Part_num";
            cmd.Parameters.AddWithValue("@Part_num", part.Part_num);
#if DEBUG
            Console.WriteLine("{0}", cmd.CommandText);
            foreach (MySqlParameter p in cmd.Parameters) {
                Console.WriteLine(" {0} = {1}", p.ParameterName, p.Value);
            }
#endif
            cmd.ExecuteNonQuery();
        }

        // Update <type>_attributes
        dt = part.ExtendedAttributes;
        // Table name will be: string.Format("{0}_attributes", part.Part_type)
        // TODO: Duplicate above to generate command and execute it
    }

    public void DeletePart(string Part_num)
    {
        string sql = "DELETE FROM Parts WHERE Part_num = '" + Part_num + "'";
        string errMsg = Execute(sql, null);
        if (errMsg != null) {
            Console.WriteLine("Warning: Failed to execute query '{0}'. Error returned: {1}", sql, errMsg);
        }
    }

    public string Execute(string sql, ResultHandler handler)
    {
#if DEBUG
        Console.WriteLine("Execute: {0}", sql);
#endif
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
#if DEBUG
            dt.TableName = "Result";
            using(var writer = new System.IO.StringWriter()) {
                dt.WriteXml(writer);
                Console.WriteLine(writer.ToString());
            }
#endif
            message = String.Format("Returned {0} rows", dt.Rows.Count);
        } catch (Exception e) {
            result = e.Message;
            message = result;
            exception = e;
        }
        if (handler != null) {
            handler(dt, message, exception);
        }
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
