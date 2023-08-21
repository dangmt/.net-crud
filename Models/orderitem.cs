using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YourNamespace.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int Quantity { get; set; }

        // Other attributes

        [ForeignKey("Order")]
        public long OrderId { get; set; }
        [JsonIgnore] // Use Newtonsoft.Json.JsonIgnore if using Newtonsoft.Json library
        public Order Order { get; set; }

        [ForeignKey("Product")]
        public long ProductId { get; set; }
        [JsonIgnore] // Use Newtonsoft.Json.JsonIgnore if using Newtonsoft.Json library
        public Product Product { get; set; }

        // Constructors
    }
}
