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

internal class Program
{
    private static void Main(string[] args)
    {
        #region Conexión y datos de la comunidad
        string pathOAuth = @"Config\oAuth.config";
        ResourceApi mResourceApi = new ResourceApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        CommunityApi mCommunityApi = new CommunityApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        ThesaurusApi mThesaurusApi = new ThesaurusApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        UserApi mUserApi = new UserApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));

        Console.WriteLine($"Id de la Comunidad -> {mCommunityApi.GetCommunityId()}");

        #endregion Conexión con la comunidad

        #region Carga de una persona

        mResourceApi.ChangeOntology("personaleo.owl");

        Person persona = new();

        persona.Schema_name = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "Prueba de actor" } };

        ComplexOntologyResource resorceLoad = persona.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        mResourceApi.LoadComplexSemanticResource(resorceLoad);
        #endregion Carga de una persona

        #region Carga de una pelicula

        mResourceApi.ChangeOntology("peliculaleo.owl");

        Movie pelicula = new();

        pelicula.Schema_name = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "Prueba de pelicula" } };
        pelicula.Schema_description  = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "Lore Ipsum" } };
        pelicula.Schema_image = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "." } };
        pelicula.Schema_contentRating = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "N/A" } };

        ComplexOntologyResource resorceToLoad = pelicula.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        string idPeliculaCargada = mResourceApi.LoadComplexSemanticResource(resorceToLoad);
        #endregion Carga de un pelicula

        #region Carga de un genereo

        mResourceApi.ChangeOntology("generoleo.owl");

        string identificador = Guid.NewGuid().ToString();
        Genre genero = new(identificador);
        genero.Schema_name = "Genero de prueba";
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
        #endregion Carga de un genreo

        #region Modificar pelicula

        Guid idCortoPelicula = mResourceApi.GetShortGuid(getUriPelicula("Prueba de pelicula",mResourceApi));

        // Para indicar que es un auxiliar de la entidad principal se tienen que separar sus valores por tuberías '|'
        string predicadoBase = "http://schema.org/genre";

        string valorEntidadAuxiliar = getUriGenero("Genero de prueba", mResourceApi);

        List<TriplesToInclude> listaTriplesIncluir = new List<TriplesToInclude>();

        // Fuente de la valoración
        listaTriplesIncluir.Add(new TriplesToInclude
        {
            Description = false,
            Title = false,
            Predicate = predicadoBase,
            NewValue = valorEntidadAuxiliar
        });

        Dictionary<Guid, List<TriplesToInclude>> diccIncluirTriples = new Dictionary<Guid, List<TriplesToInclude>>();
        diccIncluirTriples.Add(idCortoPelicula, listaTriplesIncluir);

        Dictionary<Guid, bool> dicInsertado = mResourceApi.InsertPropertiesLoadedResources(diccIncluirTriples);

        // Comprobamos si se ha incluido corerctamente
        if (dicInsertado != null && dicInsertado.ContainsKey(idCortoPelicula) && dicInsertado[idCortoPelicula])
        {
            mResourceApi.Log.Info("Se ha incluido con exito el triple");
        }
        else
        {
            mResourceApi.Log.Error($"Error al incluir la entidad auxiliar al recurso con GUID: {idCortoPelicula}.");
        }

        #endregion Modificar pelicula
    }
    /// <summary>
    /// Devuelve la uri de un recurso pelicula si existe en la comunidad sino la cadena estará vacia
    /// </summary>
    /// <param name="nombre"></param>
    /// <param name="mResourceApi"></param>
    /// <returns></returns>
    private static string getUriPelicula(string nombre, ResourceApi mResourceApi)
    {
        string uri = "";

        //Obtención del id de la persona cargada en la comunidad
        string pOntology = "peliculaleo";
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
    private static string getUriGenero(string nombre, ResourceApi mResourceApi)
    {
        string uri = "";

        //Obtención del id de la persona cargada en la comunidad
        string pOntology = "generoleo";
        string select = string.Empty, where = string.Empty;
        select += $@"SELECT DISTINCT ?s";
        where += $@" WHERE {{ ";
        where += $@"?s ?p '{nombre}'.";
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