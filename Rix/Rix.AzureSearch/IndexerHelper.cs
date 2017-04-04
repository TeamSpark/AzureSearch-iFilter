using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Rix.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

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
			var field = new Field();
			field.IsFacetable = false;
			field.IsFilterable = false;
			field.IsKey = false;
			field.IsRetrievable = true;
			field.IsSearchable = false;
			field.IsSortable = false;
			field.Name = ConfigurationReader.SearchFieldContentName;
			field.Type = DataType.String;

			var index = new Index();
			index.Name = ConfigurationReader.SearchIndexName;
			index.Fields = new List<Field>();
			index.Fields.Add(field);

			var dataSource = new DataSource();
			dataSource.Name = ConfigurationReader.SearchDataSourceName;
			dataSource.Credentials = new DataSourceCredentials(ConfigurationReader.BlobStorageCopyConnectionString);
			dataSource.Type = DataSourceType.AzureBlob;
			dataSource.Container = new DataContainer(ConfigurationReader.BlobStorageCopyContainerName);

			await Task.WhenAll(
				ServiceClient.DataSources.CreateOrUpdateAsync(dataSource),
				ServiceClient.Indexes.CreateOrUpdateAsync(index));

			var fieldMapping = new FieldMapping();
			fieldMapping.SourceFieldName = "content"; // source field name for azure blob content
			fieldMapping.TargetFieldName = field.Name;

			var indexer = new Indexer();
			indexer.Name = ConfigurationReader.SearchIndexerName;
			indexer.DataSourceName = dataSource.Name;
			indexer.TargetIndexName = index.Name;
			indexer.FieldMappings = new List<FieldMapping>();
			indexer.FieldMappings.Add(fieldMapping);

			await ServiceClient.Indexers.CreateOrUpdateAsync(indexer);
		}
	}
}
