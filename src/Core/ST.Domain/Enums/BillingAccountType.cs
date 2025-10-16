using System.ComponentModel.DataAnnotations;

namespace ST.Domain.Enums
{
    public enum BillingAccountType
    {
        [Display(Name = "Bireysel")]
        Individual = 1,
        [Display(Name = "Kurumsal")]
        Corporate = 2
    }
}
