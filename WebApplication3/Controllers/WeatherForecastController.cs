using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Amazon;
namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyData
    {
        public string Dcombobox { get; set; }
        public string Dcombobox_Check { get; set; }
        public string Dtextbox { get; set; }
        public string Dradio_btn { get; set; }
        public string Dcheck { get; set; }
        public string Dtoggle { get; set; }
        public bool DetailRow { get; set; }
    }

    public class FoodNode : ControllerBase
    {
        public string name { get; set; }
        public string Name2 { get; set; }
        public List<FoodNode> Children { get; set; }

    }
    public class FoodNode2
    {
        public int Id { get; set; }
        public string name { get; set; }
        public int? ParentId { get; set; }
        public List<FoodNode2> Children { get; set; }
    }



    [Route("api/[controller]")]
    [ApiController]
    public class TreeDataController : ControllerBase
    {


        
        [HttpGet("all")]
        public IActionResult Get3()
        {

            MySqlConnection conn = new MySqlConnection($"server=db1.cge5up1sv9ue.eu-north-1.rds.amazonaws.com;user=db1;database=db;port=3306;password=Database1");
            conn.Open();

            // Define a query
            MySqlCommand sampleCommand = new MySqlCommand("select * from tree_table;", conn);

            // Execute a query
            MySqlDataReader mysqlDataRdr = sampleCommand.ExecuteReader();

            var nodes = new List<FoodNode2>();
            var nodeLookup = new Dictionary<int, FoodNode2>();

            // Read all rows and output the first column in each row
            while (mysqlDataRdr.Read())
            {
                var node = new FoodNode2
                {
                    Id = mysqlDataRdr.GetInt32(0),
                    ParentId = mysqlDataRdr.IsDBNull(1) ? (int?)null : mysqlDataRdr.GetInt32(1),
                    name = mysqlDataRdr.GetString(2)

                };

                nodes.Add(node);
                nodeLookup[node.Id] = node;
            }
            mysqlDataRdr.Close();
            // Close connection
            conn.Close();



            // Build tree structure
            foreach (var node in nodes)
            {
                if (node.ParentId.HasValue)
                {
                    var parentNode = nodeLookup[node.ParentId.Value];
                    if (parentNode.Children == null)
                        parentNode.Children = new List<FoodNode2>();
                    parentNode.Children.Add(node);
                }
            }

            // Get root nodes
            var rootNodes = nodes.Where(n => !n.ParentId.HasValue).ToList();

            // Convert to JSON
            var json = JsonConvert.SerializeObject(rootNodes);

            return Ok(json);
        }


        [HttpPost("newww")]
        public async Task<IActionResult> Post([FromBody] List<MyData> data)
        {
            string connStr = "server=db1.cge5up1sv9ue.eu-north-1.rds.amazonaws.com;user=db1;database=db;port=3306;password=Database1";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                foreach (var item in data)
                {
                    // Assuming you're inserting data into a table named 'data_table' with columns matching your MyData properties
                    string sql = "INSERT INTO data_table (dcombobox, dcombobox_Check, dtextbox, dradio_btn, dcheck, dtoggle, detailRow) VALUES (@dcombobox, @dcombobox_Check, @dtextbox, @dradio_btn, @dcheck, @dtoggle, @detailRow)";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    // Add the values from your MyData object to the command parameters
                    cmd.Parameters.AddWithValue("@dcombobox", item.Dcombobox);
                    cmd.Parameters.AddWithValue("@dcombobox_Check", item.Dcombobox_Check);
                    cmd.Parameters.AddWithValue("@dtextbox", item.Dtextbox);
                    cmd.Parameters.AddWithValue("@dradio_btn", item.Dradio_btn);
                    cmd.Parameters.AddWithValue("@dcheck", item.Dcheck);
                    cmd.Parameters.AddWithValue("@dtoggle", item.Dtoggle);
                    cmd.Parameters.AddWithValue("@detailRow", item.DetailRow);

                    await cmd.ExecuteNonQueryAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }


        [HttpGet("newww")]
        public IActionResult Get4()
        {
            var connStr = "server=db1.cge5up1sv9ue.eu-north-1.rds.amazonaws.com;user=db1;database=db;port=3306;password=Database1";
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                var cmd = new MySqlCommand("SELECT * FROM data_table;", conn);

                using (var rdr = cmd.ExecuteReader())
                {
                    var result = new List<Dictionary<string, object>>();
                    while (rdr.Read())
                    {
                        var dict = new Dictionary<string, object>();
                        for (var i = 0; i < rdr.FieldCount; i++)
                        {
                            dict[rdr.GetName(i)] = rdr.GetValue(i);
                        }
                        result.Add(dict);
                    }

                    return Ok(result);
                }
            }
        }



        [HttpPost("new")]
        public IActionResult Post( List<FoodNode2> data)
        {
            var connectionString = "Data Source=NEWIMAGE39\\MSSQLSERVERMSR;Initial Catalog=DDDD;Integrated Security=True";

            var query = "INSERT INTO tree_data_nnnn (name, ParentId) VALUES (@name, @parentId); SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Insert each node in the list
                foreach (var node in data)
                {
                    InsertNode(node, null, connection, query);
                }
            }

            return Ok();
        }

        // Helper method to insert a node and its children recursively
        private void InsertNode(FoodNode2 node, int? parentId, SqlConnection connection, string query)
        {
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", node.name);
                if (parentId.HasValue)
                    command.Parameters.AddWithValue("@parentId", parentId);
                else
                    command.Parameters.AddWithValue("@parentId", DBNull.Value);

                // Get the generated id of the inserted node
                var nodeId = Convert.ToInt32(command.ExecuteScalar());

                // Insert the children of the node if any
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        InsertNode(child, nodeId, connection, query);
                    }
                }
            }
        }


    }
}