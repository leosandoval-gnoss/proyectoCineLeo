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
        #region Conexión y datos de la comunidad
        //string pathOAuth = @"Config\oAuth.config";
        //ResourceApi mResourceApi = new ResourceApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        //CommunityApi mCommunityApi = new CommunityApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        //ThesaurusApi mThesaurusApi = new ThesaurusApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));
        //UserApi mUserApi = new UserApi(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pathOAuth));

        //Console.WriteLine($"Id de la Comunidad -> {mCommunityApi.GetCommunityId()}");
        //Console.WriteLine($"Nombre de la Comunidad -> {mCommunityApi.GetCommunityInfo().name}");
        //Console.WriteLine($"Nombre Corto de la Comunidad -> {mCommunityApi.GetCommunityInfo().short_name}");
        //Console.WriteLine($"Descripción de la comunidad inicial -> {mCommunityApi.GetCommunityInfo().description}");
        //Console.WriteLine($"Categorías de la Comunidad -> {string.Join(", ", mCommunityApi.CommunityCategories.Select(categoria => categoria.category_name))}");

        #endregion Conexión con la comunidad
        #region Leer CSV
        StreamReader sr = new StreamReader("Data/query.csv");
        string linea = "";
        int contador = 0;
        sr.ReadLine();
        linea = sr.ReadLine();
        string[] datos = linea.Split(',');
        foreach (string datp  in datos)
        {
            Console.WriteLine(datp);
        }

        #endregion Leer CSV
    }
}