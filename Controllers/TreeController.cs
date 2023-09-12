using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Connection string for the SQL database
            var connectionString = "Data Source=.;Initial Catalog=TestDB;Integrated Security=True";

            // SQL query to select the data from the database
            var query = "SELECT name, children FROM tree_data";

            // Create a list to hold the results
            var results = new List<FoodNode>();

            // Create a new SqlConnection object
            using (var connection = new SqlConnection(connectionString))
            {
                // Create a new SqlCommand object
                using (var command = new SqlCommand(query, connection))
                {
                    // Open the connection
                    connection.Open();

                    // Execute the command and get the results
                    using (var reader = command.ExecuteReader())
                    {
                        // Loop through the results
                        while (reader.Read())
                        {
                            // Create a new FoodNode object for each result
                            var node = new FoodNode
                            {
                                name = reader.GetString(0),
                                Children = JsonConvert.DeserializeObject<List<FoodNode>>(reader.GetString(1))
                            };

                            // Add the node to the results list
                            results.Add(node);
                        }
                    }
                }
            }

            // Return the results as JSON
            return Ok(results);
        }

    }
}
