using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SqlPartsDb : PartsDb
{
    private SqlConnection _con = null;

    private Dictionary<string, DataTable> tableSchemas;

    private readonly string[] stringTypes = new string[] {"CHAR", "VARCHAR", "BINARY", "VARBINARY", "BLOB", "TEXT", "ENUM", "SET"};
    private readonly string[] partTypeNames = new string[] {
        "Analog",
        "Capacitor",
        "Connector",
        "Crystals_Oscillators",
        "Data_Converters",
        "Diode",
        "DSP_uP",
        "Imaging",
        "Inductor",
        "Interface",
        "Logic",
        "Memory",
        "Misc",
        "Optocoupler_Isolator",
        "PCBs",
        "Power",
        "Programmable_Logic",
        "Resistor",
        "Switch",
        "Transformer",
        "Transistor"
    };

    public SqlPartsDb()
    {
        tableSchemas = new Dictionary<string, DataTable>();
        Username = "";
        Password = "";
        Hostname = "";
        Database = "";
    }

    public override void GetPartTypes(PartTypesHandler partTypesHandler, ErrorHandler errorHandler)
    {
        List<PartType> partTypes = new List<PartType>();
        int i = 0;
        foreach (string name in partTypeNames) {
            partTypes.Add(new PartType(i++, name));
        }
        if (partTypesHandler != null) {
            partTypesHandler(partTypes);
        }
    }

    public override void GetParts(PartType partType, PartsHandler partsHandler, ErrorHandler errorHandler)
    {
        SqlCommandBuilder bld = new SqlCommandBuilder();
        SqlCommand cmd = new SqlCommand();
        string tableNameEscaped = bld.QuoteIdentifier(partType.name);
        cmd.CommandType = System.Data.CommandType.Text;
#if DEBUG
        cmd.CommandText = string.Format("SELECT TOP 4 * FROM {0} ORDER BY [Part Number] ASC", tableNameEscaped);
#else
        cmd.CommandText = string.Format("SELECT * FROM {0} ORDER BY [Part Number] ASC", tableNameEscaped);
#endif
        Execute(cmd, delegate(DataTable result) {
            result.Columns["Part Number"].ColumnName = "Part_num";
            //result.Columns.Remove("Part Type");
            PartCollection parts = new PartCollection(result);
            if (partsHandler != null) {
                partsHandler(parts);
            }
        }, errorHandler);
    }

    public override void GetPart(string Part_num, PartType partType, PartHandler handler, ErrorHandler errorHandler)
    {
        SqlCommandBuilder bld = new SqlCommandBuilder();
        SqlCommand cmd = new SqlCommand();
        string tableNameEscaped = bld.QuoteIdentifier(partType.name);
        string partNumColumnEscaped = bld.QuoteIdentifier("Part Number");
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @partNum", tableNameEscaped, partNumColumnEscaped);
        cmd.Parameters.AddWithValue("@partNum", Part_num);
        Execute(cmd, delegate(DataTable partResult) {
            Part part = null;
            if (partResult.Rows.Count > 0) {
                partResult.Columns["Part Number"].ColumnName = "Part_num";
                part = new Part(Part_num, partType.typeId, partType.name, partResult);
            }
            if (handler != null) {
                handler(part);
            }
        }, errorHandler);
    }

    public override void NewPart(PartType partType, ErrorHandler errorHandler)
    {
        //TODO: See note in PartsDb.NewPart()
        SqlCommandBuilder bld = new SqlCommandBuilder();
        SqlCommand cmd = new SqlCommand();
        string tableNameEscaped = bld.QuoteIdentifier(partType.name);
        string partNumColumnEscaped = bld.QuoteIdentifier("Part Number");
        string partTypeColumnEscaped = bld.QuoteIdentifier("Part Type");
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText = string.Format("SELECT TOP 1 {0} FROM {1} ORDER BY {0} DESC", partNumColumnEscaped, tableNameEscaped);
        Execute(cmd, delegate(DataTable partNumResult) {
            if (partNumResult.Rows.Count > 0) {
                DataRow row = partNumResult.Rows[0];
                string Part_num = Util.PartNumberString(Util.PartNumberInteger((string)row["Part Number"]) + 1);
#if DEBUG
                Console.WriteLine("New part num: {0}", Part_num);
#endif
                SqlCommand insertCmd = new SqlCommand();
                insertCmd.CommandType = System.Data.CommandType.Text;
                insertCmd.CommandText = string.Format("INSERT INTO {0} ({1}, {2}) VALUES (@partNum, @partType);", tableNameEscaped, partNumColumnEscaped, partTypeColumnEscaped);
                insertCmd.Parameters.AddWithValue("@partNum", Part_num);
                insertCmd.Parameters.AddWithValue("@partType", partType.name);
                Execute(insertCmd, null, errorHandler);
            }
        }, errorHandler);
    }

    public override void UpdatePart(Part part, ErrorHandler errorHandler)
    {
        SqlCommand cmd = null;
        PartType partType = PartNumToPartType(part);
        if (partType == null) {
            return;
        }
        try {
            cmd = BuildUpdateCommand(partType.name, part.Attributes);
            if (cmd != null) {
                Execute(cmd, null, errorHandler);
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

    public override void Execute(string sql, ResultHandler resultHandler, ErrorHandler errorHandler)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText = sql;
        Execute(cmd, resultHandler, errorHandler);
    }

    public void Execute(SqlCommand cmd, ResultHandler resultHandler, ErrorHandler errorHandler)
    {
#if DEBUG
        Console.WriteLine("Execute: {0}", Util.SqlCommandToString(cmd));
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
            cmd.Connection = _con;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
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

    public void RepairMissingAttributes()
    {
        this.GetPartTypes(delegate(List<PartType> partTypes) {
            foreach (PartType partType in partTypes) {
                SqlCommand cmd = new SqlCommand();
                SqlCommandBuilder bld = new SqlCommandBuilder();
                Connect();
                cmd.Connection = _con;
                string typeAttributesTable = string.Format("{0}_attributes", partType.name);
                string partNumColumnEscaped = bld.QuoteIdentifier("Part_num");
                string partsTableEscaped = bld.QuoteIdentifier("Parts");
                string partTypesTableEscaped = bld.QuoteIdentifier("Part_types");
                string partTypeIdColumnEscaped = bld.QuoteIdentifier("Part_type_id");
                string typeAttributesTableEscaped = bld.QuoteIdentifier(typeAttributesTable);
                cmd.CommandText = string.Format("SELECT {0} FROM {1} AS P NATURAL JOIN {2} WHERE P.{3} = @partType AND P.{0} NOT IN ( SELECT {0} FROM {4} )",
                    partNumColumnEscaped,
                    partsTableEscaped,
                    partTypesTableEscaped,
                    partTypeIdColumnEscaped,
                    typeAttributesTableEscaped);
                cmd.Parameters.AddWithValue("@partType", partType.typeId);
#if DEBUG
                Console.WriteLine(Util.SqlCommandToString(cmd));
#endif
                SqlDataReader typeReader = cmd.ExecuteReader();
                List<SqlCommand> insertCommands = new List<SqlCommand>();
                try {
                    while (typeReader.Read()) {
                        string partNum = (string)typeReader["Part_num"];
                        SqlCommand insertCmd = new SqlCommand();
                        insertCmd.Connection = _con;
                        insertCmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES (@partNum)",
                            typeAttributesTableEscaped,
                            partNumColumnEscaped);
                        insertCmd.Parameters.AddWithValue("@partNum", partNum);
                        insertCommands.Add(insertCmd);
                    }
                } finally {
                    typeReader.Close();
                }
                foreach (SqlCommand insertCmd in insertCommands) {
                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    if (rowsAffected < 1) {
                        Console.WriteLine("Warning: Failed to add {0} to {1} table",
                                (string)insertCmd.Parameters["@partNum"].Value,
                                typeAttributesTable);
                    }
                }
            }
        }, null);
    }

    private string Connect()
    {
        if (_con != null) {
            _con.Close();
            _con = null;
        }
        if (_con == null) {
            try {
                string connectionString = "";
                connectionString += string.Format("Server={0};", Hostname);
                connectionString += string.Format("Database={0};", Database);
                if (Username.Length > 0) {
                    connectionString += string.Format("User ID={0};", Username);
                    connectionString += string.Format("Password={0};", Password);
                } else {
                    connectionString += "Integrated security=SSPI; persist security info=false; Trusted_Connection=Yes;";
                }
#if DEBUG
                Console.WriteLine("Connecting with string {0}", connectionString);
#endif
                _con = new SqlConnection(connectionString);
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

    private PartType PartNumToPartType(Part part)
    {
        PartType retVal = null;
        Console.WriteLine("{0}: {1}={2}", part.Part_num, part.Part_type_id, part.Part_type);
        retVal = new PartType(part.Part_type_id, partTypeNames[part.Part_type_id]);
        return retVal;
    }

    private SqlCommand BuildUpdateCommand(string tableName, DataTable attributes)
    {
        if (attributes == null || attributes.Rows.Count <= 0) {
            return null;
        }
        DataRow row = attributes.Rows[0];
        DataTable tableSchema = this.GetSchema(tableName);
/*
        using(var writer = new System.IO.StringWriter()) {
            tableSchema.WriteXml(writer);
            Console.WriteLine(writer.ToString());
        }
*/
        attributes.Columns["Part_num"].ColumnName = "Part Number";

        SqlCommand cmd = new SqlCommand();
        SqlCommandBuilder bld = new SqlCommandBuilder();
        int modifiedColumns = 0;
        int whereColumns = 0;
        string tableNameEscaped = bld.QuoteIdentifier(tableName);
        string sqlBody = "";
        string sqlWhere = "WHERE";
        sqlBody = string.Format("UPDATE {0} SET", tableNameEscaped);
        foreach (DataRow schemaColumn in tableSchema.Rows) {
            string columnName = (string)schemaColumn["COLUMN_NAME"];
            //TODO: Update DB schema to have proper primary keys, and figure out how to detect them ;-)
            string columnKey = (columnName == "Part Number" ? "PRI" : "");
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
