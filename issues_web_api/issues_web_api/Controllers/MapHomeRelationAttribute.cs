using System;

namespace issues_web_api.Controllers
{
    /// <summary>
    /// Esta classe representa um atributo C# que deve ser
    /// usado em controladores que querem ter o seu URL
    /// na "home page" da API.
    /// Como argumento, este atributo recebe
    /// o nome da "link relation" que é "implementation-based" e, portanto,
    /// deve estar documentada. O segundo argumento é o valor da link relation
    /// e que se espera ser um URI que possa ser desreferenciado de forma a
    /// que o cliente possa navegar pela API.
    /// </summary
    [AttributeUsage(AttributeTargets.Class)]
    public class MapHomeRelationAttribute : Attribute
    {
        public string RelationName { get; }
        public string RelationValue { get; }

        public MapHomeRelationAttribute(string relationName, string relationValue)
        {
            RelationName = relationName;
            RelationValue = relationValue;
        }
    }
}
