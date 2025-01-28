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
    
    public IEnumerable<Entity> ShapeData(IEnumerable<T> entities, string fieldsString)
    {
        var requiredProperties = GetRequiredProperties(fieldsString);
        return FetchData(entities, requiredProperties);
    }

    public Entity ShapeData(T entity, string fieldsString)
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

    private IEnumerable<Entity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
    {
        var shapedData = new List<Entity>();
        foreach (var entity in entities)
        {
            var shapedObject = FetchDataForEntity(entity, requiredProperties);
            shapedData.Add(shapedObject);
        }
        return shapedData;
    }

    private Entity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
    {
        var shapedObject = new Entity();
        foreach (var property in requiredProperties)
        {
            //Reflection API (PropertyInfo type) allows us to use this method:
            var objectPropertyValue = property.GetValue(entity);
            shapedObject.TryAdd(property.Name, objectPropertyValue);
        }
        return shapedObject;
    }
}