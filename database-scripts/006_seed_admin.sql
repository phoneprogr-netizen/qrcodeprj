DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE Name='SuperAdmin');
INSERT INTO Users (ClientId,Username,PasswordHash,FullName,Email,RoleId,IsActive,IsDeleted)
VALUES (NULL,'superadmin','R4HfQ6auMU5QParflcGm0QvNa65W8v3NQkSfA2g2m0M=','Super Admin','admin@example.com',@RoleId,1,0);
