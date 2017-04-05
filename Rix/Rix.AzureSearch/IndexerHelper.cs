﻿using Microsoft.Azure.Search;
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

		public async Task<List<KeyValuePair<string, string>>> GetDocumentsAsync()
		{
			var searchResults = await ServiceClient.Indexes.GetClient(ConfigurationReader.SearchIndexName).Documents.SearchAsync("*");

			var results = new List<KeyValuePair<string, string>>();

			foreach (var document in searchResults.Results.Select(x => x.Document))
			{
				var id = document.Single(x => x.Key == "id").Value.ToString();
				var content = document.Single(x => x.Key == "content").Value.ToString();

				results.Add(new KeyValuePair<string, string>(id, content));
			}

			return results;
		}

		public async Task DeleteDocumentsByIdsAsync(List<string> ids)
		{
			int maxBatchSize = 1000;
			int pagesCount = (ids.Count / maxBatchSize) + (ids.Count % maxBatchSize > 0 ? 1 : 0);

			for (var i = 0; i < pagesCount; i++)
			{
				var indexActions = ids
					.Skip(i * maxBatchSize)
					.Take(maxBatchSize)
					.Select(x => IndexAction.Delete(nameof(IndexDocument.Id), x));

				await ServiceClient.Indexes.GetClient(ConfigurationReader.SearchIndexName).Documents.IndexAsync(new IndexBatch(indexActions));
			}
		}

		public async Task RunIndexerAsync()
		{
			await ServiceClient.Indexers.RunAsync(ConfigurationReader.SearchIndexerName);
		}
	}
}
