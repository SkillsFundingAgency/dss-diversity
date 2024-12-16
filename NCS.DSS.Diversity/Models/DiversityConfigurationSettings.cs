namespace NCS.DSS.Diversity.Models
{
    public class DiversityConfigurationSettings
    {
        public required string CollectionId { get; set; }
        public required string CustomerCollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string DatabaseId { get; set; }
        public required string QueueName { get; set; }
        public required string ServiceBusConnectionString { get; set; }        
        public required string CosmosDBConnectionString { get; set; }        
    }
}
