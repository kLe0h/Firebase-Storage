using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace StorageFirebase.Models
{
    public class UsuarioModel
    {

        public int id_usuario { get; set; }
        public string? nombre { get; set; }
        public string? telefono { get; set; }
        public string? url_imagen { get; set; }
    }

}
