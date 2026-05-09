using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sa3dny.Data.Models
{
    public class Admin
    {
        [Key]
        public int Admin_ID { get; set; }
        [Required]
        [DisplayName("Name")]
        public string Name_Admin { get; set; }
        public string Access { get; set; }
        public string UserId { get; internal set; }
        [ForeignKey("UserId")]
        public ApplicationUser applicationUser { get; set; }
    }
}
