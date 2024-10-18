using System.ComponentModel.DataAnnotations;

namespace Uttom.Domain.Models;

public abstract class Entity
{
    [Key]
    public int Id { get; set; }
}
