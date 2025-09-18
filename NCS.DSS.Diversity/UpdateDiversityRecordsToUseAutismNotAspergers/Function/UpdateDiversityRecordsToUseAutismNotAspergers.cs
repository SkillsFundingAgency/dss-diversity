using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Diversity.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Diversity.UpdateDiversityRecordsToUseAutismNotAspergers.Function;

public class UpdateDiversityRecordsToUseAutismNotAspergers
{
    private readonly ILogger<UpdateDiversityRecordsToUseAutismNotAspergers> _logger;
    private readonly Container _diversityContainer;

    public UpdateDiversityRecordsToUseAutismNotAspergers(ILogger<UpdateDiversityRecordsToUseAutismNotAspergers> logger, IOptions<DiversityConfigurationSettings> configOptions, CosmosClient cosmosClient)
    {
        _logger = logger;
        var config = configOptions.Value;

        _diversityContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
    }

    private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

    [Function("UpdateDiversityRecordsToUseAutismNotAspergers")]
    [Display(Name = "UpdateDiversityRecordsToUseAutismNotAspergers")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var query = "SELECT c.id FROM c WHERE c.SecondaryLearningDifficultyOrDisability = 15";
            using var iteratorSecodary = _diversityContainer.GetItemQueryIterator<dynamic>(query);

            while (iteratorSecodary.HasMoreResults)
            {
                foreach (var item in await iteratorSecodary.ReadNextAsync())
                {
                    string id = item.id;

                    PatchItemRequestOptions options = new()
                    {
                        FilterPredicate = "FROM c WHERE c.SecondaryLearningDifficultyOrDisability = 15"
                    };

                    List<PatchOperation> operations = new()
                    {
                        PatchOperation.Replace($"/SecondaryLearningDifficultyOrDisability", 14),
                    };

                    await _diversityContainer.PatchItemAsync<dynamic>(
                        id: id,
                        partitionKey: PartitionKey.None,
                        patchOperations: operations,
                        requestOptions: options
                    );
                }
            }

            query = "SELECT c.id FROM c WHERE c.PrimaryLearningDifficultyOrDisability = 15";
            using var iteratorPrimary = _diversityContainer.GetItemQueryIterator<dynamic>(query);

            while (iteratorPrimary.HasMoreResults)
            {
                foreach (var item in await iteratorPrimary.ReadNextAsync())
                {
                    string id = item.id;

                    PatchItemRequestOptions options = new()
                    {
                        FilterPredicate = "FROM c WHERE c.PrimaryLearningDifficultyOrDisability = 15"
                    };

                    List<PatchOperation> operations = new()
                    {
                        PatchOperation.Replace($"/PrimaryLearningDifficultyOrDisability", 14),
                    };

                    await _diversityContainer.PatchItemAsync<dynamic>(
                        id: id,
                        partitionKey: PartitionKey.None,
                        patchOperations: operations,
                        requestOptions: options
                    );
                }
            }

            return new OkObjectResult("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating diversity records.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
