using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Rix.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Rix.AzureSearch
{
    public class IndexerHelper
	{
		protected readonly SearchServiceClient ServiceClient;

		public IndexerHelper()
		{
			ServiceClient = new SearchServiceClient(ConfigurationReader.SearchServiceName, new SearchCredentials(ConfigurationReader.SearchServiceApiKey));
		}

		public async Task EnsureIndexerCreatedAsync()
		{
            var definition = new Index()
            {
                Name = "indexDocument",
                Fields = FieldBuilder.BuildForType<IndexDocument>()
            };

            var dataSource = new DataSource();
			dataSource.Name = ConfigurationReader.SearchDataSourceName;
			dataSource.Credentials = new DataSourceCredentials(ConfigurationReader.BlobStorageConnectionString);
			dataSource.Type = DataSourceType.AzureBlob;
			dataSource.Container = new DataContainer(ConfigurationReader.BlobStorageContainerName);

			await Task.WhenAll(
				ServiceClient.DataSources.CreateOrUpdateAsync(dataSource),
				ServiceClient.Indexes.CreateOrUpdateAsync(definition));

            var mappingFunctionId = new FieldMappingFunction();
			mappingFunctionId.Name = "extractTokenAtPosition";
			mappingFunctionId.Parameters = new Dictionary<string, object>();
			mappingFunctionId.Parameters.Add("delimiter", ".");
			mappingFunctionId.Parameters.Add("position", 0);

			var fieldMappingId = new FieldMapping();
			fieldMappingId.SourceFieldName = "metadata_storage_name"; // source field name for azure blob name
			fieldMappingId.TargetFieldName = nameof(IndexDocument.Id);
			fieldMappingId.MappingFunction = mappingFunctionId;

			var fieldMappingContent = new FieldMapping();
			fieldMappingContent.SourceFieldName = "content"; // source field name for azure blob content
			fieldMappingContent.TargetFieldName = nameof(IndexDocument.Content);

			var indexer = new Indexer();
			indexer.Name = ConfigurationReader.SearchIndexerName;
			indexer.DataSourceName = dataSource.Name;
			indexer.TargetIndexName = definition.Name;
			indexer.Parameters = new IndexingParameters();
			indexer.Parameters.DoNotFailOnUnsupportedContentType();
			indexer.FieldMappings = new List<FieldMapping>();
			indexer.FieldMappings.Add(fieldMappingContent);
			indexer.FieldMappings.Add(fieldMappingId);

			await ServiceClient.Indexers.CreateOrUpdateAsync(indexer);
		}

		public async Task<List<object>> GetDocumentsCountAsync()
		{
			var searchResults = await ServiceClient.Indexes.GetClient(ConfigurationReader.SearchIndexName).Documents.SearchAsync("*");
			return searchResults.Results.SelectMany(x => x.Document.Where(y => y.Key == "id").Select(y => y.Value)).ToList();
		}

		public async Task DeleteDocumentsByIdAsync(List<string> ids)
		{
			throw new NotImplementedException();

			//var maxBatchSize = 1000;
			//var pagesCount = ids.Count % maxBatchSize;

			//var indexActions = new List<IndexAction>();
			//indexActions.Add(IndexAction.Delete("id", id));

			//await ServiceClient.Indexes.GetClient(ConfigurationReader.SearchIndexName).Documents.IndexAsync(new IndexBatch(indexActions));
		}

		public async Task RunIndexerAsync()
		{
			await ServiceClient.Indexers.RunAsync(ConfigurationReader.SearchIndexerName);
		}

        
	}
}
