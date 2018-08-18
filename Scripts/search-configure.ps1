param(
 [Parameter(Mandatory=$True)]
 [string]
 $searchName,

 [Parameter(Mandatory=$True)]
 [string]
 $apiKey,

 [Parameter(Mandatory=$True)]
 [string]
 $dbServerName,

 [Parameter(Mandatory=$True)]
 [string]
 $dbName,

 [Parameter(Mandatory=$True)]
 [string]
 $dbUsername,

 [Parameter(Mandatory=$True)]
 [string]
 $dbPassword,
	
 [Parameter(Mandatory=$True)]
 [string]
 $storageAccountName,

 [Parameter(Mandatory=$True)]
 [string]
 $storageKey
)
 
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("cache-control", "no-cache")
$headers.Add("api-key", "$($apiKey)")
$headers.Add("content-type", "application/json")
 
$createSqlDataSourceRequest = @"
{   
    "name": "datasource-sql",
    "type": "azuresql",
    "credentials": {
        "connectionString": "Server=tcp:$($dbServerName).database.windows.net,1433;Initial Catalog=$($dbName);Persist Security Info=False;User ID=$($dbUsername);Password=$($dbPassword);MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;  "
    },
    "container": {
        "name": "IndexResults"
    }
}
"@;



$createBlobDataSourceRequest = @"
{   
    "name": "datasource-blob",
    "type": "azureblob",
    "credentials": {
        "connectionString": "DefaultEndpointsProtocol=https;AccountName=$($storageAccountName);AccountKey=$($storageKey);EndpointSuffix=core.windows.net"
    },
    "container": {
        "name": "search"
    }
}
"@;

$createIndexRequest=@"
	{ name: 'index-sql',
      fields: 
		  [ 
			{ 
				name: 'Id',
				type: 'Edm.String',
				key: true, retrievable: true, 
				filterable: false, searchable: false, sortable: false, facetable: false 
			},
			{ 
				name: 'Url',
				type: 'Edm.String',
				retrievable: true, filterable: true, searchable: true, 
				sortable: false, facetable: false  
			},
			{ 
				name: 'BlobUri',
				type: 'Edm.String',
				retrievable: true, filterable: true, searchable: true, 
				sortable: false, facetable: false  
			},
			{ 
				name: 'FileSize',
				type: 'Edm.Int64',
				retrievable: true, filterable: false, searchable: false, 
				sortable: false, facetable: false  
			},
			{ 
				name: 'Content',			  
				type: 'Edm.String',
				retrievable: true, searchable: true, 
				filterable: false, sortable: false, facetable: false  
			},
			{ 
				name: 'ContentType',			  
				type: 'Edm.String',
				retrievable: true, searchable: true, 
				filterable: false, sortable: false, facetable: false  
			},
			{ 
				name: 'LastModifiedDate',
				type: 'Edm.DateTimeOffset',
				retrievable: true, sortable: true, 
				searchable: false, filterable: false, facetable: false  
			}, 
			{ 
				name: 'Category',
				type: 'Edm.String',
				retrievable: true, facetable: true, sortable: true,
				filterable: false, searchable: false
			}, 
			{ 
				name: 'SubCategory',
				type: 'Edm.String',
				retrievable: true, facetable: true, sortable: true, 
				filterable: false, searchable: false  
			},
			{ 
				name: 'Description',
				type: 'Edm.String',
				retrievable: true, searchable: true, 
				filterable: false, sortable: false, facetable: false
			},
			{ 
				name: 'Keywords',
				type: 'Edm.String',
				retrievable: true, searchable: true, filterable: true,
				sortable: false, facetable: false
			}
		  ] 
	}
"@;

$createIndexerRequest = @"
{ name: 'indexerblob',
    dataSourceName: 'datasource-blob',
	targetIndexName: 'index-sql',
	schedule: { interval: 'PT5M' },
	fieldMappings : [ 
		{ sourceFieldName : 'mykey', targetFieldName : 'Id' },
		{ sourceFieldName : 'metadata_storage_size', targetFieldName : 'FileSize' },
		{ sourceFieldName : 'metadata_storage_path', targetFieldName : 'BlobUri' },
		{ sourceFieldName : 'metadata_storage_content_type', targetFieldName : 'ContentType' }
	],
	parameters : { configuration: { failOnUnsupportedContentType : false } }
}
"@;

$createSqlIndexerRequest = @"
{ name: 'indexersql',
    dataSourceName: 'datasource-sql',
	targetIndexName: 'index-sql',
	schedule: { interval: 'PT5M' }
}
"@;


# REST - DataSource
Invoke-RestMethod -Uri "https://$($searchName).search.windows.net/datasources?api-version=2016-09-01"  -Method POST -Body $createSqlDataSourceRequest -Headers $headers -ContentType "application/json"
Invoke-RestMethod -Uri "https://$($searchName).search.windows.net/datasources?api-version=2016-09-01"  -Method POST -Body $createBlobDataSourceRequest -Headers $headers -ContentType "application/json"

# REST - Index
Invoke-RestMethod -Uri "https://$($searchName).search.windows.net/indexes?api-version=2016-09-01" -Method POST -Body $createIndexRequest -Headers $headers -ContentType "application/json"

# REST - Indexer
Invoke-RestMethod -Uri "https://$($searchName).search.windows.net/indexers?api-version=2016-09-01" -Method POST -Body $createIndexerRequest -Headers $headers -ContentType "application/json"
Invoke-RestMethod -Uri "https://$($searchName).search.windows.net/indexers?api-version=2016-09-01" -Method POST -Body $createSqlIndexerRequest -Headers $headers -ContentType "application/json"
