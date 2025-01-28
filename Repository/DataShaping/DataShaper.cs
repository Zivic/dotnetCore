using System.Dynamic;
using System.Reflection;
using Contracts;
using Entities.Models;

namespace Repository.DataShaping;

public class DataShaper<T> : IDataShaper<T> where T : class
{
    //PropertyInfo is a type from the Reflection libraries
    public PropertyInfo[] Properties { get; set; }

    public DataShaper()
    {
        // | is a logical OR, || is a short circuit OR
        Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }
    
    public IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString)
    {
        var requiredProperties = GetRequiredProperties(fieldsString);
        return FetchData(entities, requiredProperties);
    }

    public ShapedEntity ShapeData(T entity, string fieldsString)
    {
        var requiredProperties = GetRequiredProperties(fieldsString);
        return FetchDataForEntity(entity, requiredProperties);
    }

    private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
    {
        var requiredProperties = new List<PropertyInfo>();
        if (!string.IsNullOrWhiteSpace(fieldsString))
        {
            var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var field in fields)
            {
                var property = Properties.FirstOrDefault(p => p.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));
                if(property == null)
                    continue;
                
                requiredProperties.Add(property);
            }
        }
        else //if the fields string is null, all fields are required, default implementation
        {
            requiredProperties = Properties.ToList();
        }
        return requiredProperties;
    }

    private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
    {
        var shapedData = new List<ShapedEntity>();
        foreach (var entity in entities)
        {
            var shapedObject = FetchDataForEntity(entity, requiredProperties);
            shapedData.Add(shapedObject);
        }
        return shapedData;
    }

    private ShapedEntity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
    {
        var shapedObject = new ShapedEntity();
        foreach (var property in requiredProperties)
        {
            //Reflection API (PropertyInfo type) allows us to use this method:
            var objectPropertyValue = property.GetValue(entity);
            shapedObject.Entity.TryAdd(property.Name, objectPropertyValue);
        }

        var objectProperty = entity.GetType().GetProperty("Id");
        shapedObject.Id = (Guid)objectProperty.GetValue(entity);
        
        return shapedObject;
    }
}