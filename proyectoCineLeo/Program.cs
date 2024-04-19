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
using System.Text.Json;
using Newtonsoft.Json.Schema;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using System.Globalization;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

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

//        List<string> generosBorrar = new List<string>()
//        {
//            "http://gnoss.com/items/Genre_e4e16408-bedd-4ec9-bdbe-3c2520376896",
//"http://gnoss.com/items/Genre_9d74a880-46eb-4ef5-889f-cb555e50e947",
//"http://gnoss.com/items/Genre_eeee6a71-c048-4e84-9f1e-2c6bc77e63ad",
//    "http://gnoss.com/items/Genre_c8a5e951-90e9-4c18-a64b-cf58ad2422f1",
//    "http://gnoss.com/items/Genre_af41ed95-6d2a-404c-ba6e-72b0ccb9f919",
//    "http://gnoss.com/items/Genre_8aebad59-7887-4fa5-9432-a9bd25d25768",
//    "http://gnoss.com/items/Genre_932da312-dcb0-4118-b0cd-8243c8a6b5f1",
//    "http://gnoss.com/items/Genre_64a948c6-f81a-4015-8316-74977c2899e8",
//    "http://gnoss.com/items/Genre_4237cecb-8aff-44a7-b5b3-f54bf35ff2e3",
//    "http://gnoss.com/items/Genre_2a91bc14-6dde-4a33-9b81-89e310fee9c5",
//    "http://gnoss.com/items/Genre_c167ec78-8b3b-4a19-9ef5-89664881ae68",
//    "http://gnoss.com/items/Genre_39438635-3685-4ed8-9870-9b74e8b6765f",
//    "http://gnoss.com/items/Genre_14cb52cc-a81e-4c7d-95bd-853d8705d443",
//    "http://gnoss.com/items/Genre_2f6f907b-ba48-4569-af82-0b67be7ba7b0",
//"http://gnoss.com/items/Genre_c186ad81-1b76-4381-8d5c-f5716bba888f"
//        };
//        mResourceApi.ChangeOntology("generoleo.owl");
//        mResourceApi.DeleteSecondaryEntitiesList(ref generosBorrar);
//        Console.WriteLine();
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

        string uri = getUriPersona("Persona prueba", mResourceApi);

        string[] partes = uri.Split('/', '_');

        string resourceId = partes[5];
        string articleID = partes[6];

        Person persona1Modificado = new Person();
        persona1Modificado.Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() {
            { GnossOCBase.LanguageEnum.es,"Persona prueba modificado" }
        };

        mResourceApi.ModifyComplexOntologyResource(persona1Modificado.ToGnossApiResource(mResourceApi, null, new Guid(resourceId), new Guid(articleID)), false, true);

        // Eliminar la persona de prueba

        uri = getUriPersona("Persona prueba modificado", mResourceApi);

        try
        {
            mResourceApi.ChangeOntology("personaleo.owl");
            mResourceApi.PersistentDelete(mResourceApi.GetShortGuid(uri), true, true);
        }
        catch (Exception ex)
        {
            mResourceApi.Log.Error(ex.ToString());
        }

        // Carga persona prueba 2



        mResourceApi.ChangeOntology("personaleo.owl");
        Person persona2 = new Person();
        persona2.Schema_name = new Dictionary<GnossBase.GnossOCBase.LanguageEnum, string>()
        {
            {GnossOCBase.LanguageEnum.es, "Persona prueba 2"}
        };

        resorceLoad = persona2.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        mResourceApi.LoadComplexSemanticResource(resorceLoad);

        //  Carga Película de Prueba

        string uriPersonaPrueba = getUriPersona("Persona prueba 2", mResourceApi);
        string uriGeneroPrueba = getUriGenero("Genero de prueba", mResourceApi);

        Movie pelicula = new Movie();
        pelicula.Schema_image = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "https://walpaper.es/wallpaper/2015/11/wallpaper-gratis-de-un-espectacular-paisaje-en-color-azul-en-hd.jpg" } };
        pelicula.Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "Pelicula de prueba" } };
        pelicula.Schema_description = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "Descripcion de pelicula de prueba" } };
        pelicula.Schema_duration = new List<int>() { { 60 } };
        pelicula.Schema_contentRating = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, "+12" } };
        pelicula.IdsSchema_actor = new List<string>() { uriPersonaPrueba };
        pelicula.IdsSchema_genre = new List<string>() { uriGeneroPrueba };
        mResourceApi.ChangeOntology("peliculaleo.owl");
        ComplexOntologyResource resorceToLoad = pelicula.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        string idPeliculaCargada = mResourceApi.LoadComplexSemanticResource(resorceToLoad);

        #endregion Basico

        #region Intermedio

        cargaMasiva(mResourceApi);

        #endregion Intermedio

        #endregion Segunda Parte
    }


    private static void cargaMasiva(ResourceApi mResourceApi)
    {


        foreach (string rutaJson in Directory.GetFiles("Data", "*.json"))
        {
            // Leemos el JSON y lo guardamos en un objeto
            JObject datos = JObject.Parse(File.ReadAllText(rutaJson));

            // Si la uri está vacia creamos la pelicula
            string uriPelicula = getUriPelicula(datos.Value<string>("Title"), mResourceApi);

            if (uriPelicula.Length != 0) continue;

            // Cargamos los datos de la clase Movie

            Movie pelicula = new()
            {
                Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, datos.Value<string>("Title") } },
                Schema_recordedAt = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>(datos.Value<string>("Year").Split(",")) } },
                Schema_contentRating = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, datos.Value<string>("Rated") } },
                Schema_datePublished = DateTime.Parse(datos.Value<string>("Released")),
                Schema_duration = new List<int>() { int.Parse(datos.Value<string>("Runtime").Split(" ")[0]) },
                Schema_description = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, datos.Value<string>("Plot") } },
                // Hay varios idiomas, vamos a añadirlos por separado
                Schema_inLanguage = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>(datos.Value<string>("Language").Split(",")) } },
                Schema_countryOfOrigin = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>(datos.Value<string>("Country").Split(",")) } },
                Schema_award = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>() { datos.Value<string>("Awards") } } },
                Schema_image = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, datos.Value<string>("Poster") } },
                Schema_aggregateRating = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>() { datos.Value<string>("imdbRating") } } },
                Schema_productionCompany = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>() { datos.Value<string>("Production") } } },
                Schema_url = new Dictionary<GnossOCBase.LanguageEnum, List<string>>() { { GnossOCBase.LanguageEnum.es, new List<string>() { datos.Value<string>("Website") } } }
            };

            // Comprobamos si el genero existen, cargaremos los generos con sus uri
            List<string> generos = new List<string>();

            foreach (string nomGenero in datos.Value<string>("Genre").Split(","))
            {
                string nomGeneroFormateado = nomGenero.Trim();
                string uri = getUriGenero(nomGeneroFormateado, mResourceApi);
                if (uri.Length != 0) { generos.Add(uri); continue; };
                uri = cargaGenero(nomGenero, mResourceApi);
                generos.Add(uri);
            }

            // Asociamos los generos con la pelicual
            pelicula.IdsSchema_genre = generos;

            // Comprobamos si las personas que nos pasan existen
            List<string> personas = new List<string>();

            // Comprobamos los directores
            personas = comprobarPersona(datos.Value<string>("Director"), mResourceApi);
            pelicula.IdsSchema_director = personas;
            // Comprobamos los autores
            personas = comprobarPersona(datos.Value<string>("Writer"), mResourceApi);
            pelicula.IdsSchema_author = personas;
            // Comprobamos los actores
            personas = comprobarPersona(datos.Value<string>("Actors"), mResourceApi);
            pelicula.IdsSchema_actor = personas;
            // Añadir los ratings
            pelicula.Schema_rating = asignarRatings(datos.Value<JArray>("Ratings"));

            // Carga de pelicula
            mResourceApi.ChangeOntology("peliculaleo.owl");
            ComplexOntologyResource resorceToLoad = pelicula.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
            mResourceApi.LoadComplexSemanticResource(resorceToLoad);


        };
        Console.WriteLine("CARGA MASIVA TERMINADA");

    }
    private static List<Rating> asignarRatings(JArray ratings)
    {
        List<Rating> resultado = new List<Rating>();
        foreach (var datos in ratings)
        {
            Rating rating = new();
            rating.Schema_ratingSource = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, datos.Value<string>("Source") } };
            rating.Schema_ratingValue = getRatingValue(datos.Value<string>("Value"));
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
    private static List<string> comprobarPersona(string nombres, ResourceApi mResourceApi)
    {
        List<string> personas = new List<string>();
        foreach (string nomPersona in nombres.Split(","))
        {
            string nomPersonaLimpio = nomPersona.Replace("'", "");
            string uri = getUriPersona(nomPersonaLimpio, mResourceApi);
            if (uri.Length != 0) { personas.Add(uri); continue; };
            uri = cargaPersona(nomPersonaLimpio, mResourceApi);
            personas.Add(uri);
        }

        return personas;
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
    private static string cargaPersona(string nombre, ResourceApi mResourceApi)
    {
        mResourceApi.ChangeOntology("personaleo.owl");
        Person persona = new();
        persona.Schema_name = new Dictionary<GnossOCBase.LanguageEnum, string>() { { GnossOCBase.LanguageEnum.es, nombre } };
        ComplexOntologyResource resorceLoad = persona.ToGnossApiResource(mResourceApi, null, Guid.NewGuid(), Guid.NewGuid());
        mResourceApi.LoadComplexSemanticResource(resorceLoad);
        return persona.GetURI(mResourceApi);
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
        return genero.GetURI(mResourceApi);
    }
}