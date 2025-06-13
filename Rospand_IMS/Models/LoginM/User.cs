namespace Rospand_IMS.Models.LoginM
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Store hashed password
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
    }

    public class Page
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Category", "Product"
        public string Controller { get; set; }
        public string Action { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
    }

    public class RolePermission
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int PageId { get; set; }
        public Page Page { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAdjust { get; set; }          // For inventory adjustments
        public bool CanViewLowStock { get; set; }    // For low stock alerts
        public bool CanReceive { get; set; }         // For purchase order receiving
        public bool CanExport { get; set; }          // For data export
        public bool CanApprove { get; set; }         // For approval workflows
    }
}
