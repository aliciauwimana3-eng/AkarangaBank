using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AkarangaBank.Web.ViewModels;

public class DocumentUploadViewModel
{
    [Range(1, int.MaxValue)] public int ClientId { get; set; }
    [Required] public string Categorie { get; set; } = "KYC";
    [Required] public IFormFile? Fichier { get; set; }
}
