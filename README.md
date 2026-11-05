Trivia BAC
Sistema de preguntas y respuestas desarrollado en C# / ASP.NET Core MVC (.NET 8) con SQL Server. La aplicación implementa una arquitectura cliente-servidor donde todas las operaciones de base de datos se realizan exclusivamente mediante procedimientos almacenados, cumpliendo los requisitos de la prueba técnica.

📑 Índice
✨ Características
🏗️ Arquitectura
🛠️ Tecnologías
📋 Requisitos Previos
🚀 Instalación y Ejecución
🔒 Seguridad
🗄️ Modelo de Datos
✅ Cumplimiento de Requisitos
✨ Características
🔐 Autenticación de Usuarios
Registro de usuarios con validación de campos obligatorios.
Validación de usuarios duplicados.
Inicio de sesión mediante usuario y contraseña.
Contraseñas protegidas mediante PBKDF2-HMAC-SHA256.
Gestión de sesión mediante cookies seguras.
Protección contra ataques de fuerza bruta mediante Rate Limiting.
Protección CSRF para formularios y solicitudes AJAX.
🎯 Sistema de Trivia
Dos categorías iniciales:
🚗 Carros
🦁 Animales
Selección aleatoria de preguntas por categoría.
Cada partida contiene exactamente 3 preguntas.
Cada pregunta muestra 3 respuestas posibles.
Las respuestas aparecen mezcladas aleatoriamente.
Validación y calificación realizadas únicamente en el servidor.
El cliente nunca conoce cuáles son las respuestas correctas antes de responder.
🏆 Tablero de Posiciones
Ranking de jugadores.
Historial de puntajes.
Consulta de resultados almacenados en SQL Server.
Actualización automática al finalizar cada partida.
✅ Calidad y Pruebas
Pruebas unitarias con xUnit.
Scripts de verificación de datos.
Validación a nivel de:
Cliente (JavaScript)
Servidor (ASP.NET Core)
Base de datos (SQL Server)
🏗️ Arquitectura
Navegador (Razor + Bootstrap + jQuery/AJAX)
        │
Controllers
        │
Services
        │
Repositories (ADO.NET)
        │
SqlStoredProcedureRunner
        ▼
SQL Server (Stored Procedures)
Componentes Principales
TriviaApp
Controllers MVC
Vistas Razor
Endpoints JSON
Gestión de autenticación
Services
AuthService
TriviaService
Pbkdf2PasswordHasher
Repositories
UsuarioRepository
TriviaRepository
SqlStoredProcedureRunner
SQL Server
Procedimientos almacenados:

usp_RegistrarUsuario
usp_ObtenerUsuarioLogin
usp_ListarCategorias
usp_IniciarPartida
usp_ResponderPregunta
usp_TableroPosiciones
🛠️ Tecnologías
Backend
.NET 8 (LTS)
C# 12
ASP.NET Core MVC
ADO.NET
Microsoft.Data.SqlClient
SQL Server 2019+
Frontend
Razor Views
Bootstrap 5.3
JavaScript
jQuery 3.6
AJAX
Seguridad
PBKDF2-HMAC-SHA256
Antiforgery Tokens (CSRF)
Cookies HttpOnly
Rate Limiting
Consultas parametrizadas
Validación multicapa
Pruebas
xUnit
Validación de modelos
Reglas de autenticación
Verificación de hash de contraseñas
📋 Requisitos Previos
Antes de ejecutar el proyecto asegúrate de tener instalado:

.NET 8 SDK
SQL Server 2019+ (Express, Developer o LocalDB)
SQL Server Management Studio (SSMS)
Visual Studio 2022 o Visual Studio Code con C# Dev Kit
Git
🚀 Instalación y Ejecución
1️⃣ Crear la Base de Datos
Abrir y ejecutar:

database/01_TriviaBAC.sql
Opcionalmente ejecutar:

database/02_Verificacion.sql
También puede ejecutarse mediante línea de comandos:

sqlcmd -S localhost -E -I -f 65001 -i database/01_TriviaBAC.sql
2️⃣ Configurar la Cadena de Conexión
Archivo:

src/TriviaApp/appsettings.json
Autenticación Windows:

{
  "ConnectionStrings": {
    "TriviaBAC": "Server=localhost;Database=TriviaBAC;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
Autenticación SQL:

{
  "ConnectionStrings": {
    "TriviaBAC": "Server=localhost;Database=TriviaBAC;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"
  }
}
3️⃣ Ejecutar la Aplicación
cd src/TriviaApp

dotnet run
Abrir:

http://localhost:5080
Crear una cuenta y comenzar a jugar.

4️⃣ Ejecutar las Pruebas
dotnet test
🎮 Flujo de una Partida
1. Inicio de Partida
El procedimiento almacenado:

usp_IniciarPartida
Crea una nueva partida.
Selecciona 3 preguntas aleatorias.
Guarda las preguntas asignadas.
No devuelve la respuesta correcta al cliente.
2. Respuesta del Usuario
El cliente envía:

{
  "partidaId": 1,
  "preguntaId": 5,
  "respuestaId": 12
}
El procedimiento:

usp_ResponderPregunta
Valida:

Que la partida pertenezca al usuario.
Que la pregunta pertenezca a la partida.
Que no haya sido respondida.
Que la respuesta exista.
3. Finalización
Al responder la tercera pregunta:

Se calcula la puntuación.
Se marca la partida como finalizada.
Se actualiza el tablero de posiciones.
Todo el cálculo se realiza en el servidor.

🔒 Seguridad
Contraseñas almacenadas mediante PBKDF2-HMAC-SHA256 con sal individual.
Nunca se almacenan contraseñas en texto plano.
Protección CSRF para formularios y AJAX.
Validación tanto en cliente como en servidor.
Restricciones y validaciones en base de datos.
Consultas ejecutadas únicamente mediante procedimientos almacenados.
Redirección segura tras autenticación.
Soporte para HTTPS y HSTS en producción.
Cookies HttpOnly para proteger la sesión.
Mensajes genéricos de autenticación para evitar filtraciones de información.
🗄️ Modelo de Datos
Entidades Principales
Usuarios
Categorías
Preguntas
Respuestas
Partidas
PartidaPreguntas
Relación General
Usuarios
   │
Partidas
   │
PartidaPreguntas
   │
Preguntas
   │
Respuestas

Categorias
   │
Preguntas
✅ Cumplimiento de Requisitos
Requisito	Estado
Registro de usuarios	✅ Implementado
Inicio de sesión	✅ Implementado
Categorías disponibles al ingresar	✅ Implementado
Partidas de 3 preguntas	✅ Implementado
Respuestas aleatorias	✅ Implementado
Mínimo 10 preguntas por categoría	✅ Implementado
Procedimientos almacenados obligatorios	✅ Implementado
Tablero de posiciones	✅ Implementado
Validaciones de seguridad	✅ Implementado
Pruebas unitarias	✅ Implementado
🌟 Características Destacadas
Arquitectura cliente-servidor moderna.
SQL Server con acceso exclusivo mediante procedimientos almacenados.
Calificación segura realizada únicamente en el servidor.
Contraseñas protegidas con PBKDF2.
AJAX para una experiencia fluida sin recargas de página.
Bootstrap 5 para diseño responsive.
Ranking de jugadores integrado.
Pruebas unitarias para garantizar calidad y mantenimiento.
📄 Licencia
Proyecto desarrollado con fines académicos y de evaluación técnica.
