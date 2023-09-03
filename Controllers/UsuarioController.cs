using Microsoft.AspNetCore.Mvc;
using Firebase.Auth;
using Firebase.Storage;
using System.Data.SqlClient;
using System.Data;
using StorageFirebase.Models;
using Microsoft.CodeAnalysis.Host;

namespace StorageFirebase.Controllers
{
    public class UsuarioController : Controller
    {
        //hola
        private readonly string? cadenaSQL;
        public readonly string? firebaseConfig;
        
        public UsuarioController(IConfiguration configuration)
        {
            cadenaSQL = configuration.GetConnectionString("CadenaSQL");
            firebaseConfig = configuration.GetValue<string>("FireBase:ApiKey");
        }
        public IActionResult Crear()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(UsuarioModel oUsuario, IFormFile Imagen)
        {  
            Stream imagen = Imagen.OpenReadStream();
            string url_imagen = await SubirStorage(imagen, Imagen.FileName);

            using(var con = new SqlConnection(cadenaSQL))
            {
                con.Open();
                var cmd = new SqlCommand("Guardar", con);
                cmd.Parameters.AddWithValue("nombre", oUsuario.nombre);
                cmd.Parameters.AddWithValue("telefono", oUsuario.telefono);
                cmd.Parameters.AddWithValue("url_imagen", url_imagen);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var oListaUsuarios = new List<UsuarioModel>();


            using (var con = new SqlConnection(cadenaSQL))
            {
                con.Open();
                var cmd = new SqlCommand("Listar", con);
                cmd.CommandType = CommandType.StoredProcedure;

                using(var dr  = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        oListaUsuarios.Add(new UsuarioModel() { 
                            nombre = dr["nombre"].ToString(),
                            telefono = dr["telefono"].ToString(),
                            url_imagen = dr["url_imagen"].ToString()
                        
                        });
                    }
                }
            }
            return View(oListaUsuarios);
        }
        public async Task<string> SubirStorage(Stream archivo, string nombre)
        {
            string email = "test@gmail.com";
            string clave = "test123";
            string ruta = "fir-storage-5e5ea.appspot.com";
            string api_key = firebaseConfig;

            var authConfig = new FirebaseAuthProvider(new FirebaseConfig(api_key));
            var authentication = await authConfig.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                ruta,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(authentication.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("fotos_perfil")
                .Child(nombre)
                .PutAsync(archivo,cancellation.Token);

            var downloadURL = await task;
            return downloadURL;

        }
    }
}
