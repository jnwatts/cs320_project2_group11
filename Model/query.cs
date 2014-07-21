using System;
using System.Data;
using System.Collections.Generic;

public class Query
{
    public String query{ get; set; }
    public String name { get; set; }

    public Query(String name, String query)
    {
        this.name = name;
        this.query = query;
    }


    public override String ToString()
    {
        return name;
    }

    public static List<Query> CreateQueries()
    {
        List<Query> QueryList = new List<Query>();

        QueryList.Add(new Query("Select a query...",
            ""));

        return QueryList;
    }
}
