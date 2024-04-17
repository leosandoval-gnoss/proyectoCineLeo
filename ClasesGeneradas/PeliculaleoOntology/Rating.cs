using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Gnoss.ApiWrapper.Helpers;
using GnossBase;
using Es.Riam.Gnoss.Web.MVC.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using Gnoss.ApiWrapper.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace PeliculaleoOntology
{
	[ExcludeFromCodeCoverage]
	public class Rating : GnossOCBase
	{
		public Rating() : base() { } 

		public Rating(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			mGNOSSID = pSemCmsModel.Entity.Uri;
			mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Schema_ratingValue = new Dictionary<LanguageEnum,string>();
			this.Schema_ratingValue.Add(idiomaUsuario , GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://schema.org/ratingValue")));
			
			this.Schema_ratingSource = new Dictionary<LanguageEnum,string>();
			this.Schema_ratingSource.Add(idiomaUsuario , GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://schema.org/ratingSource")));
			
		}

		public virtual string RdfType { get { return "http://schema.org/Rating"; } }
		public virtual string RdfsLabel { get { return "http://schema.org/Rating"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://schema.org/ratingValue")]
		[RDFProperty("http://schema.org/ratingValue")]
		public  Dictionary<LanguageEnum,string> Schema_ratingValue { get; set;}

		[LABEL(LanguageEnum.es,"http://schema.org/ratingSource")]
		[RDFProperty("http://schema.org/ratingSource")]
		public  Dictionary<LanguageEnum,string> Schema_ratingSource { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			if(this.Schema_ratingValue != null)
			{
				foreach (LanguageEnum idioma in this.Schema_ratingValue.Keys)
				{
					propList.Add(new StringOntologyProperty("schema:ratingValue", this.Schema_ratingValue[idioma], idioma.ToString()));
				}
			}
			else
			{
				throw new GnossAPIException($"La propiedad schema:ratingValue debe tener al menos un valor en el recurso: {resourceID}");
			}
			if(this.Schema_ratingSource != null)
			{
				foreach (LanguageEnum idioma in this.Schema_ratingSource.Keys)
				{
					propList.Add(new StringOntologyProperty("schema:ratingSource", this.Schema_ratingSource[idioma], idioma.ToString()));
				}
			}
			else
			{
				throw new GnossAPIException($"La propiedad schema:ratingSource debe tener al menos un valor en el recurso: {resourceID}");
			}
		}

		internal override void GetEntities()
		{
			base.GetEntities();
		} 











	}
}
