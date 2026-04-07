-- Paso 2: autenticacion (modelo usuario) + auditoria (UsuarioSesionId)
-- Ejecutar en MySQL sobre el esquema actual.

ALTER TABLE usuario
    CHANGE COLUMN Hash PasswordHash VARCHAR(255) NOT NULL;

ALTER TABLE usuario
    MODIFY Nombres VARCHAR(150) NOT NULL,
    MODIFY PrimerApellido VARCHAR(100) NOT NULL,
    MODIFY SegundoApellido VARCHAR(100) NOT NULL,
    MODIFY Email VARCHAR(150) NOT NULL,
    MODIFY NombreUsuario VARCHAR(100) NOT NULL,
    MODIFY Salt VARCHAR(255) NULL,
    MODIFY Rol VARCHAR(30) NOT NULL,
    MODIFY Estado TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE usuario
    ADD CONSTRAINT CHK_usuario_rol
        CHECK (Rol IN ('Admin', 'Bibliotecario'));

CREATE UNIQUE INDEX UX_usuario_NombreUsuario ON usuario (NombreUsuario);
CREATE UNIQUE INDEX UX_usuario_Email ON usuario (Email);

ALTER TABLE autor MODIFY UsuarioSesionId INT NULL;
ALTER TABLE libro MODIFY UsuarioSesionId INT NULL;
ALTER TABLE ejemplar MODIFY UsuarioSesionId INT NULL;
ALTER TABLE usuario MODIFY UsuarioSesionId INT NULL;

CREATE INDEX IX_autor_UsuarioSesionId ON autor (UsuarioSesionId);
CREATE INDEX IX_libro_UsuarioSesionId ON libro (UsuarioSesionId);
CREATE INDEX IX_ejemplar_UsuarioSesionId ON ejemplar (UsuarioSesionId);
CREATE INDEX IX_usuario_UsuarioSesionId ON usuario (UsuarioSesionId);

ALTER TABLE autor
    ADD CONSTRAINT FK_autor_UsuarioSesion
        FOREIGN KEY (UsuarioSesionId)
        REFERENCES usuario (UsuarioId)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE libro
    ADD CONSTRAINT FK_libro_UsuarioSesion
        FOREIGN KEY (UsuarioSesionId)
        REFERENCES usuario (UsuarioId)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE ejemplar
    ADD CONSTRAINT FK_ejemplar_UsuarioSesion
        FOREIGN KEY (UsuarioSesionId)
        REFERENCES usuario (UsuarioId)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE usuario
    ADD CONSTRAINT FK_usuario_UsuarioSesion
        FOREIGN KEY (UsuarioSesionId)
        REFERENCES usuario (UsuarioId)
        ON DELETE SET NULL
        ON UPDATE CASCADE;
