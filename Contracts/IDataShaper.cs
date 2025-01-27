using System.Dynamic;

namespace Contracts;

public interface IDataShaper<T>
{
    /// <summary>
    /// Method for a collection of entities
    /// </summary>
    /// <param name="entities">The collection of entities</param>
    /// <param name="fieldString">The string containing the fields requested by the client</param>
    /// <returns></returns>
    IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldString);
    /// <summary>
    /// Method for a single entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="fieldString">The string containing the fields requested by the client</param>
    /// <returns></returns>
    ExpandoObject ShapeData(T entity, string fieldString);

}