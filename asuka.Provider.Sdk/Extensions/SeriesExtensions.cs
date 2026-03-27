using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace asuka.Provider.Sdk.Extensions;

public static class SeriesExtensions
{
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS8618
    [XmlRoot("ComicInfo", Namespace = "")]
    public class TachiyomiDetails
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Series")]
        public string Series { get; set; }

        [XmlElement("Writer")]
        public string Author { get; set; }

        [XmlElement("Penciller")]
        public string Artist { get; set; }

        [XmlElement("Summary")]
        public string Description { get; set; }

        [XmlElement("Genre")]
        public string Genres { get; set; }
    
        [XmlElement(ElementName = "PublishingStatusTachiyomi", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public string Status { get; set; }

        [XmlElement(ElementName = "Categories", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public string Categories { get; set; }

        [XmlElement(ElementName = "SourceMihon", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public string Source { get; set; }
    }
#pragma warning restore CS8618
// ReSharper restore UnusedAutoPropertyAccessor.Local

    /// <summary>
    /// Save metadata
    /// </summary>
    /// <param name="series"></param>
    /// <param name="writePath"></param>
    public static async Task WriteMetadata(this Series series, string writePath)
    {
        var metadata = new TachiyomiDetails
        {
            Title = series.Title,
            Artist = string.Join(", ", series.Artists),
            Author = string.Join(", ", series.Authors),
            Genres = string.Join(", ", series.Genres), // List of string
            Status = series.Status == SeriesStatus.Completed ? "Completed" : "Ongoing",
            Source = "Local"
        };
        
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");
        namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        namespaces.Add("ty", "http://www.w3.org/2001/XMLSchema"); 
        namespaces.Add("mh", "http://www.w3.org/2001/XMLSchema");
        
        // Serialize
        var serializer = new XmlSerializer(typeof(TachiyomiDetails));
        await using var stringWriter = new StringWriter();
        
        // OmitXmlDeclaration = false will keep the <?xml version="1.0" encoding="utf-8"?> header
        var settings = new XmlWriterSettings { 
            Indent = true, 
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
            Async = true
        };
        
        await using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        
        serializer.Serialize(xmlWriter, metadata, namespaces);
        var serialized = stringWriter.ToString();
        
        // Dirty patch for now to force the output to use UTF-8 instead of UTF-16
        serialized = serialized.Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");

        await File.WriteAllTextAsync(writePath, serialized, Encoding.UTF8);
    }
}
