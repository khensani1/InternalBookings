using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Booking
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Resource")]
    public int ResourceId { get; set; }

    [ForeignKey("ResourceId")]
    [ValidateNever]
    public Resource Resource { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public string BookedBy { get; set; }

    [Required]
    public string Purpose { get; set; }
} 