USE ProyectoLoginMVC;
GO

DROP TABLE IF EXISTS Usuarios;
GO

CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Correo NVARCHAR(120) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    Rol NVARCHAR(50) NOT NULL DEFAULT 'Estudiante',
    UltimoAcceso DATETIME NULL
);
GO

-- La contraseña '123456' encriptada en SHA-256 es:
-- 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
INSERT INTO Usuarios (Nombre, Correo, Password, Rol)
VALUES
    ('Sebastian Coronado', 'sebascoronadol1301@gmail.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Administrador')
GO