using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class MySqlPartsDb : PartsDb
{
    public delegate void ResultHandler(DataTable result);
    /*
    public string Username { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string Database { get; set; }
    */

    private MySqlConnection _con = null;

    public MySqlPartsDb()
    {
        Username = "";
        Password = "";
        Hostname = "";
        Database = "";
    }

    public override void GetPartTypes(PartTypesHandler partTypesHandler, ErrorHandler errorHandler)
    {
        string sql = "SELECT Part_type_id, Type FROM Part_types;";
        Execute(sql, delegate(DataTable result) {
            List<PartType> partTypes = new List<PartType>();
            foreach (DataRow row in result.Rows) {
                PartType entry = new PartType((int)row["Part_type_id"], (string)row["Type"]);
                partTypes.Add(entry);
            }
            if (partTypesHandler != null) {
                partTypesHandler(partTypes);
            }
        }, errorHandler);
    }

    public override void GetParts(PartType partType, PartsHandler partsHandler, ErrorHandler errorHandler)
    {
        string sql = "SELECT Part_num FROM Parts WHERE Part_type_id = " + partType.typeId;
        Execute(sql, delegate(DataTable result) {
            List<Part> parts = new List<Part>();
            foreach (DataRow row in result.Rows) {
                GetPart((string)row["Part_num"], delegate(Part part) {
                    parts.Add(part);
                }, null); // Ignore errors in GetPart() and continue
            }
            if (partsHandler != null) {
                partsHandler(parts);
            }
        }, errorHandler);
    }

    public override void GetPart(string Part_num, PartHandler handler, ErrorHandler errorHandler)
    {
        string sql = "SELECT * FROM Parts WHERE Part_num = '" + Part_num + "'";
        Execute(sql, delegate(DataTable attributeResult) {
            if (attributeResult.Rows.Count > 0) {
                int typeId = (int)attributeResult.Rows[0]["Part_type_id"];
                sql = "SELECT Type FROM Part_types WHERE Part_type_id = " + typeId;
                Execute(sql, delegate(DataTable typeResult) {
                    string typeName = (string)typeResult.Rows[0]["Type"];
                    sql = "SELECT * FROM " + typeName + "_attributes WHERE Part_num = '" + Part_num + "'";
                    Execute(sql, delegate(DataTable extendedResult) {
                        Part part = new Part(Part_num, typeId, typeName, attributeResult, extendedResult);
                        if (handler != null) {
                            handler(part);
                        }
                    }, errorHandler);
                }, errorHandler);
            } else {
                // Part doesn't exist, call handler with null
                handler(null);
            }
        }, errorHandler);
    }

    public override void NewPart(PartType partType, ErrorHandler errorHandler)
    {
        //TODO: See note in PartsDb.NewPart()
        string sql = "SELECT Part_num FROM Parts WHERE Part_type_id = " + partType.typeId + " ORDER BY Part_num DESC LIMIT 1";
        Execute(sql, delegate(DataTable partNumResult) {
            if (partNumResult.Rows.Count > 0) {
                DataRow row = partNumResult.Rows[0];
                string Part_num = Util.PartNumberString(Util.PartNumberInteger((string)row["Part_num"]) + 1);
#if DEBUG
                Console.WriteLine("New part num: {0}", Part_num);
#endif
                sql = "INSERT INTO Parts (Part_num, Part_type_id) VALUES ('" + Part_num + "', " + partType.typeId + ")";
                Execute(sql, null, errorHandler);
            }
        }, errorHandler);
    }

    public override void UpdatePart(Part part, ErrorHandler errorHandler)
    {
        DataTable dt = null;
        DataRow row = null;
        MySqlCommand cmd = null;
        int modifiedColumns = 0;

        /* TODO: Part will soon have only a flat set of attribute columns.
         *  Read schema (perhaps cached earlier?) and determine which columns go back to which table.
         *  We get inside knowledge on which 2 tables to examine (Parts and <type>_attributes), but
         *  both may change their columns in the future.
         *
         *  Extra credit: Identify and ignore primary key and foreign key columns?
         */

        try {
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
            row = dt.Rows[0];
            modifiedColumns = 0;
            cmd.CommandType = System.Data.CommandType.Text;
            string table = string.Format("{0}_attributes", part.Part_type);
            cmd.CommandText = string.Format("UPDATE {0} SET",table);
            foreach (DataColumn col in dt.Columns)
            {
                if (col.DataType != typeof(string))
                {
                    // For now, all attributes are strings. Ignore numerics.
                    continue;
                }
                else if (col.ColumnName == "Part_num")
                {
                    // Don't change Part_num
                    continue;
                }
                string original = "";
                if(! DBNull.Value.Equals(row[col, DataRowVersion.Original]) )
                {
                    original = (string)row[col, DataRowVersion.Original];
                }
                string current = "";
                if (!DBNull.Value.Equals(row[col, DataRowVersion.Current]) )
                {
                    current = (string)row[col, DataRowVersion.Current];
                }
                if (original != current)
                {
                    cmd.CommandText += string.Format("{1}{0} = @{0}", col.ColumnName, (modifiedColumns++ > 0 ? ", " : " "));
                    cmd.Parameters.AddWithValue(string.Format("@{0}", col.ColumnName), current);
                }
            }
            // Only finish the update if there was anything to update ;-)
            if (cmd.Parameters.Count > 0)
            {
                cmd.CommandText += " WHERE Part_num = @Part_num";
                cmd.Parameters.AddWithValue("@Part_num", part.Part_num);
#if DEBUG
                Console.WriteLine("{0}", cmd.CommandText);
                foreach (MySqlParameter p in cmd.Parameters)
                {
                    Console.WriteLine(" {0} = {1}", p.ParameterName, p.Value);
                }
#endif
                cmd.ExecuteNonQuery();
            }
        } catch (Exception e) {
            if (errorHandler != null) {
                errorHandler(e.Message, e);
            }
        }
    }

    //TODO: Remove this
    public void DeletePart(string Part_num, ErrorHandler errorHandler)
    {
        string sql = "DELETE FROM Parts WHERE Part_num = '" + Part_num + "'";
        Execute(sql, null, errorHandler);
    }

    public void Execute(string sql, ResultHandler resultHandler, ErrorHandler errorHandler)
    {
#if DEBUG
        Console.WriteLine("Execute: {0}", sql);
#endif
        string result = null;
        DataTable dt = null;
        if ((result = Connect()) != null) {
            if (errorHandler != null) {
                //TODO: Move this into Connect() itself
                errorHandler(result, null);
            }
            return;
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
        } catch (Exception e) {
            Console.WriteLine("Exception querying DB: {0}", e.Message);
            if (errorHandler != null) {
                errorHandler(e.Message, e);
            }
        }
        if (resultHandler != null) {
            resultHandler(dt);
        }
    }

    private string Connect()
    {
        if (_con == null) {
            try {
                string connectionString =
                    "Server=" + Hostname + ";" +
                    "Database=" + Database + ";" +
                    "User ID=" + Username + ";" +
                    "Password=" + Password + ";";
                _con = new MySqlConnection(connectionString);
                _con.Open();
            } catch (Exception e) {
                return e.Message;
            }
        }
        return null;
    }
}
