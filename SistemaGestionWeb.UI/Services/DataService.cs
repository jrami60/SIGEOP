using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SistemaGestionWeb.UI.Models;

namespace SistemaGestionWeb.UI.Services;

public class DataService
{
    private readonly HttpClient _http;

    private const string SupabaseUrl = "https://xdbeuralzhcsyiumortw.supabase.co/";
    private const string SupabaseKey = "sb_publishable_IzpQxdgDfq4hmGawKNGO4g_UIRTA4kt";

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public DataService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(SupabaseUrl)
        };

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("apikey", SupabaseKey);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
    }

    public async Task<List<MiembroModel>> ObtenerMiembrosDeOrganizacionAsync(string organizacionId)
    {
        try
        {
            // Consulta limpia que solo pide los campos directos de la tabla intermedia sin joins complejos que fallen
            string query = $"rest/v1/usuario_organizaciones?organizacion_id=eq.{organizacionId}&select=rol_en_org,id_usuario,usuarios!id_usuario(nombre)";
            var response = await _http.GetFromJsonAsync<List<MiembroModel>>(query, _jsonOptions);

            var listaMiembros = response ?? new List<MiembroModel>();

            foreach (var miembro in listaMiembros)
            {
                if (miembro.Usuario == null)
                {
                    miembro.Usuario = new UsuarioData { Nombre = $"Usuario ID: {miembro.IdUsuario}" };
                }
            }

            return listaMiembros;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error en ObtenerMiembrosDeOrganizacionAsync]: {ex.Message}");
            return new List<MiembroModel>();
        }
    }

    public async Task<List<Registro>> ObtenerTareasAsync(string organizacionId = "")
    {
        try
        {
            string url = "rest/v1/registros?select=*";

            if (!string.IsNullOrEmpty(organizacionId))
            {
                url += $"&organizacion_id=eq.{organizacionId}";
            }

            var resultado = await _http.GetFromJsonAsync<List<Registro>>(url, _jsonOptions);
            return resultado ?? new List<Registro>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al obtener tareas]: {ex.Message}");
            return new List<Registro>();
        }
    }

    public async Task<List<Registro>> ObtenerRegistrosPorOrganizacionAsync(string organizacionId)
    {
        return await ObtenerTareasAsync(organizacionId);
    }

   public async Task<bool> CrearTareaAsync(Registro tarea)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tarea.OrganizacionId))
            {
                Console.WriteLine("[Error]: No se puede crear la tarea porque falta el ID de la organización.");
                return false;
            }

            var payload = new
            {
                titulo = tarea.Titulo,
                descripcion = tarea.Descripcion,
                id_categoria = tarea.CategoriaId,
                id_estado = tarea.EstadoId > 0 ? tarea.EstadoId : 1,
                organizacion_id = tarea.OrganizacionId,
                id_usuario = tarea.UsuarioId // <--- ¡AQUÍ ESTABA FALTANDO!
            };

            var bodyJson = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "rest/v1/registros");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Headers.Add("Prefer", "return=representation");
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error Supabase al crear tarea]: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción C#]: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ActualizarTareaAsync(Registro tarea)
    {
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"rest/v1/registros?id_registro=eq.{tarea.Id}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Headers.Add("Prefer", "return=representation");

            var payload = new Dictionary<string, object?>
            {
                { "titulo", tarea.Titulo },
                { "descripcion", tarea.Descripcion },
                { "id_categoria", tarea.CategoriaId },
                { "organizacion_id", tarea.OrganizacionId },
                { "foto_url", tarea.FotoUrl },
                { "id_estado", tarea.EstadoId } 
            };

            request.Content = JsonContent.Create(payload);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error Supabase 400]: {errorDetails}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción C# al actualizar tarea]: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> ActualizarEstadoTareaAsync(int idTarea, int nuevoEstado)
    {
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"rest/v1/registros?id_registro=eq.{idTarea}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Content = JsonContent.Create(new { id_estado = nuevoEstado });

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción C# al actualizar]: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EditarTareaAsync(int idTarea, string nuevoTitulo, string nuevaDescripcion)
    {
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"rest/v1/registros?id_registro=eq.{idTarea}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Content = JsonContent.Create(new { titulo = nuevoTitulo, descripcion = nuevaDescripcion });

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al editar tarea]: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EliminarTareaAsync(int idTarea)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"rest/v1/registros?id_registro=eq.{idTarea}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al eliminar tarea]: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Categoria>> ObtenerCategoriasAsync(string organizacionId = "")
    {
        try
        {
            string url = "rest/v1/categorias?select=*";
            if (!string.IsNullOrEmpty(organizacionId))
            {
                url += $"&organizacion_id=eq.{organizacionId}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Categoria>>(json, _jsonOptions) ?? new List<Categoria>();
            }
            return new List<Categoria>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción ObtenerCategorias]: {ex.Message}");
            return new List<Categoria>();
        }
    }

    public async Task<bool> EliminarCategoriaAsync(int idCategoria)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"rest/v1/categorias?id=eq.{idCategoria}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción C# al eliminar categoría]: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Organizacion>> ObtenerOrganizacionesAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "rest/v1/organizaciones?select=*");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<Organizacion>>(json, _jsonOptions);
                return lista ?? new List<Organizacion>();
            }

            return new List<Organizacion>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al obtener organizaciones]: {ex.Message}");
            return new List<Organizacion>();
        }
    }

    public async Task<bool> CrearOrganizacionAsync(string idUsuario, string nombreOrg)
    {
        try
        {
            string idOrg = Guid.NewGuid().ToString();
            var random = new Random();
            string codigoOrg = $"ORG-{random.Next(1000, 9999)}";

            var nuevaOrg = new { id = idOrg, nombre = nombreOrg, codigo = codigoOrg };
            var jsonOrg = System.Text.Json.JsonSerializer.Serialize(nuevaOrg);
            var contentOrg = new StringContent(jsonOrg, Encoding.UTF8, "application/json");

            var requestOrg = new HttpRequestMessage(HttpMethod.Post, "rest/v1/organizaciones");
            requestOrg.Headers.Add("apikey", SupabaseKey);
            requestOrg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            requestOrg.Headers.Add("Prefer", "return=minimal");
            requestOrg.Content = contentOrg;

            var responseOrg = await _http.SendAsync(requestOrg);
            if (!responseOrg.IsSuccessStatusCode)
            {
                var errOrg = await responseOrg.Content.ReadAsStringAsync();
                Console.WriteLine($"ERROR EN ORGANIZACIONES: {errOrg}");
                return false;
            }

            if (!int.TryParse(idUsuario, out int parsedUserId))
            {
                Console.WriteLine($"ERROR: El idUsuario '{idUsuario}' no se pudo parsear a entero.");
                return false;
            }

            var relacion = new { id_usuario = parsedUserId, organizacion_id = idOrg, rol_en_org = "admin" };
            var jsonRel = System.Text.Json.JsonSerializer.Serialize(relacion);
            var contentRel = new StringContent(jsonRel, Encoding.UTF8, "application/json");

            var requestRel = new HttpRequestMessage(HttpMethod.Post, "rest/v1/usuario_organizaciones");
            requestRel.Headers.Add("apikey", SupabaseKey);
            requestRel.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            requestRel.Headers.Add("Prefer", "return=minimal");
            requestRel.Content = contentRel;

            var responseRel = await _http.SendAsync(requestRel);

            if (!responseRel.IsSuccessStatusCode)
            {
                var errRel = await responseRel.Content.ReadAsStringAsync();
                Console.WriteLine($"ERROR EXACTO DE SUPABASE EN USUARIO_ORGANIZACIONES: {errRel}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPCIÓN CRÍTICA EN C#: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UnirsePorCodigoAsync(string idUsuario, string codigoOrg)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigoOrg)) return false;

            var codigoLimpio = Uri.EscapeDataString(codigoOrg.Trim().ToUpper());
            var requestGet = new HttpRequestMessage(HttpMethod.Get, $"{SupabaseUrl}/rest/v1/organizaciones?codigo=eq.{codigoLimpio}");

            requestGet.Headers.Add("apikey", SupabaseKey);
            requestGet.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            requestGet.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var responseGet = await _http.SendAsync(requestGet);
            if (!responseGet.IsSuccessStatusCode)
            {
                var errorGet = await responseGet.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error al buscar organización]: {errorGet}");
                return false;
            }

            var contentStream = await responseGet.Content.ReadAsStreamAsync();
            var organizaciones = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Organizacion>>(
                contentStream,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var organizacion = organizaciones?.FirstOrDefault();
            if (organizacion == null)
            {
                Console.WriteLine($"[Aviso]: No se encontró ninguna organización con el código: {codigoOrg}");
                return false;
            }

            object relacion = int.TryParse(idUsuario, out var idInt)
                ? new { id_usuario = idInt, organizacion_id = organizacion.Id, rol_en_org = "pendiente" }
                : new { id_usuario = idUsuario, organizacion_id = organizacion.Id, rol_en_org = "pendiente" };

            var jsonRel = System.Text.Json.JsonSerializer.Serialize(relacion);
            var contentRel = new StringContent(jsonRel, Encoding.UTF8, "application/json");

            var requestRel = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseUrl}/rest/v1/usuario_organizaciones");
            requestRel.Headers.Add("apikey", SupabaseKey);
            requestRel.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            requestRel.Headers.Add("Prefer", "return=minimal");
            requestRel.Content = contentRel;

            var responseRel = await _http.SendAsync(requestRel);

            if (!responseRel.IsSuccessStatusCode)
            {
                var errorRel = await responseRel.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error al insertar en usuario_organizaciones]: {errorRel}");
            }

            return responseRel.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción al unirse por código]: {ex.Message}");
            return false;
        }
    }

    public async Task<string> RegenerarCodigoAsync(string idOrg)
    {
        try
        {
            var random = new Random();
            string nuevoCodigo = $"ORG-{random.Next(1000, 9999)}";

            var payload = new { codigo = nuevoCodigo };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, $"{SupabaseUrl}/rest/v1/organizaciones?id=eq.{idOrg}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            request.Headers.Add("Prefer", "return=minimal");
            request.Content = content;

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode ? nuevoCodigo : string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al regenerar código]: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<List<Organizacion>> ObtenerOrganizacionesPorUsuarioAsync(int usuarioId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"rest/v1/usuario_organizaciones?id_usuario=eq.{usuarioId}&rol_en_org=neq.pendiente&select=rol_en_org,organizaciones(*)");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<Organizacion>();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var listaFinal = new List<Organizacion>();

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                string rolOrg = "miembro";
                if (element.TryGetProperty("rol_en_org", out var rolProp) && rolProp.ValueKind == JsonValueKind.String)
                {
                    rolOrg = rolProp.GetString() ?? "miembro";
                }

                if (element.TryGetProperty("organizaciones", out var orgProp) && orgProp.ValueKind == JsonValueKind.Object)
                {
                    var org = JsonSerializer.Deserialize<Organizacion>(orgProp.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (org != null)
                    {
                        org.Rol = rolOrg;
                        listaFinal.Add(org);
                    }
                }
            }

            return listaFinal;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener organizaciones: {ex.Message}");
            return new List<Organizacion>();
        }
    }

    public async Task<Usuario?> ObtenerUsuarioPorEmailAsync(string email)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"rest/v1/usuarios?email=eq.{Uri.EscapeDataString(email)}&select=*");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var usuarios = System.Text.Json.JsonSerializer.Deserialize<List<Usuario>>(json, options);

            return usuarios?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar usuario: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> VerificarCorreoExisteAsync(string email)
    {
        var usuario = await ObtenerUsuarioPorEmailAsync(email);
        return usuario != null;
    }

    public async Task<bool> RegistrarUsuarioAsync(string nombre, string email, string password)
    {
        try
        {
            var payload = new
            {
                nombre = nombre,
                email = email,
                password_hash = password,
                id_rol = 2
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "rest/v1/usuarios");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Headers.Add("Prefer", "return=minimal");
            request.Content = content;

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error detallado de Supabase]: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al registrar usuario]: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ActualizarEstadoSolicitudAsync(int usuarioId, string orgId, string nuevoRol)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, $"rest/v1/usuario_organizaciones?id_usuario=eq.{usuarioId}&organizacion_id=eq.{orgId}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var body = new { rol_en_org = nuevoRol };
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar estado de solicitud: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Dictionary<string, object>>> ObtenerSolicitudesPendientesAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SupabaseUrl}/rest/v1/usuario_organizaciones?rol_en_org=eq.pendiente");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<Dictionary<string, object>>();

            var contentStream = await response.Content.ReadAsStreamAsync();
            var resultado = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Dictionary<string, object>>>(
                contentStream,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return resultado ?? new List<Dictionary<string, object>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al obtener pendientes]: {ex.Message}");
            return new List<Dictionary<string, object>>();
        }
    }

    public async Task<bool> ActualizarEstadoRolAsync(string usuarioId, string organizacionId, string nuevoRol)
    {
        try
        {
            var data = new { rol_en_org = nuevoRol };
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, $"{SupabaseUrl}/rest/v1/usuario_organizaciones?id_usuario=eq.{usuarioId}&organizacion_id=eq.{organizacionId}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            request.Headers.Add("Prefer", "return=minimal");
            request.Content = content;

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al actualizar rol]: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EliminarRelacionUsuarioOrganizacionAsync(string usuarioId, string organizacionId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{SupabaseUrl}/rest/v1/usuario_organizaciones?id_usuario=eq.{usuarioId}&organizacion_id=eq.{organizacionId}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            request.Headers.Add("Prefer", "return=minimal");

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error Supabase Delete]: {response.StatusCode} - {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción al eliminar relación]: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Dictionary<string, object>>> ObtenerSolicitudesPendientesPorAdminAsync(int usuarioIdAdmin)
    {
        try
        {
            var orgsAdmin = await ObtenerOrganizacionesPorUsuarioAsync(usuarioIdAdmin);

            if (orgsAdmin == null || !orgsAdmin.Any()) return new List<Dictionary<string, object>>();

            var orgIds = orgsAdmin
                .Where(o => !string.IsNullOrEmpty(o.Rol) &&
                        (o.Rol.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                            o.Rol.Equals("administrador", StringComparison.OrdinalIgnoreCase)))
                .Select(o => o.Id)
                .ToList();

            if (!orgIds.Any()) return new List<Dictionary<string, object>>();

            string idsCsv = string.Join(",", orgIds);
            string url = $"{SupabaseUrl}/rest/v1/usuario_organizaciones?select=*,usuarios(nombre)&rol_en_org=eq.pendiente&organizacion_id=in.({idsCsv})";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<Dictionary<string, object>>();

            var contentStream = await response.Content.ReadAsStreamAsync();
            var resultado = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Dictionary<string, object>>>(
                contentStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return resultado ?? new List<Dictionary<string, object>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al obtener pendientes del admin]: {ex.Message}");
            return new List<Dictionary<string, object>>();
        }
    }
    public async Task<List<OrgConMiembrosDto>> ObtenerOrganizacionesParaPerfilAsync(int usuarioId)
    {
        try
        {
            var orgs = await ObtenerOrganizacionesPorUsuarioAsync(usuarioId);
            var resultado = new List<OrgConMiembrosDto>();

            foreach (var org in orgs)
            {
                var miembros = await ObtenerMiembrosDeOrganizacionAsync(org.Id);
                resultado.Add(new OrgConMiembrosDto
                {
                    IdOrganizacion = org.Id,
                    NombreOrganizacion = org.Nombre,
                    RolUsuarioActual = org.Rol ?? "miembro",
                    Miembros = miembros
                });
            }

            return resultado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error en ObtenerOrganizacionesParaPerfilAsync]: {ex.Message}");
            return new List<OrgConMiembrosDto>();
        }
    }
    public async Task<bool> ActualizarPasswordAsync(int usuarioId, string passwordActual, string nuevoPasswordHash)
    {
        try
        {

            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"rest/v1/usuarios?id=eq.{usuarioId}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Content = JsonContent.Create(new { password_hash = nuevoPasswordHash });

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al actualizar contraseña]: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> ActualizarEstadoRegistroAsync(int idRegistro, int nuevoEstadoId)
    {
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"rest/v1/registros?id_registro=eq.{idRegistro}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Headers.Add("Prefer", "return=representation");

            var payload = new Dictionary<string, object?>
            {
                { "id_estado", nuevoEstadoId }
            };

            request.Content = JsonContent.Create(payload);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción al actualizar estado]: {ex.Message}");
            return false;
        }
    }
    public async Task<Categoria?> CrearCategoriaAsync(string nombre, string colorHex, string organizacionId)
    {
        try
        {
            var nuevaCat = new Dictionary<string, object>
            {
                { "nombre", nombre },
                { "color_hex", colorHex },
                { "organizacion_id", organizacionId }
            };

            var content = new StringContent(JsonSerializer.Serialize(nuevaCat), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "rest/v1/categorias");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Headers.Add("Prefer", "return=representation");
            request.Content = content;

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var listaCreada = JsonSerializer.Deserialize<List<Categoria>>(json, _jsonOptions);
                return listaCreada?.FirstOrDefault();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción CrearCategoria]: {ex.Message}");
            return null;
        }
    }
    public async Task<bool> ActualizarRolMiembroAsync(string organizacionId, int usuarioId, string nuevoRol)
    {
        try
        {
            var payload = new { rol_en_org = nuevoRol };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Filtramos directamente por las columnas de la tabla intermedia
            string patchUrl = $"rest/v1/usuario_organizaciones?organizacion_id=eq.{organizacionId}&id_usuario=eq.{usuarioId}";

            using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);
            patchRequest.Headers.Add("apikey", SupabaseKey);
            patchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            patchRequest.Headers.Add("Prefer", "return=minimal");
            patchRequest.Content = content;

            var response = await _http.SendAsync(patchRequest);
            Console.WriteLine($"[Respuesta PATCH Supabase]: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción ActualizarRol]: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> ActualizarRolPorRelacionAsync(int relacionId, string nuevoRol)
    {
        try
        {
            if (relacionId <= 0)
            {
                Console.WriteLine("[Error]: El relacionId llegó como 0 o negativo.");
                return false;
            }

            var payload = new { rol_en_org = nuevoRol };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Actualizamos directo por la llave primaria 'id' de la tabla usuario_organizaciones
            string patchUrl = $"rest/v1/usuario_organizaciones?id=eq.{relacionId}";

            using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);
            patchRequest.Headers.Add("apikey", SupabaseKey);
            patchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            patchRequest.Headers.Add("Prefer", "return=minimal");
            patchRequest.Content = content;

            var response = await _http.SendAsync(patchRequest);
            Console.WriteLine($"[Respuesta PATCH por ID]: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción]: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> ActualizarRolPorOrgYUsuarioAsync(string organizacionId, int usuarioId, string nuevoRol)
    {
        try
        {
            var payload = new { rol_en_org = nuevoRol };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            string patchUrl = $"rest/v1/usuario_organizaciones?organizacion_id=eq.{organizacionId}&id_usuario=eq.{usuarioId}";

            using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);
            patchRequest.Headers.Add("apikey", SupabaseKey);
            patchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            patchRequest.Headers.Add("Prefer", "return=minimal");
            patchRequest.Content = content;

            var response = await _http.SendAsync(patchRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción]: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> ArchivarRegistroAsync(int registroId)
    {
        try
        {
            var payload = new { archivado = true };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            string patchUrl = $"rest/v1/registros?id_registro=eq.{registroId}";

            using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);
            patchRequest.Headers.Add("apikey", SupabaseKey);
            patchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            patchRequest.Headers.Add("Prefer", "return=minimal");
            patchRequest.Content = content;

            var response = await _http.SendAsync(patchRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al archivar]: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EliminarRegistroAsync(int registroId)
    {
        try
        {
            string deleteUrl = $"rest/v1/registros?id_registro=eq.{registroId}";

            using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            deleteRequest.Headers.Add("apikey", SupabaseKey);
            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var response = await _http.SendAsync(deleteRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error al eliminar]: {ex.Message}");
            return false;
        }
    }
    public async Task<List<Registro>> ObtenerRegistrosActivosParaDashboardAsync(string organizacionId)
    {
        try
        {
            string url = $"rest/v1/registros?organizacion_id=eq.{organizacionId}&archivado=eq.false&select=*";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<Registro>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Registro>>(json, _jsonOptions) ?? new List<Registro>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error]: {ex.Message}");
            return new List<Registro>();
        }
    }
    public async Task<Registro?> ObtenerRegistroPorIdAsync(int id)
    {
        try
        {
            // Cambiado de id=eq a id_registro=eq para que coincida con tu tabla de Supabase
            string url = $"rest/v1/registros?id_registro=eq.{id}&select=*";

            var response = await _http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var registros = await response.Content.ReadFromJsonAsync<List<Registro>>(_jsonOptions);
                return registros?.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener registro por ID: {ex.Message}");
        }
        return null;
    }
    public async Task<bool> ReabrirTareaCompletaAsync(Registro tarea)
    {
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"rest/v1/registros?id_registro=eq.{tarea.Id}");
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            request.Headers.Add("Prefer", "return=representation");

            var payload = new Dictionary<string, object?>
            {
                { "id_estado", 1 },
                { "titulo", tarea.Titulo },
                { "descripcion", tarea.Descripcion },
                { "id_categoria", tarea.CategoriaId },
                { "organizacion_id", tarea.OrganizacionId },
                { "foto_url", tarea.FotoUrl }
            };

            request.Content = JsonContent.Create(payload);
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excepción al reabrir tarea]: {ex.Message}");
            return false;
        }
    }
    public async Task<UsuarioData?> ObtenerUsuarioPorIdAsync(int usuarioId)
    {
        return await _http.GetFromJsonAsync<UsuarioData>($"api/usuarios/{usuarioId}");
    }

    public async Task ActualizarFotoPerfilUsuarioAsync(int usuarioId, string base64)
    {
        var content = JsonContent.Create(new { foto_url = base64 });
        await _http.PutAsync($"api/usuarios/{usuarioId}/foto", content);
    }
}