using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace Turnos.C.Models
{
    public class Rol :IdentityRole<int>
    {

        //public int Id { get; set; }

        public Rol() : base()
        {

        }

        public Rol(string name) : base(name) { }


        [Display(Name = "Nombre")]
        public string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        public override string NormalizedName
        {
            get => base.NormalizedName;
            set => base.NormalizedName = value;
        }
    }
}
