# User Management System

This document describes the user management system implemented in the Rospand IMS application.

## Features

1. **User Authentication**
   - Login/Logout functionality
   - Password hashing using BCrypt
   - Session management

2. **User Management**
   - Create, Read, Update, Delete users
   - User activation/deactivation
   - Last login tracking

3. **Role Management**
   - Create, Read, Update, Delete roles
   - Role-based page access control
   - Manage page access permissions per role

4. **Access Control**
   - Page-level permissions (Add, Edit, Delete)
   - Role-based navigation

## Database Structure

### Users Table
- Id (int, primary key)
- Username (string, required)
- PasswordHash (string, required)
- Role (string, required)
- Email (string, optional)
- IsActive (bool, default: true)
- CreatedDate (DateTime, default: current date)
- LastLoginDate (DateTime, optional)

### RoleMasters Table
- Id (int, primary key)
- RoleName (string, required)

### PageAccesses Table
- Id (int, primary key)
- RoleId (int, foreign key to RoleMasters)
- PageName (string, required)
- IsAdd (bool, default: false)
- IsEdit (bool, default: false)
- IsDelete (bool, default: false)

## Default Users and Roles

### Default Roles
1. Admin
2. Manager
3. User

### Default User
- Username: admin
- Password: admin123
- Role: Admin

## Controllers

### AccountController
Handles authentication (login/logout) and access control.

### UserController
Manages user CRUD operations.

### RoleController
Manages role CRUD operations and page access permissions.

## Services

### IUserService
Interface defining all user management operations.

### UserService
Implementation of IUserService with database operations.

## Views

### Account Views
- Login.cshtml: User login form
- AccessDenied.cshtml: Access denied page

### User Views
- Index.cshtml: List all users
- Details.cshtml: User details
- Create.cshtml: Create new user
- Edit.cshtml: Edit existing user
- Delete.cshtml: Delete user confirmation

### Role Views
- Index.cshtml: List all roles
- Details.cshtml: Role details
- Create.cshtml: Create new role
- Edit.cshtml: Edit existing role
- Delete.cshtml: Delete role confirmation
- ManagePageAccess.cshtml: Manage page access permissions

## Security

- Passwords are hashed using BCrypt.Net
- Session-based authentication
- Role-based access control
- Input validation and sanitization

## Usage

1. Login with the default admin user (admin/admin123)
2. Navigate to User Management > Users to manage users
3. Navigate to User Management > Role Management to manage roles and permissions
4. Create new users and assign roles
5. Configure page access permissions for each role