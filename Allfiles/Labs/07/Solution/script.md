#Create Resource Group
$rg = "lab7"

$location = "westeurope"

az configure --defaults location=$location group=$rg

az group create -n $rg

#Create storage account
$storName="storageistvan" 

az storage account create -n  $storName --sku Standard_LRS

$storConn=(az storage account show-connection-string -n $storName --query connectionString)

#Create Blob storage container
$container="drop"
az storage container create -n $container --connection-string $storConn

##Upload records.json
az storage blob upload -c $container -f "C:\Users\vitos\Desktop\Work\Azure Course\AZ-204-DevelopingSolutionsforMicrosoftAzure\Allfiles\Labs\07\Starter\records.json" -n records.json --connection-string $storConn

#Create Key Vault
$kv="keyvaultistvan"
az keyvault create --name $kv --resource-group $rg

##Save Storage Account Credentials
az keyvault secret set --vault-name $kv --name "StorageAccountConnectionString" --value $storConn

#Create Function app
$func="funclogicistvan"
az functionapp create -g $rg -n $func -s $storName --functions-version 3 --consumption-plan-location $location

##Add new Function
func init
func new

###Upload code to Azure
dotnet publish /o out

Compress-Archive -Path out\* -Update -DestinationPath site.zip

az functionapp deployment source config-zip -g $rg -n $func --src site.zip

#Set up permissions

##Function App - Identity -> enable system assigned managed identity
##Key Vault - Access Policies -> Add Access Policy
    Principal: funclogicistvan
    Secret permissions: Get

###Add application setting to function app

#Clean up
az group delete --name $rg --no-wait --yes

