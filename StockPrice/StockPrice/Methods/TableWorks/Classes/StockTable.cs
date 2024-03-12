using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPrice.Methods.TableWorks.Classes
{
    /// <summary>
    /// Stock table data
    /// </summary>
    public sealed class StockTable
    {
        /// <summary>
        /// SKU of row.
        /// </summary>
        

        [Column(1)]
        [Required]
        public string Sku { get; set; }

        /// <summary>
        /// Condition of this SKU.
        /// </summary>
        [Column(2)]
        [Required]
        public string Condition { get; set; }

        /// <summary>
        /// Quantity of this SKU.
        /// </summary>
        [Column(3)]
        [Required]
        public string Quantity { get; set; }

        /// <summary>
        /// Comment for this SKU.
        /// </summary>
        [Column(4)]
        [Required]
        public string Comment { get; set; }

        /// <summary>
        /// Name of this SKU (Title).
        /// </summary>
        [Column(5)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// WareHouse where of this SKU.
        /// </summary>
        [Column(6)]
        [Required]
        public string WareHouse { get; set; }

        /// <summary>
        /// Replaces of this SKU (definited by ',').
        /// </summary>
        [Column(7)]
        [Required]
        public string Replaces { get; set; }
    }
}
