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
using SixLabors.ImageSharp.ColorSpaces;
using System.Globalization;

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
        pelicula.Schema_description = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "Lore Ipsum" } };
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

        #region Añadir genero a pelicula

        Guid idCortoPelicula = mResourceApi.GetShortGuid(getUriPelicula("Prueba de pelicula", mResourceApi));

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

        #endregion Añadir genero a pelicula

        #region Carga datos CSV

        cargaDatosCSV("Data/query.csv", mResourceApi);

        #endregion Carga datos CSV
    }
    private static void cargaDatosCSV(string ruta, ResourceApi mResourceApi)
    {
        StreamReader reader = new StreamReader(ruta);

        string lineaActual = reader.ReadLine();
        while ((lineaActual = reader.ReadLine()) != null)
        {
            string[] datos = lineaActual.Split(",");
            string tituloPelicula = datos[1].Trim();
            string uriPelicula = getUriPelicula(tituloPelicula, mResourceApi);

            if (uriPelicula.Length != 0) continue;

            // Escogemos los datos que podemos almacenar
            string imagen = datos[2].Trim();
            int duracion = 0;
            int.TryParse(datos[3].Trim(), out duracion);
            string contentRating = datos[6];
            string[] paises = datos[8].Trim().Split(" || ");
            string productora = datos[10].Trim();
            string[] premios = datos[11].Trim().Split(" || ");

            // Cargamos los datos de la clase Movie

            Movie pelicula = new()
            {
                Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, tituloPelicula } },
                Schema_description = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "" } },
                Schema_image = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, imagen } },
                Schema_duration = new List<int>() { duracion },
                Schema_contentRating = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, contentRating } },
                Schema_countryOfOrigin = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>(paises) } },
                Schema_productionCompany = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>() { productora } } },
                Schema_award = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>(premios) } },
            };

            // Cargamos los generos
            List<string> generos = new List<string>();

            foreach (string nomGenero in datos[7].Trim().Split(" || "))
            {
                int indice = nomGenero.IndexOf("[");
                string nomGeneroFormateado = nomGenero.Trim().Substring(0,indice);
                string uri = getUriGenero(nomGeneroFormateado, mResourceApi);
                if (uri.Length != 0) { generos.Add(uri); continue; };
                uri = cargaGenero(nomGeneroFormateado, mResourceApi);
                generos.Add(uri);
            }

            pelicula.IdsSchema_genre = generos;

            // Cargamos los ratings
            pelicula.Schema_rating = asignarRatings(datos[9].Trim().Split(" || "));

            //// Comprobamos si las personas que nos pasan existen
            List<string> personas = new List<string>();
            // Comprobamos los actores
            personas = comprobarPersona(datos[12].Trim(), mResourceApi);
            pelicula.IdsSchema_actor = personas;

            // Carga de pelicula
            mResourceApi.ChangeOntology("peliculaleo.owl");
            ComplexOntologyResource resorceToLoad = pelicula.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
            mResourceApi.LoadComplexSemanticResource(resorceToLoad);
        }
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
    /// <summary>
    /// Devuelve la uri de un recurso genero si existe en la comunidad sino la cadena estará vacia
    /// </summary>
    /// <param name="nombre"></param>
    /// <param name="mResourceApi"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Carga en la comunidad un recurso de genero(secundario) y devuelve su uri
    /// </summary>
    /// <param name="nombre"></param>
    /// <param name="mResourceApi"></param>
    /// <returns></returns>
    private static string cargaGenero(string nombre, ResourceApi mResourceApi)
    {
        string identificador = Guid.NewGuid().ToString();
        Genre genero = new(identificador);
        genero.Schema_name = nombre;
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

        return getUriGenero(nombre, mResourceApi);
    }
    private static List<Rating> asignarRatings(string[] ratings)
    {
        List<Rating> resultado = new List<Rating>();
        foreach (string datos in ratings)
        {
            Rating rating = new();
            int indice = datos.Trim().IndexOf("[");
            string ratingSource = datos.Substring(indice+1).Replace("]","");
            string ratingValue = datos.Substring(0,indice);
            rating.Schema_ratingSource = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, ratingSource } };
            rating.Schema_ratingValue = getRatingValue(ratingValue);
            resultado.Add(rating);
        }
        return resultado;
    }
    private static int getRatingValue(string valorCadena)
    {
        int valor = 0;
        string formateado = valorCadena.Split('%', '/')[0];
        if (formateado.Contains('.'))
        {
            double valorDecimal = double.Parse(formateado, CultureInfo.InvariantCulture) * 10;
            valor = (int)valorDecimal;
        }
        else
        {
            valor = int.Parse(formateado);
        }
        return valor;
    }
    private static string cargaPersona(string nombre, string imagen, ResourceApi mResourceApi)
    {
        mResourceApi.ChangeOntology("personaleo.owl");
        Person persona = new();
        persona.Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, nombre } };
        persona.Schema_image = imagen;
        ComplexOntologyResource resorceLoad = persona.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        mResourceApi.LoadComplexSemanticResource(resorceLoad);
        return getUriPersona(nombre, mResourceApi);
    }
    /// <summary>
    /// Devuelve la uri de un recurso persona si existe en la comunidad sino la cadena estará vacia
    /// </summary>
    /// <param name="nombre"></param>
    /// <param name="mResourceApi"></param>
    /// <returns></returns>
    private static string getUriPersona(string nombre, ResourceApi mResourceApi)
    {
        string uri = "";

        //Obtención del id de la persona cargada en la comunidad
        string pOntology = "personaleo";
        string select = string.Empty, where = string.Empty;
        select += $@"SELECT DISTINCT ?s";
        where += $@" WHERE {{ ";
        where += $@"?s ?p '{nombre}'@es.";
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
    private static List<string> comprobarPersona(string datos, ResourceApi mResourceApi)
    {
        List<string> personas = new List<string>();
        foreach (string datosPersona in datos.Split(" || "))
        {
            int indice = datosPersona.Trim().IndexOf("[");
            string nombre = datosPersona.Substring(0, indice);
            string imagen = datosPersona.Substring(indice + 1).Replace("]","");
            string uri = getUriPersona(nombre, mResourceApi);
            if (uri.Length != 0) { personas.Add(uri); continue; };
            uri = cargaPersona(nombre,imagen, mResourceApi);
            personas.Add(uri);
        }

        return personas;
    }
}