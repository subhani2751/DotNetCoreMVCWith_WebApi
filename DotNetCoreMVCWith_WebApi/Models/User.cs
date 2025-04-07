using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCWith_WebApi.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? Role { get; set; }

    public bool? IsEmailConfirmed { get; set; }

    public string? EmailConfirmToken { get; set; }
    [NotMapped]
    public string? ConfirmPassword { get; set; }
}
