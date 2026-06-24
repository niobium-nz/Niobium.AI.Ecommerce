targetScope = 'resourceGroup'

@description('Name of the environment.')
param environmentName string

@description('Location for deployed resources. If empty, uses the resource group location.')
param location string = resourceGroup().location

@description('Short name used as a prefix for Azure resources. Keep it globally unique where required.')
param appName string = 'niobiumecom-${environmentName}'

@description('Name of the Container Apps managed environment.')
param containerAppsEnvironmentName string = '${appName}-cae'

@description('Name of the Container App.')
param containerAppName string = '${appName}-ca'

@description('App settings to project into the container app environment.')
param appSettings array = []

@description('Automatically set by azd. True if the container app already exists.')
param appExists bool = true

var logAnalyticsName = '${appName}-law'
var appInsightsName = '${appName}-ai'
var storageAccountName = replace('${appName}-sa', '-', '')
var environmentStorageName = '${appName}-caes'
var schedulerName = '${appName}-dts'
var taskHubName = '${appName}-hub'

var derivedSecrets = [for setting in appSettings: {
  name: toLower(replace(string(setting.name), '_', '-'))
  value: string(setting.value)
}]

var containerEnv = [for setting in appSettings: {
  name: string(setting.name)
  secretRef: toLower(replace(string(setting.name), '_', '-'))
}]

module logAnalytics 'br/public:avm/res/operational-insights/workspace:0.15.1' = {
  params: {
    name: logAnalyticsName
    location: location
  }
}

module appInsights 'br/public:avm/res/insights/component:0.7.2' = {
  params: {
    name: appInsightsName
    workspaceResourceId: logAnalytics.outputs.resourceId
    location: location
  }
}

resource durableTaskScheduler 'Microsoft.DurableTask/schedulers@2026-02-01' = {
  name: schedulerName
  location: location
  properties: {
    sku: {
      name: 'Consumption'
    }
    publicNetworkAccess: 'Enabled'
    ipAllowlist: [
      '0.0.0.0/0'
    ]
  }
}

resource durableTaskHub 'Microsoft.DurableTask/schedulers/taskHubs@2026-02-01' = {
  parent: durableTaskScheduler
  name: taskHubName
}

var containerEnv2 = concat(containerEnv, [
  { 
      name: 'APPLICATION_INSIGHTS_CONNECTION_STRING'
      value: appInsights.outputs.connectionString
  }
  { 
      name: 'DURABLE_TASK_CONNECTION_STRING'
      value: 'Endpoint=${durableTaskScheduler.properties.endpoint};TaskHub=${taskHubName};Authentication=ManagedIdentity'
  }
])

module storageAccount 'br/public:avm/res/storage/storage-account:0.32.0' = {
  params: {
    name: storageAccountName
    publicNetworkAccess: 'Enabled'
    networkAcls: {
        defaultAction: 'Allow'
    }
  }
}

module managedEnvironment 'br/public:avm/res/app/managed-environment:0.13.3' = {
  params: {
    name: containerAppsEnvironmentName
    location: location
    zoneRedundant: false
    publicNetworkAccess: 'Enabled'
    appInsightsConnectionString: appInsights.outputs.connectionString
    storages: [
      {
        accessMode: 'ReadWrite'
        kind: 'SMB'
        name: environmentStorageName
        storageAccountName: storageAccount.outputs.name
      }
    ]
  }
}

var currentImage = appExists ? reference(resourceId('Microsoft.App/containerApps', containerAppName), '2026-01-01').template.containers[0].image : 'mcr.microsoft.com/dotnet/samples:dotnetapp'
module containerApp 'br/public:avm/res/app/container-app:0.21.0' = {
  params: {
    name: containerAppName
    location: location
    tags: {
        'azd-service-name': 'host'
    }
    environmentResourceId: managedEnvironment.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    containers: [
      {
        name: 'app'
        image: currentImage
        env: containerEnv2
        resources: {
          cpu: any('0.25')
          memory: '0.5Gi'
        }
        volumeMounts: [
          {
            volumeName: environmentStorageName
            mountPath: '/artifacts'
          }
        ]
      }
    ]
    scaleSettings: {
        minReplicas: 0
        maxReplicas: 1
    }
    secrets: derivedSecrets
    disableIngress: true
    volumes: [
      {
        name: environmentStorageName
        storageName: environmentStorageName
        storageType: 'AzureFile'
      }
    ]
  }
}

module fileShare 'br/public:avm/res/storage/storage-account/file-service/share:0.1.3' = {
  params: {
    name: environmentStorageName
    storageAccountName: storageAccount.outputs.name
    accessTier: 'Hot'
    roleAssignments: [
      {
         principalId: containerApp.outputs.systemAssignedMIPrincipalId!
         roleDefinitionIdOrName: 'Storage File Data SMB Share Contributor'
         principalType: 'ServicePrincipal'
      }
    ]
  }
}

var durableTaskDataContributorRoleId = '0ad04412-c4d5-4796-b79c-f76d14c8d402'
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(durableTaskScheduler.id, containerApp.name, durableTaskDataContributorRoleId)
  scope: durableTaskScheduler
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', durableTaskDataContributorRoleId)
    principalId: containerApp.outputs.systemAssignedMIPrincipalId!
    principalType: 'ServicePrincipal'
  }
}

output containerAppId string = containerApp.outputs.resourceId
output containerAppFqdn string = containerApp.outputs.fqdn