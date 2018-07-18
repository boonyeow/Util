using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace Util
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /* Text File Access */
            string path = @"C:\Users\boon\source\repos\Util\QNA_17072018151525059.csv";
            List<string[]> fileContent = ConvertTo2DArray(path, ',');
            Response.Write(fileContent[0].Length + "<br>"); //retrieves number of columns
            Response.Write(fileContent.Count + "<br>"); //retrieves number of rows

            /* SQLite Database Access | ref; https://csharp-station.com/Tutorial/AdoDotNet */
            
            string connString = @"C:\Users\boon\source\repos\Util\chinook.db";
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + connString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from albums", conn);

                //ExecuteReader Usage 
                using (SQLiteDataReader reader = cmd.ExecuteReader()) //DataReader -> can be iterated over only once | DataAdapter -> can be iterated multiple times (disconnected access)
                {
                    while(reader.Read()) //reads data line by line
                    {
                        Response.Write(reader.FieldCount); //return number of columns in a table
                        Response.Write(reader[0]); //returns value of first column 
                        Response.Write(reader[1]);
                        Response.Write(reader[2] + "<br>");
                        break;
                    }
                }
                
                //ExecuteNonQuery Usage -> used when you do not want any results to be returned (e.g. Update, Insert or Delete statements) 
                cmd.CommandText = "delete from albums where AlbumId=@AlbumId";
                cmd.Parameters.AddWithValue("@AlbumId", 346); //if used ...where AlbumId=" + "347" instead -> vulnerable to SQL injections
                cmd.ExecuteNonQuery();

                //ExecuteScalar Usage -> returns a single value (value in first row and first col)
                cmd.CommandText = "select count(*) AlbumId from albums";
                int numberOfRows = Convert.ToInt32(cmd.ExecuteScalar());
                Response.Write(numberOfRows);



                //DataTable to HTML Table
                DataTable dt = QueryResults("select * from albums", conn);
                //Method 1
                Label1.Text = ConvertToHTMLTable(dt);

                //Method 2
                GridView1.DataSource = dt;
                GridView1.DataBind();
            }
        }

        private List<string[]> ConvertTo2DArray(string path, char delimiter) //Convert CSV text file to 2D Array, 
        {
            List<string[]> fileContent = new List<string[]>();
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                while (!sr.EndOfStream)
                {
                    string[] curLine = sr.ReadLine().Split(delimiter);
                    fileContent.Add(curLine); 
                }
            }
            return fileContent; //Access fileContent using fileContent[row index][column index]; 
        }

        private DataTable QueryResults(string commandText, SQLiteConnection conn) //Store results from query statement into DataTable
        {
            DataTable dt = new DataTable();
            using (SQLiteDataAdapter da = new SQLiteDataAdapter(commandText, conn))
            {
                da.Fill(dt);
            }
            return dt; //Access DataTable using dt.Rows[row index][column index]
        }

        private string ConvertToHTMLTable(DataTable dt) //Convert DataTable to HTML Table
        {
            StringBuilder s = new StringBuilder();
            s.Append("<table border=1>");

            //Build the header row
            s.Append("<thead><tr>");
            foreach (DataColumn column in dt.Columns)
            {
                s.Append("<th>" + column.ColumnName + "</th>");
            }
            s.Append("</tr><thead>");
            //**IMPT ->> s.Append("<th></th>") to add another column header 

            //Build the data rows
            s.Append("<tbody>");
            foreach (DataRow row in dt.Rows)
            {
                s.Append("<tr>");
                foreach (DataColumn column in dt.Columns)
                {
                    s.Append("<td>" + row[column.ColumnName] + "</td>");
                }
                //**IMPT ->> s.Append("<td></td>") to add another column of data 
                s.Append("</tr>");
            }

            s.Append("</tbody>");
            s.Append("</table");

            return s.ToString();
        }
    }
}