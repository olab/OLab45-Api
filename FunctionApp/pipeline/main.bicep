@description('Environment short code')
param environment_code string = 'dev'

@description('The prefix for Azure resources (e.g. kuvadev).')
param resource_prefix string = 'olab'

// Note: Array of allowable values not recommended by Microsoft in this case as the list of SKUs can be different per region
@description('Describes plan\'s pricing tier and capacity. Check details at https://azure.microsoft.com/en-us/pricing/details/app-service/')
param sku string = 'B1'

@description('The location to deploy the service resources')
param location string = 'canadacentral'

@description('MySQL database user id')
param MySqlUserId string = 'olab4admin'

@description('MySQL database user id')
param MySqlDatabaseId string = 'olab45'

@description('MySQL database host name')
param MySqlHostName string = 'olab45db'

@description('MySQL database password')
@secure()
param MySqlPassword string

@description('Auth Token key')
@secure()
param AuthTokenKey string

// Variables
// Note: Declaring variable blocks is not recommended by Microsoft
var serviceName = 'api'
var resourceNameFunctionApp = '${resource_prefix}${environment_code}${serviceName}'
var resourceNameFunctionAppFarm = resourceNameFunctionApp
var resourceNameFunctionAppInsights = resourceNameFunctionApp
var resourceNameFunctionAppStorage = '${resourceNameFunctionApp}az'
var resourceNameFileStorage = resourceNameFunctionApp
var resourceNameSignalr = '${resource_prefix}signalr'
var fileStorageContainerName = '$web'

resource appService 'Microsoft.Web/sites@2021-02-01' = {
  name: resourceNameFunctionApp
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    disposable: 'yes'
  }
  properties: {
    serverFarmId: appServicePlan.id
    enabled: true
    httpsOnly: true
    hostNamesDisabled: false
    siteConfig: {
      alwaysOn: true
      healthCheckPath: '/api/health'
      use32BitWorkerProcess : false
    }
  }
}

resource appSettings 'Microsoft.Web/sites/config@2021-02-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    'AppSetting:FileStorageConnectionString': fileStorageConnectionString
    'AppSetting:FileStorageContainer': fileStorageContainerName
    'AppSettings:Audience': 'https://www.olab.ca'
    'AppSettings:ImportFolder': '${resourceFileStorage.properties.primaryEndpoints.blob}/${fileStorageContainerName}/import'
    'AppSettings:Issuer': 'olab,moodle'
    'AppSettings:Secret': AuthTokenKey
    'AppSettings:SignalREndpoint': '/turktalk'
    'AppSettings:PublicFileFolder': '${resourceFileStorage.properties.primaryEndpoints.blob}/${fileStorageContainerName}/files'
    AzureWebJobsStorage: functionAppStorageConnectionString
    DefaultDatabase: 'server=${MySqlHostName}.mysql.database.azure.com;uid=${MySqlUserId};pwd=${MySqlPassword};database=${MySqlDatabaseId};ConvertZeroDateTime=True'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: functionAppStorageConnectionString
    WEBSITE_CONTENTSHARE: '${resourceNameFunctionApp}9552'
    WEBSITE_RUN_FROM_PACKAGE: '1'
  }
}

resource appCors 'Microsoft.Web/sites/config@2021-02-01' = {
  parent: appService
  name: 'web'
  properties: {
    cors: {
      allowedOrigins: [
        '*'        
      ]
      supportCredentials: false
    }
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: resourceNameFunctionAppFarm
  location: location
  kind: 'app'
  sku: {
    name: sku
  }
  tags: {
    disposable: 'yes'
  }
  properties: {
    // Note: These properties probably not required
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceNameFunctionAppInsights
  location: location
  kind: 'web'
  tags: {
    disposable: 'yes'
  }
  properties: {
    Application_Type: 'web'

    // Note: These properties probably not required
    RetentionInDays: 90
    IngestionMode: 'ApplicationInsights'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

var functionAppStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${resourceFunctionAppStorage.name};AccountKey=${resourceFunctionAppStorage.listkeys().keys[0].value};EndpointSuffix=core.windows.net'
resource resourceFunctionAppStorage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: resourceNameFunctionAppStorage
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: {
    disposable: 'yes'
  }
}

var fileStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${resourceFileStorage.name};AccountKey=${resourceFileStorage.listkeys().keys[0].value};EndpointSuffix=core.windows.net'
resource resourceFileStorage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: resourceNameFileStorage
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: {
    disposable: 'yes'
  }
}

var signalrConnectionString = resourceSignalr.listKeys().primaryConnectionString
resource resourceSignalr 'Microsoft.SignalRService/SignalR@2022-02-01' = {
  name: resourceNameSignalr
  location: location
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    capacity: 1
  }
  tags: {
    disposable: 'yes'
  }
  kind: 'SignalR'
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'True'
      }
    ]
    liveTraceConfiguration: {
      enabled: 'true'
      categories: [
        {
          name: 'ConnectivityLogs'
          enabled: 'true'
        }
        {
          name: 'MessagingLogs'
          enabled: 'true'
        }
        {
          name: 'HttpRequestLogs'
          enabled: 'true'
        }
      ]
    }
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    upstream: {
      templates: []
    }
    networkACLs: {
      defaultAction: 'Deny'
      publicNetwork: {
        allow: [
          'ServerConnection'
          'ClientConnection'
          'RESTAPI'
          'Trace'
        ]
      }
      privateEndpoints: []
    }
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    disableAadAuth: false
  }
}

