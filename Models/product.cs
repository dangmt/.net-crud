using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YourNamespace.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }

        // Navigation properties
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
