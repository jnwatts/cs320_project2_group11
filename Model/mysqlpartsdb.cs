using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class MySqlPartsDb : PartsDb
{
    public delegate void ResultHandler(DataTable result);

    private MySqlConnection _con = null;

    private Dictionary<string, DataTable> tableSchemas;

    private readonly string[] stringTypes = new string[] {"CHAR", "VARCHAR", "BINARY", "VARBINARY", "BLOB", "TEXT", "ENUM", "SET"};

    public MySqlPartsDb()
    {
        tableSchemas = new Dictionary<string, DataTable>();
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
        string sql = "SELECT * FROM Parts AS P NATURAL LEFT JOIN " + partType.name + "_attributes AS A WHERE P.Part_type_id = " + partType.typeId;
        Execute(sql, delegate(DataTable result) {
            PartCollection parts = new PartCollection(result);
            if (partsHandler != null) {
                partsHandler(parts);
            }
        }, errorHandler);
    }

    public override void GetPart(string Part_num, PartHandler handler, ErrorHandler errorHandler)
    {
        //TODO: Move this logic into a stored procedure
        string sql = "SELECT Part_type_id, Type FROM Parts NATURAL LEFT JOIN Part_types WHERE Part_num = '" + Part_num + "'";
        Execute(sql, delegate(DataTable typeResult) {
            int typeId = (int)typeResult.Rows[0]["Part_type_id"];
            string typeName = (string)typeResult.Rows[0]["Type"];
            sql = "SELECT * FROM Parts AS P NATURAL LEFT JOIN " + typeName + "_attributes AS A WHERE P.Part_type_id = " + typeId + " AND Part_num = '" + Part_num + "'";
            Execute(sql, delegate(DataTable partResult) {
                Part part = null;
                if (partResult.Rows.Count > 0) {
                    part = new Part(Part_num, typeId, typeName, partResult);
                }
                if (handler != null) {
                    handler(part);
                }
            }, errorHandler);
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
        MySqlCommand cmd = null;

        try {
            cmd = BuildUpdateCommand("Parts", part.Attributes);
            if (cmd != null) {
                cmd.Connection = _con;
#if DEBUG
                Console.WriteLine(Util.SqlCommandToString(cmd));
#endif
                cmd.ExecuteNonQuery();
            }
            cmd = BuildUpdateCommand(string.Format("{0}_attributes", part.Part_type), part.Attributes);
            if (cmd != null) {
                cmd.Connection = _con;
#if DEBUG
                Console.WriteLine(Util.SqlCommandToString(cmd));
#endif
                cmd.ExecuteNonQuery();
            }
        } catch (Exception e) {
            Console.WriteLine(e);
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

    private DataTable GetSchema(string tableName)
    {
        if (!tableSchemas.ContainsKey(tableName)) {
            string errMsg;
            if ((errMsg = Connect()) != null) {
                Console.WriteLine("Error while connecting: {0}", errMsg);
                return null;
            }
            tableSchemas[tableName] = _con.GetSchema("Columns", new string[] { null, null, tableName, null });
        }
        return tableSchemas[tableName];
    }

    private MySqlCommand BuildUpdateCommand(string tableName, DataTable attributes)
    {
        if (attributes == null || attributes.Rows.Count <= 0) {
            return null;
        }
        DataRow row = attributes.Rows[0];
        DataTable tableSchema = this.GetSchema(tableName);
        MySqlCommand cmd = new MySqlCommand();
        MySqlCommandBuilder bld = new MySqlCommandBuilder();
        int modifiedColumns = 0;
        int whereColumns = 0;
        string tableNameEscaped = bld.QuoteIdentifier(tableName);
        string sqlBody = "";
        string sqlWhere = "WHERE";
        sqlBody = string.Format("UPDATE {0} SET", tableNameEscaped);
        foreach (DataRow schemaColumn in tableSchema.Rows) {
            string columnKey = (string)schemaColumn["COLUMN_KEY"];
            string columnName = (string)schemaColumn["COLUMN_NAME"];
            string dataType = (string)schemaColumn["DATA_TYPE"];
            string columnNameEscaped = bld.QuoteIdentifier(columnName);
            if (columnKey.Length > 0) {
                if (columnKey == "PRI") {
                    // Add WHERE clause to satisfy PRImary key
                    if (attributes.Columns.Contains(columnName)) {
                        object originalValue = row[columnName, DataRowVersion.Original];
                        string paramName = "@w" + modifiedColumns.ToString();
                        sqlWhere += string.Format("{2}{0} = {1}", columnNameEscaped, paramName, (whereColumns++ > 0 ? " AND " : " "));
                        cmd.Parameters.AddWithValue(paramName, originalValue);
                    }
                }
                // Never update any UNIque, PRImary or MULty-key columns
                continue;
            }
            if (Array.IndexOf(stringTypes, dataType.ToUpper()) < 0) {
                // For now, ignore non-string parameters
                continue;
            }
            if (attributes.Columns.Contains(columnName)) {
                object orignalObject = row[columnName, DataRowVersion.Original];
                object currentObject = row[columnName, DataRowVersion.Current];
                string originalValue = Convert.IsDBNull(orignalObject) ? "" : (string)orignalObject;
                string currentValue = Convert.IsDBNull(currentObject) ? "" : (string)currentObject;
                if (originalValue != currentValue) {
                    // Add SET expression
                    string paramName = "@p" + modifiedColumns.ToString();
                    sqlBody += string.Format("{2}{0} = {1}", columnNameEscaped, paramName, (modifiedColumns++ > 0 ? ", " : " "));
                    cmd.Parameters.AddWithValue(paramName, currentValue);
                }
            }
        }
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText = string.Format("{0} {1}", sqlBody, sqlWhere);
        if (modifiedColumns > 0) {
            return cmd;
        } else {
            return null;
        }
    }
}
