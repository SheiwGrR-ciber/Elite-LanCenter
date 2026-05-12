// =============================================
// ELITE LAN CENTER - MODELO DE USUARIO
// =============================================

namespace EliteLanCenter.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public string CreadoEn { get; set; } = string.Empty;

        public bool EsAdmin => Rol == "AdminDueno" || Rol == "AdminEncargado";
        public bool EsOperador => Rol == "Operador";
        public bool EsAdminDueno { get; set; }
        public bool EsAdminEncargado { get; set; }
        public string Iniciales { get; set; } = string.Empty;

        public string RolDescripcion => Rol switch
        {
            "AdminDueno" => "Admin Dueño",
            "AdminEncargado" => "Admin Encargado",
            "Operador" => "Operador",
            _ => "Desconocido"
        };
    }
}