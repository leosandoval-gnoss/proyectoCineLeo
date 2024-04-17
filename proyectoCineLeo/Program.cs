using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System.Xml;
using System.Text;
using GeneroleoOntology;
using PeliculaleoOntology;
using PersonaleoOntology;
using Newtonsoft.Json.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        #region Primera parte
        #region Conexión y datos de la comunidad
        string pathOAuth = @"Config\oAuth.config";
        ResourceApi mResourceApi = new ResourceApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        CommunityApi mCommunityApi = new CommunityApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        ThesaurusApi mThesaurusApi = new ThesaurusApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        UserApi mUserApi = new UserApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));

        Console.WriteLine($"Id de la Comunidad -> {mCommunityApi.GetCommunityId()}");
        Console.WriteLine($"Nombre de la Comunidad -> {mCommunityApi.GetCommunityInfo().name}");
        Console.WriteLine($"Nombre Corto de la Comunidad -> {mCommunityApi.GetCommunityInfo().short_name}");
        Console.WriteLine($"Descripción de la comunidad inicial -> {mCommunityApi.GetCommunityInfo().description}");
        Console.WriteLine($"Categorías de la Comunidad -> {string.Join(", ", mCommunityApi.CommunityCategories.Select(categoria => categoria.category_name))}");

        #endregion Conexión con la comunidad

        #endregion Primera Parte

        #region Segunda Parte

        #region Basico

        // Cargar un genero de prueba

        string identificador = Guid.NewGuid().ToString(); //Se pone en el grafo de ontología
        Genre genero = new(identificador); //Se pone en el grafo de búsqueda
        genero.Schema_name = "Genero de prueba";
        mResourceApi.ChangeOntology("generoleo.owl");
        SecondaryResource generoSR = genero.ToGnossApiResource(mResourceApi, $"Genre_{identificador}");
        string mensajeFalloCarga = $"Error en la carga del Género con identificador {identificador} -> Nombre: {genero.Schema_name}";
        try
        {
            mResourceApi.LoadSecondaryResource(generoSR);
            if (!generoSR.Uploaded)
            {
                mResourceApi.Log.Error(mensajeFalloCarga);
            }
        }
        catch (Exception)
        {
            mResourceApi.Log.Error($"Exception -> {mensajeFalloCarga}");
        }

        #endregion Basico

        #endregion Segunda Parte
    }
}