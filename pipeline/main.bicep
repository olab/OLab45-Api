@description('Environment short code')
param environment_code string

@description('The prefix for Azure resources (e.g. olabdev).')
param resource_prefix string

@description('The location to deploy the service resources')
param location string = resourceGroup().location

var resourceNameFunctionApp = '${resource_prefix}${serviceName}'
var resourceNameFunctionAppFarm = '${resource_prefix}${serviceName}'
var resourceNameFunctionAppInsights = '${resource_prefix}${serviceName}'
var resourceNameFunctionAppStorage = take(replace('${resource_prefix}${serviceName}az', '-', ''), 24) // Remove special characters and assume 24 characters
var resourceNameStorage = take(replace('${resource_prefix}${serviceName}', '-', ''), 24) // Remove special characters and assume 24 characters
var serviceName = 'api'

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
    }
  }
}

resource appSettings 'Microsoft.Web/sites/config@2021-02-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    AzureWebJobsStorage: functionAppStorageConnectionString
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
    name: 'Y1'
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

var functionAppStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${resourceFunctionAppStorage.name};AccountKey=${listKeys(resourceFunctionAppStorage.id, resourceFunctionAppStorage.apiVersion).keys[0].value};EndpointSuffix=core.windows.net'
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

var provisioningServiceConnectionString = 'Hostname=${resourceNameProvisioningService}.azure-devices-provisioning.net;SharedAccessKeyName=provisioningserviceowner;SharedAccessKey=${listKeys(resourceProvisioningService.id, '2020-01-01').value[0].primaryKey}'
resource resourceProvisioningService 'Microsoft.Devices/provisioningServices@2020-03-01' = {
  name: resourceNameProvisioningService
  location: location
  properties: {
    iotHubs: [
      {
        connectionString: iothubConnectionString
        location: location
      }
    ]
  }
  sku: {
    name: dpsSkuName
    capacity: dpsSkuUnits
  }
}

var iothubConnectionString = 'HostName=${resourceIoTHub.properties.hostName};SharedAccessKeyName=${iotHubAccessPolicy};SharedAccessKey=${listKeys(resourceIoTHub.id, resourceIoTHub.apiVersion).value[0].primaryKey}'
resource resourceIoTHub 'Microsoft.Devices/IotHubs@2021-07-01' = {
  name: resourceNameIotHub
  location: location
  properties: {
    eventHubEndpoints: {
      events: {
        retentionTimeInDays: 1
        partitionCount: iothubPartitions
      }
    }
    routing: {
      endpoints: {
        eventHubs: [
          {
            name: eventHubNameTelemetry
            connectionString: resourceEventHubTelemetrySendConnectionString
            authenticationType: 'keyBased'
          }
          {
            name: eventHubNameBlobEvent
            connectionString: resourceEventHubBlobSendConnectionString
            authenticationType: 'keyBased'
          }
          {
            name: eventHubNameWebAppEvents
            connectionString: resourceEventHubWebAppsSendConnectionString
            authenticationType: 'keyBased'
          }
          {
            name: eventHubNameDeviceTwinEvents
            connectionString: resourceEventHubDeviceTwinSendConnectionString
            authenticationType: 'keyBased'
          }
          {
            name: eventHubNameDeviceConnectionEvents
            connectionString: resourceEventHubDeviceConnectionSendConnectionString
            authenticationType: 'keyBased'
          }          
        ]
        storageContainers: [
        ]
      }
      routes: [
        {
          name: eventHubNameTelemetry
          source: 'DeviceMessages'
          condition: '$body.msg_type = "camera_telemetry"'
          endpointNames: [
            eventHubNameTelemetry
          ]
          isEnabled: true
        }
        {
          name: eventHubNameBlobEvent
          source: 'DeviceMessages'
          condition: '$body.msg_type = "blob_event"'
          endpointNames: [
            eventHubNameBlobEvent
          ]
          isEnabled: true
        }
        {
          name: 'deviceTwinChanges'
          source: 'TwinChangeEvents'
          condition: 'true'
          endpointNames: [
            'events'
          ]
          isEnabled: true
        }
        {
          name: 'devicetwin'
          source: 'TwinChangeEvents'
          condition: 'true'
          endpointNames: [
            'devicetwin'
          ]
          isEnabled: true
        }        
        {
          name: eventHubNameDeviceConnectionEvents
          source: 'DeviceConnectionStateEvents'
          condition: 'true'
          endpointNames: [
            eventHubNameDeviceConnectionEvents
          ]
          isEnabled: true
        }           
      ]
      fallbackRoute: {
        name: '$fallback'
        source: 'DeviceMessages'
        condition: 'true'
        endpointNames: [
          'events'
        ]
        isEnabled: true
      }
    }
    cloudToDevice: {
      maxDeliveryCount: 10
      defaultTtlAsIso8601: 'PT1H'
      feedback: {
        lockDurationAsIso8601: 'PT1M'
        ttlAsIso8601: 'PT1H'
        maxDeliveryCount: 10
      }
    }
  }
  sku: {
    name: 'S1'
    capacity: 1
  }
}

resource resourceEventHub 'Microsoft.EventHub/namespaces@2017-04-01' existing = {
  name: resourceNameEventHub
}

// WebApps Event Hub

resource resourceEventHubWebAppEvents 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' existing = {
  parent: resourceEventHub
  name: eventHubNameWebAppEvents
}

