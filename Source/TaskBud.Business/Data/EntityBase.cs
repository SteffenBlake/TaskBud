using System;
using System.ComponentModel.DataAnnotations;

namespace TaskBud.Business.Data
{
    public class EntityBase
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}