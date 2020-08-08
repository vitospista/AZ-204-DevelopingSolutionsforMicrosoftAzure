az login
az account set -s "cd95c468-c847-4d77-a75e-1df1ae3a002f"
$rg=(az group list --query [0].name)
az configure --defaults location=westeurope group=$rg


# Create storage account
$storage="mediastoristvan"
az storage account create -n $storage --sku "Standard_LRS"

# Create blob container
$connStr=(az storage account show-connection-string -n $storage --query connectionString)
$key=(az storage account keys list -n $storage --query [0].value)
az storage container create -n raster-graphics --connection-string $connStr 
az storage container create -n compressed-audio --connection-string $connStr

# Upload file to container
az storage blob upload -f "C:\Users\vitos\Desktop\Work\Azure Course\AZ-204-DevelopingSolutionsforMicrosoftAzure\Allfiles\Labs\03\Starter\Images\graph.jpg" -c raster-graphics -n graph.jpg --connection-string $connStr

# Create dotnet console app
dotnet new console -n BlobManager

# Get nuget package 
dotnet add package Azure.Storage.Blobs --version 12.0.0

# Save connection string as secret
## Either as environment variable
setx AZURE_STORAGE_CONNECTION_STRING "<yourconnectionstring>"

## Or as a secret
dotnet add package Microsoft.Extensions.Configuration.UserSecrets

dotnet user-secrets init
dotnet user-secrets set AZURE_STORAGE_CONNECTION_STRING $connStr
dotnet user-secrets set AZURE_STORAGE_ACCOUNT_NAME $storage
dotnet user-secrets set AZURE_STORAGE_ACCOUNT_KEY $key