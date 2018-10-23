using System.ComponentModel.DataAnnotations;

namespace DuploAuth.Models.Entities
{
    public class VariablesModel
    {
        [Key]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}