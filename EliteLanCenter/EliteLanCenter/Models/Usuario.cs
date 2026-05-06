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

        // Propiedades calculadas
        public bool EsAdmin => Rol == "AdminDueno" || Rol == "AdminEncargado";
        public bool EsOperador => Rol == "Operador";

        public string RolDescripcion => Rol switch
        {
            "AdminDueno" => "Administrador Dueño",
            "AdminEncargado" => "Administrador Encargado",
            "Operador" => "Operador",
            _ => "Desconocido"
        };
    }
}