resource resourceEventHubWebAppEventsDev 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubWebAppEvents
  name: 'dev'
}

resource resourceEventHubWebAppEventsHeartbeat 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubWebAppEvents
  name: 'heartbeat'
}


var resourceEventHubWebAppsSendConnectionString = listkeys(resourceEventHubWebAppEventsAuthRuleNameSend.id, '2015-08-01').primaryConnectionString
resource resourceEventHubWebAppEventsAuthRuleNameSend 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubWebAppEvents
  name: eventHubAuthRuleNameSend
}

resource resourceEventHubWebAppEventsAuthRuleNameListen 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubWebAppEvents
  name: eventHubAuthRuleNameListen
}

// BlobEvent Event Hub

resource resourceEventHubBlobEvent 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' existing = {
  parent: resourceEventHub
  name: eventHubNameBlobEvent
}

resource resourceEventHubBlobEvent_dev 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubBlobEvent
  name: 'dev'
}

resource resourceEventHubBlobEvent_heartbeat 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubBlobEvent
  name: 'heartbeat'
}

resource resourceEventHubBlobEvent_telemetryservice 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubBlobEvent
  name: 'telemetryservice'
}

var resourceEventHubBlobSendConnectionString = listkeys(resourceEventHubBlobEventAuthRuleNameSend.id, '2015-08-01').primaryConnectionString
resource resourceEventHubBlobEventAuthRuleNameSend 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubBlobEvent
  name: eventHubAuthRuleNameSend
}

resource resourceEventHubBlobEventAuthRuleNameListen 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubBlobEvent
  name: eventHubAuthRuleNameListen
}

// DeviceTelemetry Event Hub

resource resourceEventHubTelemetry 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' existing = {
  parent: resourceEventHub
  name: eventHubNameTelemetry
}

var resourceEventHubTelemetrySendConnectionString = listkeys(resourceEventHubTelemetryAuthRuleNameSend.id, '2015-08-01').primaryConnectionString
resource resourceEventHubTelemetryAuthRuleNameSend 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubTelemetry
  name: eventHubAuthRuleNameSend
}

var resourceEventHubTelemetryListenConnectionString = listkeys(resourceEventHubTelemetryAuthRuleNameListen.id, '2015-08-01').primaryConnectionString
resource resourceEventHubTelemetryAuthRuleNameListen 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubTelemetry
  name: eventHubAuthRuleNameListen
}

resource resourceEventHubTelemetryDev 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubTelemetry
  name: 'dev'
}

resource resourceEventHubTelemetryHeartbeat 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubTelemetry
  name: 'heartbeat'
}

resource resourceEventHubTelemetryTelemetryService 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2017-04-01' existing = {
  parent: resourceEventHubTelemetry
  name: 'telemetryservice'
}

// DeviceTwin Event Hub

resource resourceEventHubDeviceTwin 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' existing = {
  parent: resourceEventHub
  name: eventHubNameDeviceTwinEvents
}

var resourceEventHubDeviceTwinSendConnectionString = listkeys(resourceEventHubDeviceTwinAuthRuleNameSend.id, '2015-08-01').primaryConnectionString
resource resourceEventHubDeviceTwinAuthRuleNameSend 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubDeviceTwin
  name: eventHubAuthRuleNameSend
}

// DeviceConnection Event Hub

resource resourceEventHubDeviceConnection 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' existing = {
  parent: resourceEventHub
  name: eventHubNameDeviceConnectionEvents
}

var resourceEventHubDeviceConnectionSendConnectionString = listkeys(resourceEventHubDeviceConnectionAuthRuleNameSend.id, '2015-08-01').primaryConnectionString
resource resourceEventHubDeviceConnectionAuthRuleNameSend 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubDeviceConnection
  name: eventHubAuthRuleNameSend
}

resource resourceEventHubDeviceTwinAuthRuleNameListen 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' existing = {
  parent: resourceEventHubDeviceConnection
  name: eventHubAuthRuleNameListen
}


// Device Storage

// Note: Assumes the same resource group. If not true, add "scope" property with resource group name
var deviceStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${resourceDeviceStorage.name};AccountKey=${listKeys(resourceDeviceStorage.id, resourceDeviceStorage.apiVersion).keys[0].value};EndpointSuffix=core.windows.net'
resource resourceDeviceStorage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: resourceNameStorage
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: {
    disposable: 'yes'
  }
}

resource storageTableServices 'Microsoft.Storage/storageAccounts/tableServices@2021-06-01' = {
  parent: resourceDeviceStorage
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource storageTableProvisioning 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
  parent: storageTableServices
  name: 'provisioning'
}

resource storageTableConfigurationChanges 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
  parent: storageTableServices
  name: 'configurationChanges'
}

resource storageTableStreamConfiguration 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
  parent: storageTableServices
  name: 'streamConfiguration'
}

resource storageTableStreamHistory 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
  parent: storageTableServices
  name: 'deviceStreamHistory'
}

resource storageTableStreamAssignment 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
  parent: storageTableServices
  name: 'streamAssignment'
}

resource storageTableInstalledDevice 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
   parent: storageTableServices
   name: 'installedDevice'
}

output FunctionAppResourceName string = resourceNameFunctionApp
output TelemetryNamespaceName string = resourceNameEventHub
output TelemetryEventHubName string = eventHubNameTelemetry
output TelemetryEventHubPolicyName string = eventHubAuthRuleNameListen
