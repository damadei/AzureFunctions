using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderProcessingFunctionApp
{
    public static class OrderProcessing
    {
        [FunctionName("OrderProcessing")]
        public static void Run(
            [QueueTrigger("orderqueue", Connection = "OrderQueueConnection")]OrderMessage orderMessage,
            [CosmosDB("MainDB", "OrdersColl", SqlQuery = "SELECT * from c where c.id = {OrderId}", ConnectionStringSetting = "DocDB")] IEnumerable<Order> documents,
            [Blob("orders/{OrderId}", FileAccess.Write, Connection = "BlobConnection")] Stream stream,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {orderMessage}");

            if(documents == null)
            {
                throw new ArgumentNullException("documents");
            }

            var enumerator = documents.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var writer = new StreamWriter(stream);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, enumerator.Current);

                writer.Flush();
            }
            else
            {
                throw new ArgumentException("No order found");
            }
        }
    }

    public class OrderMessage
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
    }

    public class Order
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderItems Items { get; set; }
        public int CustomerId { get; set; }
    }

    public class OrderItems
    {
        public int TotalItems { get; set; }

        public OrderItem[] Items { get; set; }
    }

    public class OrderItem
    {
        public string SKU { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
    }

}
