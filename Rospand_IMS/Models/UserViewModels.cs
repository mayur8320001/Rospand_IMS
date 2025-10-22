using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UserViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }
    }

    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        public List<PageAccessViewModel> PageAccesses { get; set; } = new List<PageAccessViewModel>();
    }

    public class PageAccessViewModel
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string PageName { get; set; } = string.Empty;

        public bool IsAdd { get; set; } = false;
        public bool IsEdit { get; set; } = false;
        public bool IsDelete { get; set; } = false;
    }
}