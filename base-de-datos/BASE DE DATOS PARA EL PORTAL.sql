BEGIN;

CREATE TABLE rol (
    id_rol SERIAL PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE estado_cuenta (
    id_estado_cuenta SERIAL PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE estado_solicitud (
    id_estado_solicitud SERIAL PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE situacion_academica (
    id_situacion SERIAL PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE area_especializacion (
    id_area_esp SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE sector (
    id_sector SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE habilidad (
    id_habilidad SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE estado_oferta (
    id_estado_oferta SERIAL PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE estado_postulacion (
    id_estado_postulacion SERIAL PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE usuario (
    id_usuario SERIAL PRIMARY KEY,
    correo VARCHAR(150) NOT NULL CHECK (correo ~ '^[^@[:space:]]+@[^@[:space:]]+\.[^@[:space:]]+$'),
    contrasenia_hash VARCHAR(255) NOT NULL,
    id_rol INTEGER NOT NULL REFERENCES rol(id_rol),
    id_estado_cuenta INTEGER NOT NULL REFERENCES estado_cuenta(id_estado_cuenta)
);

CREATE TABLE datos_registro (
    id_usuario INTEGER PRIMARY KEY REFERENCES usuario(id_usuario),
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    telefono VARCHAR(20) NOT NULL,
    foto_url VARCHAR(255),
    cargo VARCHAR(100),
    fecha_registro TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE solicitud_cuenta (
    id_solicitud SERIAL PRIMARY KEY,
    id_usuario INTEGER NOT NULL UNIQUE REFERENCES usuario(id_usuario),
    id_estado_solicitud INTEGER NOT NULL REFERENCES estado_solicitud(id_estado_solicitud),
    id_revisor INTEGER REFERENCES usuario(id_usuario),
    fecha_solicitud TIMESTAMP NOT NULL DEFAULT now(),
    fecha_resolucion TIMESTAMP CHECK (fecha_resolucion >= fecha_solicitud),
    declaracion_veracidad BOOLEAN
);

CREATE TABLE alumno (
    id_alumno SERIAL PRIMARY KEY,
    id_usuario INTEGER NOT NULL UNIQUE REFERENCES usuario(id_usuario),
    id_situacion INTEGER NOT NULL REFERENCES situacion_academica(id_situacion),
    id_area_esp INTEGER REFERENCES area_especializacion(id_area_esp),
    carnet VARCHAR(20) NOT NULL UNIQUE,
    anio_egreso_graduacion SMALLINT,
    CHECK (
        (id_situacion = 1 AND anio_egreso_graduacion IS NULL)
        OR (id_situacion IN (2, 3) AND anio_egreso_graduacion IS NOT NULL
            AND anio_egreso_graduacion BETWEEN 1960 AND 2100)
    )
);

CREATE TABLE consentimiento_visibilidad (
    id_consentimiento SERIAL PRIMARY KEY,
    id_alumno INTEGER NOT NULL REFERENCES alumno(id_alumno),
    fecha_otorgado TIMESTAMP NOT NULL DEFAULT now(),
    fecha_revocado TIMESTAMP CHECK (fecha_revocado >= fecha_otorgado)
);

CREATE TABLE experiencia_laboral (
    id_experiencia SERIAL PRIMARY KEY,
    id_alumno INTEGER NOT NULL REFERENCES alumno(id_alumno),
    id_sector INTEGER REFERENCES sector(id_sector),
    empresa VARCHAR(150) NOT NULL,
    cargo VARCHAR(100) NOT NULL,
    responsabilidades TEXT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE CHECK (fecha_fin >= fecha_inicio),
    es_actual BOOLEAN NOT NULL CHECK (es_actual = (fecha_fin IS NULL))
);

CREATE TABLE alumno_habilidad (
    id_alumno INTEGER NOT NULL REFERENCES alumno(id_alumno),
    id_habilidad INTEGER NOT NULL REFERENCES habilidad(id_habilidad),
    PRIMARY KEY (id_alumno, id_habilidad)
);

CREATE TABLE empresa (
    id_empresa SERIAL PRIMARY KEY,
    id_usuario INTEGER NOT NULL UNIQUE REFERENCES usuario(id_usuario),
    nombre_empresa VARCHAR(150) NOT NULL,
    nit VARCHAR(20),
    sitio_web VARCHAR(200),
    descripcion TEXT
);

CREATE TABLE oferta_laboral (
    id_oferta SERIAL PRIMARY KEY,
    id_empresa INTEGER REFERENCES empresa(id_empresa),
    empresa_externa VARCHAR(150),
    id_publicador INTEGER NOT NULL REFERENCES usuario(id_usuario),
    id_sector INTEGER NOT NULL REFERENCES sector(id_sector),
    id_estado_oferta INTEGER NOT NULL REFERENCES estado_oferta(id_estado_oferta),
    titulo VARCHAR(150) NOT NULL,
    mision TEXT NOT NULL,
    funciones TEXT NOT NULL,
    requisitos TEXT NOT NULL,
    competencias TEXT,
    horarios TEXT,
    ofrecemos TEXT,
    puestos_vacantes SMALLINT NOT NULL CHECK (puestos_vacantes > 0),
    puestos_cubiertos SMALLINT NOT NULL DEFAULT 0 CHECK (puestos_cubiertos BETWEEN 0 AND puestos_vacantes),
    nivel_experiencia VARCHAR(50) NOT NULL,
    departamento VARCHAR(60) NOT NULL,
    detalle_ubicacion VARCHAR(200),
    tipo_empleo VARCHAR(20) NOT NULL,
    modalidad VARCHAR(20),
    vehiculo VARCHAR(20),
    salario_min NUMERIC(10,2) CHECK (salario_min >= 0),
    salario_max NUMERIC(10,2) CHECK (salario_max >= 0 AND salario_max >= salario_min),
    persona_contacto VARCHAR(150),
    correo_contacto VARCHAR(150),
    indicaciones TEXT,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT now(),
    fecha_publicacion DATE,
    fecha_vencimiento DATE NOT NULL CHECK (fecha_vencimiento >= fecha_publicacion),
    CHECK (
        (id_empresa IS NOT NULL AND empresa_externa IS NULL)
        OR (id_empresa IS NULL AND empresa_externa IS NOT NULL)
    )
);

CREATE TABLE oferta_habilidad (
    id_oferta INTEGER NOT NULL REFERENCES oferta_laboral(id_oferta),
    id_habilidad INTEGER NOT NULL REFERENCES habilidad(id_habilidad),
    PRIMARY KEY (id_oferta, id_habilidad)
);

CREATE TABLE aviso (
    id_aviso SERIAL PRIMARY KEY,
    id_oferta INTEGER NOT NULL REFERENCES oferta_laboral(id_oferta),
    id_alumno INTEGER NOT NULL REFERENCES alumno(id_alumno),
    fecha_envio TIMESTAMP NOT NULL DEFAULT now(),
    estado_envio VARCHAR(20) NOT NULL DEFAULT 'Pendiente'
        CHECK (estado_envio IN ('Pendiente', 'Enviado', 'Fallido')),
    CONSTRAINT ux_aviso_oferta_alumno UNIQUE (id_oferta, id_alumno)
);

CREATE TABLE postulacion (
    id_postulacion SERIAL PRIMARY KEY,
    id_oferta INTEGER NOT NULL REFERENCES oferta_laboral(id_oferta),
    id_alumno INTEGER NOT NULL REFERENCES alumno(id_alumno),
    id_estado_postulacion INTEGER NOT NULL REFERENCES estado_postulacion(id_estado_postulacion),
    fecha_postulacion TIMESTAMP NOT NULL DEFAULT now(),
    fecha_resolucion TIMESTAMP CHECK (fecha_resolucion >= fecha_postulacion),
    CONSTRAINT ux_postulacion_oferta_alumno UNIQUE (id_oferta, id_alumno)
);

CREATE UNIQUE INDEX ux_usuario_correo ON usuario (lower(correo));
CREATE UNIQUE INDEX ux_consentimiento_vigente
    ON consentimiento_visibilidad (id_alumno) WHERE fecha_revocado IS NULL;
CREATE INDEX ix_oferta_estado_venc ON oferta_laboral (id_estado_oferta, fecha_vencimiento);
CREATE INDEX ix_postulacion_alumno ON postulacion (id_alumno);
CREATE INDEX ix_aviso_alumno ON aviso (id_alumno);
CREATE INDEX ix_alumno_situacion ON alumno (id_situacion, anio_egreso_graduacion);
CREATE INDEX ix_experiencia_alumno ON experiencia_laboral (id_alumno);
CREATE INDEX ix_alumno_hab_habilidad ON alumno_habilidad (id_habilidad);
CREATE INDEX ix_oferta_hab_habilidad ON oferta_habilidad (id_habilidad);

INSERT INTO rol (nombre) VALUES ('Administrador'), ('Alumno'), ('Empresa');
INSERT INTO estado_cuenta (nombre) VALUES ('Pendiente'), ('Activa'), ('Suspendida'), ('Rechazada');
INSERT INTO estado_solicitud (nombre) VALUES ('Pendiente'), ('Aprobada'), ('Rechazada');
INSERT INTO situacion_academica (id_situacion, nombre)
VALUES (1, 'Quinto año'), (2, 'Egresado'), (3, 'Graduado');
SELECT setval(pg_get_serial_sequence('situacion_academica', 'id_situacion'), 3);
INSERT INTO estado_oferta (nombre) VALUES ('Borrador'), ('Publicada'), ('Cubierta'), ('Vencida'), ('Cerrada');
INSERT INTO estado_postulacion (nombre) VALUES ('Recibida'), ('En revisión'), ('Seleccionado'), ('Descartado'), ('Retirada');
INSERT INTO sector (nombre) VALUES
  ('Tecnología | Informática'),
  ('Bancos | Financieras'),
  ('Telecomunicaciones'),
  ('Educación'),
  ('Salud'),
  ('Comercio'),
  ('Industria | Manufactura'),
  ('Gobierno | Instituciones públicas'),
  ('Servicios profesionales'),
  ('Otro')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO area_especializacion (nombre) VALUES
  ('Desarrollo de software'),
  ('Redes y telecomunicaciones'),
  ('Bases de datos'),
  ('Seguridad informática'),
  ('Soporte técnico'),
  ('Análisis de datos'),
  ('Gestión de proyectos de TI'),
  ('Otra')
ON CONFLICT (nombre) DO NOTHING;

CREATE FUNCTION fn_vacantes_disponibles(p_id_oferta INTEGER) RETURNS INTEGER
LANGUAGE sql STABLE AS $$
    SELECT puestos_vacantes - puestos_cubiertos FROM oferta_laboral WHERE id_oferta = p_id_oferta;
$$;

CREATE FUNCTION fn_oferta_vigente(p_id_oferta INTEGER) RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS (
        SELECT FROM oferta_laboral JOIN estado_oferta USING (id_estado_oferta)
        WHERE id_oferta = p_id_oferta AND nombre = 'Publicada'
          AND fecha_vencimiento >= CURRENT_DATE AND puestos_cubiertos < puestos_vacantes
    );
$$;

CREATE FUNCTION fn_consentimiento_vigente(p_id_alumno INTEGER) RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS (SELECT FROM consentimiento_visibilidad
                   WHERE id_alumno = p_id_alumno AND fecha_revocado IS NULL);
$$;

CREATE FUNCTION fn_condicion_laboral(p_id_alumno INTEGER) RETURNS TEXT
LANGUAGE sql STABLE AS $$
    SELECT CASE WHEN EXISTS (SELECT FROM experiencia_laboral WHERE id_alumno = p_id_alumno AND es_actual)
                THEN 'Empleado' ELSE 'Sin empleo actual' END;
$$;

CREATE FUNCTION fn_anios_experiencia(p_id_alumno INTEGER) RETURNS NUMERIC
LANGUAGE sql STABLE AS $$
    SELECT round(coalesce(sum(coalesce(fecha_fin, CURRENT_DATE) - fecha_inicio), 0) / 365.25, 1)
    FROM experiencia_laboral WHERE id_alumno = p_id_alumno;
$$;

CREATE FUNCTION fn_afinidad(p_id_alumno INTEGER, p_id_oferta INTEGER) RETURNS NUMERIC
LANGUAGE sql STABLE AS $$
    SELECT round(100.0 * count(habilidad_del_alumno.id_habilidad) / nullif(count(*), 0), 2)
    FROM oferta_habilidad AS habilidad_requerida
    LEFT JOIN alumno_habilidad AS habilidad_del_alumno
      ON habilidad_del_alumno.id_habilidad = habilidad_requerida.id_habilidad
     AND habilidad_del_alumno.id_alumno = p_id_alumno
    WHERE habilidad_requerida.id_oferta = p_id_oferta;
$$;

CREATE FUNCTION fn_generar_avisos_oferta(p_id_oferta INTEGER)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE
    avisos_insertados INTEGER;
BEGIN
    INSERT INTO aviso (id_oferta, id_alumno)
    SELECT p_id_oferta, alumno.id_alumno
    FROM alumno
    JOIN usuario ON usuario.id_usuario = alumno.id_usuario
    JOIN estado_cuenta AS estado_de_cuenta ON estado_de_cuenta.id_estado_cuenta = usuario.id_estado_cuenta
    WHERE estado_de_cuenta.nombre = 'Activa'
      AND COALESCE(fn_afinidad(alumno.id_alumno, p_id_oferta), 100) > 0
    ON CONFLICT (id_oferta, id_alumno) DO NOTHING;

    GET DIAGNOSTICS avisos_insertados = ROW_COUNT;
    RETURN avisos_insertados;
END;
$$;

CREATE FUNCTION fn_trg_perfil_rol() RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF NOT EXISTS (
        SELECT FROM usuario JOIN rol USING (id_rol)
        WHERE id_usuario = NEW.id_usuario AND nombre = TG_ARGV[0]
    ) THEN
        RAISE EXCEPTION 'El perfil requiere un usuario con rol %.', TG_ARGV[0];
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_alumno_rol BEFORE INSERT OR UPDATE OF id_usuario ON alumno
FOR EACH ROW EXECUTE FUNCTION fn_trg_perfil_rol('Alumno');
CREATE TRIGGER trg_empresa_rol BEFORE INSERT OR UPDATE OF id_usuario ON empresa
FOR EACH ROW EXECUTE FUNCTION fn_trg_perfil_rol('Empresa');

CREATE FUNCTION fn_trg_alumno_situacion() RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.id_situacion < OLD.id_situacion THEN
        RAISE EXCEPTION 'La situación académica no puede retroceder.';
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_alumno_situacion BEFORE UPDATE OF id_situacion ON alumno
FOR EACH ROW EXECUTE FUNCTION fn_trg_alumno_situacion();

CREATE FUNCTION fn_trg_experiencia_coherente() RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.fecha_inicio > CURRENT_DATE THEN
        RAISE EXCEPTION 'La fecha de inicio laboral no puede ser futura.';
    END IF;
    IF NEW.es_actual THEN
        NEW.fecha_fin := NULL;
    ELSIF NEW.fecha_fin IS NULL THEN
        RAISE EXCEPTION 'Un trabajo finalizado requiere fecha de fin.';
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_experiencia_coherente BEFORE INSERT OR UPDATE ON experiencia_laboral
FOR EACH ROW EXECUTE FUNCTION fn_trg_experiencia_coherente();

CREATE FUNCTION fn_trg_solicitud_antes_resolver() RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    estado_anterior TEXT;
    estado_nuevo TEXT;
BEGIN
    SELECT nombre INTO estado_anterior FROM estado_solicitud WHERE id_estado_solicitud = OLD.id_estado_solicitud;
    SELECT nombre INTO estado_nuevo FROM estado_solicitud WHERE id_estado_solicitud = NEW.id_estado_solicitud;
    IF estado_anterior <> 'Pendiente' THEN
        RAISE EXCEPTION 'La solicitud ya fue resuelta.';
    END IF;
    IF estado_nuevo IN ('Aprobada', 'Rechazada') THEN
        IF NEW.id_revisor = NEW.id_usuario OR NOT EXISTS (
            SELECT FROM usuario JOIN rol USING (id_rol)
            WHERE id_usuario = NEW.id_revisor AND nombre = 'Administrador'
        ) THEN
            RAISE EXCEPTION 'Se requiere un administrador distinto del solicitante.';
        END IF;
        NEW.fecha_resolucion := now();
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_solicitud_antes_resolver BEFORE UPDATE OF id_estado_solicitud, id_revisor ON solicitud_cuenta
FOR EACH ROW WHEN (OLD.* IS DISTINCT FROM NEW.*) EXECUTE FUNCTION fn_trg_solicitud_antes_resolver();

CREATE FUNCTION fn_trg_solicitud_aplicar() RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    estado_resuelto TEXT;
BEGIN
    SELECT nombre INTO estado_resuelto FROM estado_solicitud WHERE id_estado_solicitud = NEW.id_estado_solicitud;
    IF estado_resuelto IN ('Aprobada', 'Rechazada') THEN
        UPDATE usuario SET id_estado_cuenta = (
            SELECT id_estado_cuenta FROM estado_cuenta
            WHERE nombre = CASE WHEN estado_resuelto = 'Aprobada' THEN 'Activa' ELSE 'Rechazada' END
        ) WHERE id_usuario = NEW.id_usuario;
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_solicitud_aplicar AFTER UPDATE OF id_estado_solicitud ON solicitud_cuenta
FOR EACH ROW WHEN (OLD.id_estado_solicitud IS DISTINCT FROM NEW.id_estado_solicitud)
EXECUTE FUNCTION fn_trg_solicitud_aplicar();

CREATE FUNCTION fn_trg_oferta_antes_guardar() RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    estado_nuevo TEXT;
    rol_publicador TEXT;
    cuenta_publicador TEXT;
    estado_anterior TEXT;
BEGIN
    SELECT nombre INTO estado_nuevo FROM estado_oferta WHERE id_estado_oferta = NEW.id_estado_oferta;

    IF TG_OP = 'INSERT' THEN
        IF estado_nuevo IS DISTINCT FROM 'Borrador' OR NEW.puestos_cubiertos <> 0 THEN
            RAISE EXCEPTION 'La oferta debe crearse en Borrador y con cero puestos cubiertos.';
        END IF;
        NEW.fecha_publicacion := NULL;
    ELSE
        SELECT nombre INTO estado_anterior FROM estado_oferta WHERE id_estado_oferta = OLD.id_estado_oferta;
        NEW.fecha_publicacion := OLD.fecha_publicacion;
    END IF;

    SELECT rol.nombre, estado_de_cuenta.nombre INTO rol_publicador, cuenta_publicador
    FROM usuario
    JOIN rol USING (id_rol)
    JOIN estado_cuenta AS estado_de_cuenta USING (id_estado_cuenta)
    WHERE usuario.id_usuario = NEW.id_publicador;
    IF rol_publicador IS NULL OR rol_publicador NOT IN ('Empresa', 'Administrador') THEN
        RAISE EXCEPTION 'Solo una empresa o un administrador puede registrar ofertas.';
    END IF;
    IF rol_publicador = 'Empresa' AND NOT EXISTS (
        SELECT FROM empresa WHERE id_empresa = NEW.id_empresa AND id_usuario = NEW.id_publicador
    ) THEN
        RAISE EXCEPTION 'La empresa solo puede publicar a su propio nombre.';
    END IF;
    IF estado_nuevo = 'Publicada' AND estado_anterior IS DISTINCT FROM 'Publicada' THEN
        IF NEW.fecha_publicacion IS NULL THEN
            IF cuenta_publicador <> 'Activa' OR NEW.fecha_vencimiento < CURRENT_DATE THEN
                RAISE EXCEPTION 'Publicar requiere cuenta activa y fecha de vencimiento vigente.';
            END IF;
            NEW.fecha_publicacion := CURRENT_DATE;
        ELSIF estado_anterior <> 'Cubierta' THEN
            IF cuenta_publicador <> 'Activa' OR NEW.fecha_vencimiento < CURRENT_DATE THEN
                RAISE EXCEPTION 'Volver a publicar requiere cuenta activa y fecha de vencimiento vigente.';
            END IF;
            NEW.fecha_publicacion := CURRENT_DATE;
        END IF;
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_oferta_antes_guardar BEFORE INSERT OR UPDATE ON oferta_laboral
FOR EACH ROW EXECUTE FUNCTION fn_trg_oferta_antes_guardar();

CREATE FUNCTION fn_trg_oferta_publicada() RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF OLD.fecha_publicacion IS DISTINCT FROM NEW.fecha_publicacion AND EXISTS (
        SELECT FROM estado_oferta WHERE id_estado_oferta = NEW.id_estado_oferta AND nombre = 'Publicada'
    ) THEN
        PERFORM fn_generar_avisos_oferta(NEW.id_oferta);
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_oferta_publicada AFTER UPDATE OF id_estado_oferta ON oferta_laboral
FOR EACH ROW WHEN (OLD.id_estado_oferta IS DISTINCT FROM NEW.id_estado_oferta)
EXECUTE FUNCTION fn_trg_oferta_publicada();

CREATE FUNCTION fn_trg_postulacion_antes_insertar() RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    PERFORM 1 FROM oferta_laboral WHERE id_oferta = NEW.id_oferta FOR NO KEY UPDATE;
    IF NOT fn_oferta_vigente(NEW.id_oferta) THEN
        RAISE EXCEPTION 'La oferta no admite postulaciones.';
    END IF;
    IF NOT EXISTS (
        SELECT FROM alumno JOIN usuario USING (id_usuario) JOIN estado_cuenta USING (id_estado_cuenta)
        WHERE id_alumno = NEW.id_alumno AND nombre = 'Activa'
    ) THEN
        RAISE EXCEPTION 'El alumno necesita una cuenta activa.';
    END IF;
    SELECT id_estado_postulacion INTO NEW.id_estado_postulacion FROM estado_postulacion WHERE nombre = 'Recibida';
    NEW.fecha_postulacion := now();
    NEW.fecha_resolucion := NULL;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_postulacion_antes_insertar BEFORE INSERT ON postulacion
FOR EACH ROW EXECUTE FUNCTION fn_trg_postulacion_antes_insertar();

CREATE FUNCTION fn_trg_postulacion_antes_actualizar() RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    anterior TEXT;
    nuevo TEXT;
BEGIN
    SELECT nombre INTO anterior FROM estado_postulacion WHERE id_estado_postulacion = OLD.id_estado_postulacion;
    SELECT nombre INTO nuevo FROM estado_postulacion WHERE id_estado_postulacion = NEW.id_estado_postulacion;
    IF anterior = 'Retirada' THEN
        RAISE EXCEPTION 'Una postulación retirada no se puede reactivar.';
    END IF;
    NEW.fecha_resolucion := CASE WHEN nuevo IN ('Seleccionado', 'Descartado', 'Retirada') THEN now() END;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_postulacion_antes_actualizar BEFORE UPDATE OF id_estado_postulacion ON postulacion
FOR EACH ROW WHEN (OLD.id_estado_postulacion IS DISTINCT FROM NEW.id_estado_postulacion)
EXECUTE FUNCTION fn_trg_postulacion_antes_actualizar();

CREATE FUNCTION fn_trg_postulacion_vacantes() RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    seleccion INTEGER;
    cubierta INTEGER;
    publicada INTEGER;
    vencida INTEGER;
BEGIN
    SELECT id_estado_postulacion INTO seleccion FROM estado_postulacion WHERE nombre = 'Seleccionado';
    SELECT id_estado_oferta INTO cubierta FROM estado_oferta WHERE nombre = 'Cubierta';
    SELECT id_estado_oferta INTO publicada FROM estado_oferta WHERE nombre = 'Publicada';
    SELECT id_estado_oferta INTO vencida FROM estado_oferta WHERE nombre = 'Vencida';
    IF NEW.id_estado_postulacion = seleccion THEN
        UPDATE oferta_laboral
        SET puestos_cubiertos = puestos_cubiertos + 1,
            id_estado_oferta = CASE WHEN puestos_cubiertos + 1 = puestos_vacantes
                                         AND id_estado_oferta = publicada THEN cubierta ELSE id_estado_oferta END
        WHERE id_oferta = NEW.id_oferta AND puestos_cubiertos < puestos_vacantes;
        IF NOT FOUND THEN
            RAISE EXCEPTION 'La oferta ya no tiene vacantes disponibles.';
        END IF;
    ELSIF OLD.id_estado_postulacion = seleccion THEN
        UPDATE oferta_laboral
        SET puestos_cubiertos = puestos_cubiertos - 1,
            id_estado_oferta = CASE WHEN id_estado_oferta <> cubierta THEN id_estado_oferta
                                    WHEN fecha_vencimiento < CURRENT_DATE THEN vencida ELSE publicada END
        WHERE id_oferta = OLD.id_oferta;
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_postulacion_vacantes AFTER UPDATE OF id_estado_postulacion ON postulacion
FOR EACH ROW WHEN (OLD.id_estado_postulacion IS DISTINCT FROM NEW.id_estado_postulacion)
EXECUTE FUNCTION fn_trg_postulacion_vacantes();

CREATE PROCEDURE sp_registrar_alumno(
    p_correo VARCHAR, p_contrasenia_hash VARCHAR, p_nombres VARCHAR, p_apellidos VARCHAR,
    p_telefono VARCHAR, p_carnet VARCHAR, p_situacion VARCHAR, p_anio SMALLINT,
    p_declaracion BOOLEAN, INOUT p_id_usuario INTEGER
)
LANGUAGE plpgsql AS $$
DECLARE
    situacion INTEGER;
    restriccion TEXT;
BEGIN
    SELECT id_situacion INTO situacion FROM situacion_academica WHERE nombre = p_situacion;
    IF situacion IS NULL THEN
        RAISE EXCEPTION 'La situación académica no es válida.';
    END IF;
    IF situacion IN (2, 3) AND (p_anio IS NULL OR p_anio NOT BETWEEN 1960 AND 2100) THEN
        RAISE EXCEPTION 'El año de egreso o graduación debe estar entre 1960 y 2100.';
    END IF;
    INSERT INTO usuario (correo, contrasenia_hash, id_rol, id_estado_cuenta)
    SELECT p_correo, p_contrasenia_hash, rol.id_rol, estado_de_cuenta.id_estado_cuenta
    FROM rol
    CROSS JOIN estado_cuenta AS estado_de_cuenta
    WHERE rol.nombre = 'Alumno' AND estado_de_cuenta.nombre = 'Pendiente'
    RETURNING id_usuario INTO p_id_usuario;
    INSERT INTO datos_registro (id_usuario, nombres, apellidos, telefono)
    VALUES (p_id_usuario, p_nombres, p_apellidos, p_telefono);
    INSERT INTO alumno (id_usuario, id_situacion, carnet, anio_egreso_graduacion)
    VALUES (p_id_usuario, situacion, p_carnet, CASE WHEN situacion <> 1 THEN p_anio END);
    INSERT INTO solicitud_cuenta (id_usuario, id_estado_solicitud, declaracion_veracidad)
    SELECT p_id_usuario, id_estado_solicitud, p_declaracion FROM estado_solicitud WHERE nombre = 'Pendiente';
EXCEPTION WHEN unique_violation THEN
    GET STACKED DIAGNOSTICS restriccion = CONSTRAINT_NAME;
    IF restriccion = 'ux_usuario_correo' THEN RAISE EXCEPTION 'Ese correo ya está registrado.' USING ERRCODE = '23505'; END IF;
    IF restriccion = 'alumno_carnet_key' THEN RAISE EXCEPTION 'Ese carnet ya está registrado.' USING ERRCODE = '23505'; END IF;
    RAISE;
END;
$$;

CREATE PROCEDURE sp_registrar_empresa(
    p_correo VARCHAR, p_contrasenia_hash VARCHAR, p_nombres VARCHAR, p_apellidos VARCHAR,
    p_telefono VARCHAR, p_nombre_empresa VARCHAR, p_cargo VARCHAR, p_nit VARCHAR,
    p_sitio_web VARCHAR, p_descripcion TEXT, p_declaracion BOOLEAN, INOUT p_id_usuario INTEGER
)
LANGUAGE plpgsql AS $$
DECLARE
    restriccion TEXT;
BEGIN
    INSERT INTO usuario (correo, contrasenia_hash, id_rol, id_estado_cuenta)
    SELECT p_correo, p_contrasenia_hash, rol.id_rol, estado_de_cuenta.id_estado_cuenta
    FROM rol
    CROSS JOIN estado_cuenta AS estado_de_cuenta
    WHERE rol.nombre = 'Empresa' AND estado_de_cuenta.nombre = 'Pendiente'
    RETURNING id_usuario INTO p_id_usuario;
    INSERT INTO datos_registro (id_usuario, nombres, apellidos, telefono, cargo)
    VALUES (p_id_usuario, p_nombres, p_apellidos, p_telefono, p_cargo);
    INSERT INTO empresa (id_usuario, nombre_empresa, nit, sitio_web, descripcion)
    VALUES (p_id_usuario, p_nombre_empresa, p_nit, p_sitio_web, p_descripcion);
    INSERT INTO solicitud_cuenta (id_usuario, id_estado_solicitud, declaracion_veracidad)
    SELECT p_id_usuario, id_estado_solicitud, p_declaracion FROM estado_solicitud WHERE nombre = 'Pendiente';
EXCEPTION WHEN unique_violation THEN
    GET STACKED DIAGNOSTICS restriccion = CONSTRAINT_NAME;
    IF restriccion = 'ux_usuario_correo' THEN RAISE EXCEPTION 'Ese correo ya está registrado.' USING ERRCODE = '23505'; END IF;
    RAISE;
END;
$$;

CREATE PROCEDURE sp_resolver_solicitud(p_id_solicitud INTEGER, p_id_revisor INTEGER, p_aprobar BOOLEAN)
LANGUAGE plpgsql AS $$
BEGIN
    IF p_aprobar IS NULL THEN RAISE EXCEPTION 'Indica si se aprueba o se rechaza.'; END IF;
    UPDATE solicitud_cuenta
    SET id_revisor = p_id_revisor,
        id_estado_solicitud = (SELECT id_estado_solicitud FROM estado_solicitud
            WHERE nombre = CASE WHEN p_aprobar THEN 'Aprobada' ELSE 'Rechazada' END)
    WHERE id_solicitud = p_id_solicitud
      AND id_estado_solicitud = (SELECT id_estado_solicitud FROM estado_solicitud WHERE nombre = 'Pendiente');
    IF NOT FOUND THEN RAISE EXCEPTION 'No existe una solicitud pendiente con ese identificador.'; END IF;
END;
$$;

CREATE PROCEDURE sp_postular(p_id_oferta INTEGER, p_id_alumno INTEGER, INOUT p_id_postulacion INTEGER)
LANGUAGE plpgsql AS $$
DECLARE
    restriccion TEXT;
BEGIN
    INSERT INTO postulacion (id_oferta, id_alumno)
    VALUES (p_id_oferta, p_id_alumno)
    RETURNING id_postulacion INTO p_id_postulacion;
EXCEPTION WHEN unique_violation THEN
    GET STACKED DIAGNOSTICS restriccion = CONSTRAINT_NAME;
    IF restriccion = 'ux_postulacion_oferta_alumno' THEN
        RAISE EXCEPTION 'El alumno ya se postuló a esta oferta.' USING ERRCODE = '23505';
    END IF;
    RAISE;
END;
$$;

CREATE PROCEDURE sp_cambiar_estado_postulacion(
    p_id_postulacion INTEGER, p_nuevo_estado VARCHAR, p_id_actor INTEGER
)
LANGUAGE plpgsql AS $$
DECLARE
    datos RECORD;
    nuevo INTEGER;
BEGIN
    SELECT postulacion.id_estado_postulacion, alumno.id_usuario AS usuario_alumno, oferta.id_publicador,
           empresa.id_usuario AS usuario_empresa
    INTO datos
    FROM postulacion JOIN alumno USING (id_alumno) JOIN oferta_laboral AS oferta USING (id_oferta)
    LEFT JOIN empresa USING (id_empresa)
    WHERE postulacion.id_postulacion = p_id_postulacion
    FOR UPDATE OF postulacion
    FOR NO KEY UPDATE OF oferta;
    IF NOT FOUND THEN RAISE EXCEPTION 'La postulación no existe.'; END IF;
    IF NOT EXISTS (
        SELECT FROM usuario JOIN estado_cuenta USING (id_estado_cuenta)
        WHERE id_usuario = p_id_actor AND nombre = 'Activa'
    ) THEN
        RAISE EXCEPTION 'El actor necesita una cuenta activa.';
    END IF;
    IF p_nuevo_estado = 'Retirada' THEN
        IF p_id_actor IS DISTINCT FROM datos.usuario_alumno THEN
            RAISE EXCEPTION 'Solo el alumno titular puede retirar la postulación.';
        END IF;
    ELSIF p_nuevo_estado IN ('En revisión', 'Seleccionado', 'Descartado') THEN
        IF p_id_actor IS DISTINCT FROM datos.usuario_empresa AND p_id_actor IS DISTINCT FROM datos.id_publicador
           AND NOT EXISTS (
               SELECT FROM usuario JOIN rol USING (id_rol) WHERE id_usuario = p_id_actor AND nombre = 'Administrador'
           ) THEN
            RAISE EXCEPTION 'Ese usuario no puede gestionar la postulación.';
        END IF;
    ELSE
        RAISE EXCEPTION 'El estado solicitado no es válido.';
    END IF;

    SELECT id_estado_postulacion INTO nuevo FROM estado_postulacion WHERE nombre = p_nuevo_estado;
    UPDATE postulacion SET id_estado_postulacion = nuevo
    WHERE id_postulacion = p_id_postulacion AND id_estado_postulacion <> nuevo;
END;
$$;

CREATE PROCEDURE sp_vencer_ofertas(INOUT p_vencidas INTEGER)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE oferta_laboral
    SET id_estado_oferta = (SELECT id_estado_oferta FROM estado_oferta WHERE nombre = 'Vencida')
    WHERE id_estado_oferta = (SELECT id_estado_oferta FROM estado_oferta WHERE nombre = 'Publicada')
      AND fecha_vencimiento < CURRENT_DATE;
    GET DIAGNOSTICS p_vencidas = ROW_COUNT;
END;
$$;

CREATE VIEW vw_ofertas_vigentes AS
SELECT oferta.id_oferta, oferta.id_empresa, oferta.titulo,
       coalesce(empresa.nombre_empresa, oferta.empresa_externa) AS empresa,
       sector.nombre AS sector, oferta.departamento, oferta.detalle_ubicacion, oferta.tipo_empleo, oferta.modalidad,
       oferta.salario_min, oferta.salario_max, oferta.fecha_publicacion, oferta.fecha_vencimiento,
       oferta.puestos_vacantes - oferta.puestos_cubiertos AS vacantes_disponibles,
       oferta.fecha_vencimiento - CURRENT_DATE AS dias_restantes,
       ARRAY(SELECT habilidad.nombre FROM oferta_habilidad AS habilidad_requerida JOIN habilidad USING (id_habilidad)
             WHERE habilidad_requerida.id_oferta = oferta.id_oferta ORDER BY habilidad.nombre) AS habilidades
FROM oferta_laboral AS oferta
JOIN estado_oferta AS estado_de_oferta USING (id_estado_oferta)
JOIN sector USING (id_sector)
LEFT JOIN empresa USING (id_empresa)
WHERE estado_de_oferta.nombre = 'Publicada' AND oferta.fecha_vencimiento >= CURRENT_DATE
  AND oferta.puestos_cubiertos < oferta.puestos_vacantes;

CREATE VIEW vw_perfil_alumno_publico AS
SELECT alumno.id_alumno, registro.nombres, registro.apellidos, registro.foto_url, situacion.nombre AS situacion_academica,
       alumno.anio_egreso_graduacion, area.nombre AS area_especializacion,
       fn_condicion_laboral(alumno.id_alumno) AS condicion_laboral,
       fn_anios_experiencia(alumno.id_alumno) AS anios_experiencia,
       ARRAY(SELECT habilidad.nombre FROM alumno_habilidad AS habilidad_del_alumno JOIN habilidad USING (id_habilidad)
             WHERE habilidad_del_alumno.id_alumno = alumno.id_alumno ORDER BY habilidad.nombre) AS habilidades
FROM alumno
JOIN usuario USING (id_usuario)
JOIN estado_cuenta AS estado_de_cuenta USING (id_estado_cuenta)
JOIN datos_registro AS registro USING (id_usuario)
JOIN situacion_academica AS situacion USING (id_situacion)
LEFT JOIN area_especializacion AS area USING (id_area_esp)
WHERE estado_de_cuenta.nombre = 'Activa' AND fn_consentimiento_vigente(alumno.id_alumno);

CREATE VIEW vw_candidatos_oferta AS
SELECT postulacion.id_postulacion, postulacion.id_oferta, oferta.id_empresa, oferta.id_publicador, alumno.id_alumno,
       registro.nombres, registro.apellidos, situacion.nombre AS situacion_academica,
       fn_anios_experiencia(alumno.id_alumno) AS anios_experiencia,
       fn_afinidad(alumno.id_alumno, oferta.id_oferta) AS afinidad,
       estado_de_postulacion.nombre AS estado_postulacion, postulacion.fecha_postulacion, postulacion.fecha_resolucion
FROM postulacion
JOIN oferta_laboral AS oferta USING (id_oferta)
JOIN alumno USING (id_alumno)
JOIN datos_registro AS registro ON registro.id_usuario = alumno.id_usuario
JOIN situacion_academica AS situacion USING (id_situacion)
JOIN estado_postulacion AS estado_de_postulacion USING (id_estado_postulacion);

CREATE VIEW vw_mis_postulaciones AS
SELECT postulacion.id_postulacion, postulacion.id_alumno, postulacion.id_oferta, oferta.titulo,
       coalesce(empresa.nombre_empresa, oferta.empresa_externa) AS empresa,
       estado_de_postulacion.nombre AS estado_postulacion, estado_de_oferta.nombre AS estado_oferta,
       postulacion.fecha_postulacion, postulacion.fecha_resolucion
FROM postulacion
JOIN oferta_laboral AS oferta USING (id_oferta)
JOIN estado_oferta AS estado_de_oferta USING (id_estado_oferta)
JOIN estado_postulacion AS estado_de_postulacion USING (id_estado_postulacion)
LEFT JOIN empresa USING (id_empresa);

CREATE VIEW vw_solicitudes_pendientes AS
SELECT solicitud.id_solicitud, usuario.id_usuario, usuario.correo, rol.nombre AS rol,
       registro.nombres, registro.apellidos, registro.telefono, registro.cargo,
       alumno.carnet, situacion.nombre AS situacion_academica, empresa.nombre_empresa, empresa.nit,
       solicitud.fecha_solicitud, CURRENT_DATE - solicitud.fecha_solicitud::date AS dias_espera,
       solicitud.declaracion_veracidad
FROM solicitud_cuenta AS solicitud
JOIN estado_solicitud AS estado_de_solicitud USING (id_estado_solicitud)
JOIN usuario USING (id_usuario)
JOIN rol USING (id_rol)
LEFT JOIN datos_registro AS registro USING (id_usuario)
LEFT JOIN alumno USING (id_usuario)
LEFT JOIN situacion_academica AS situacion USING (id_situacion)
LEFT JOIN empresa USING (id_usuario)
WHERE estado_de_solicitud.nombre = 'Pendiente';

CREATE VIEW vw_seguimiento_alumnos AS
SELECT alumno.id_alumno, registro.nombres, registro.apellidos, alumno.carnet, situacion.nombre AS situacion_academica,
       alumno.anio_egreso_graduacion, area.nombre AS area_especializacion,
       fn_condicion_laboral(alumno.id_alumno) AS condicion_laboral,
       empleo_actual.empresa AS empresa_actual, empleo_actual.cargo AS cargo_actual,
       fn_anios_experiencia(alumno.id_alumno) AS anios_experiencia,
       fn_consentimiento_vigente(alumno.id_alumno) AS consentimiento_vigente
FROM alumno
JOIN usuario USING (id_usuario)
JOIN estado_cuenta AS estado_de_cuenta USING (id_estado_cuenta)
LEFT JOIN datos_registro AS registro USING (id_usuario)
JOIN situacion_academica AS situacion USING (id_situacion)
LEFT JOIN area_especializacion AS area USING (id_area_esp)
LEFT JOIN experiencia_laboral AS empleo_actual ON empleo_actual.id_experiencia = (
    SELECT experiencia.id_experiencia
    FROM experiencia_laboral AS experiencia
    WHERE experiencia.id_alumno = alumno.id_alumno AND experiencia.es_actual
    ORDER BY experiencia.fecha_inicio DESC, experiencia.id_experiencia DESC
    LIMIT 1
)
WHERE estado_de_cuenta.nombre = 'Activa';

CREATE VIEW vw_empleabilidad_por_anio AS
SELECT anio_egreso_graduacion, situacion_academica, count(*) AS total_alumnos,
       count(*) FILTER (WHERE condicion_laboral = 'Empleado') AS alumnos_con_empleo,
       round(100.0 * count(*) FILTER (WHERE condicion_laboral = 'Empleado') / count(*), 2) AS porcentaje_empleabilidad
FROM vw_seguimiento_alumnos WHERE anio_egreso_graduacion IS NOT NULL
GROUP BY anio_egreso_graduacion, situacion_academica;

CREATE VIEW vw_resumen_ofertas AS
SELECT oferta.id_oferta, oferta.id_empresa, oferta.id_publicador, oferta.titulo,
       coalesce(empresa.nombre_empresa, oferta.empresa_externa) AS empresa, estado_de_oferta.nombre AS estado_oferta,
       oferta.puestos_vacantes, oferta.puestos_cubiertos, oferta.puestos_vacantes - oferta.puestos_cubiertos AS vacantes_disponibles,
       count(*) FILTER (WHERE estado_de_postulacion.nombre = 'Recibida') AS recibidas,
       count(*) FILTER (WHERE estado_de_postulacion.nombre = 'En revisión') AS en_revision,
       count(*) FILTER (WHERE estado_de_postulacion.nombre = 'Seleccionado') AS seleccionadas,
       count(*) FILTER (WHERE estado_de_postulacion.nombre = 'Descartado') AS descartadas,
       count(*) FILTER (WHERE estado_de_postulacion.nombre = 'Retirada') AS retiradas
FROM oferta_laboral AS oferta
JOIN estado_oferta AS estado_de_oferta USING (id_estado_oferta)
LEFT JOIN empresa USING (id_empresa)
LEFT JOIN postulacion USING (id_oferta)
LEFT JOIN estado_postulacion AS estado_de_postulacion USING (id_estado_postulacion)
GROUP BY oferta.id_oferta, empresa.nombre_empresa, estado_de_oferta.nombre;

COMMIT;
