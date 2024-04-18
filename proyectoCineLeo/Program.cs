using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System.Xml;
using System.Text;
using GeneroleoOntology;
using PeliculaleoOntology;
using PersonaleoOntology;
using Newtonsoft.Json.Linq;
using GnossBase;
using Gnoss.ApiWrapper.Helpers;

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

        // Cargar una persona de prueba

        mResourceApi.ChangeOntology("personaleo.owl");
        Person persona1 = new Person();
        persona1.Schema_name = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>()
        {
            {GnossOCBase.LanguageEnum.es, "Persona prueba"}
        };

        ComplexOntologyResource resorceLoad = persona1.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        mResourceApi.LoadComplexSemanticResource(resorceLoad);

        // Modificacion de persona de prueba

        string uri = getUriRecurso("Persona prueba","personaleo",mResourceApi);

        string[] partes = uri.Split('/', '_');

        string resourceId = partes[5];
        string articleID = partes[6];

        Person persona1Modificado = new Person();
        persona1Modificado.Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() { 
            { GnossOCBase.LanguageEnum.es,"Persona prueba modificado" } 
        };

        mResourceApi.ModifyComplexOntologyResource(persona1Modificado.ToGnossApiResource(mResourceApi, null, new Guid(resourceId), new Guid(articleID)), false, true);

        // Eliminar la persona de prueba

        uri = getUriRecurso("Persona prueba modificado", "personaleo", mResourceApi);
        
        try
        {
            mResourceApi.ChangeOntology("personaleo.owl");
            mResourceApi.PersistentDelete(mResourceApi.GetShortGuid(uri), true, true);
        }
        catch (Exception ex)
        {
            mResourceApi.Log.Error(ex.ToString());
        }


        #endregion Basico

        #endregion Segunda Parte
    }

    public static string getUriRecurso(string nombre, string ontologia, ResourceApi mResourceApi)
    {
        string uri = "";

        //Obtención del id de la persona cargada en la comunidad
        string pOntology = ontologia;
        string select = string.Empty, where = string.Empty;
        select += $@"SELECT DISTINCT ?s";
        where += $@" WHERE {{ ";
        where += $@"OPTIONAL{{?s ?p '{nombre}'@es.}}";
        where += $@"}}";

        SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, pOntology);
        //Si está ya en el grafo, obtengo la URI
        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0 && resultadoQuery.results.bindings.FirstOrDefault()?.Keys.Count > 0)
        {
            foreach (var item in resultadoQuery.results.bindings)
            {
                uri = item["s"].value;
            }
        }
        return uri;
    }
}