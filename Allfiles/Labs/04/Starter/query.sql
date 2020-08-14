select
m.Id as id,
m.Name as name,
m.Category as category,
m.Description as description,
m.Photo as photo,
products.Id as id,
products.Number as number,
products.Name as name,
products.Color as color,
products.Size as size,
products.StandardCost as standardCost,
products.ListPrice as listPrice
from [dbo].[Models] m
inner join [dbo].[Products] products
on m.Id = products.ModelId
for json auto