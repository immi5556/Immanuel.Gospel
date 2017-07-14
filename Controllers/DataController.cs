using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Immanuel.Gospel.Controllers
{
    public class DataController : ApiController
    {
        [HttpGet]
        public string GetPath()
        {
            string rootdir = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_data"), "Vids");
            return rootdir;
        }

        [HttpGet]
        public HttpResponseMessage GetJson()
        {
            return new HttpResponseMessage()
            {
                Content = new JsonContent(new
                {
                    Name = "Vivek",
                    Address = "Address",
                    Message = "Any Message" //return exception
                })
            };
        }

        [HttpGet]
        public Dashboard GetTopics()
        {
            var ds = new Dashboard();
            try
            {
                string rootdir = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_data"), "Vids");
                foreach (var tt in Directory.EnumerateDirectories(rootdir))
                {
                    var keyname = Path.GetFileName(tt);
                    ds.Videos.Add(Path.GetFileName(tt), new List<Publication>());
                    foreach (var tt1 in Directory.EnumerateFiles(tt))
                    {
                        ds.Videos[keyname].Add(new Publication() { Name = Path.GetFileName(tt1), Author = keyname, Duraion = GetDuration(tt1) });
                    }
                }
            }
            catch (Exception exp)
            {
                ds.Videos.Add("WWWWW", new List<Publication>());
                ds.Videos["WWWWW"].Add(new Publication() { Name = exp.ToString() });
            }
            return ds;
        }

        public string GetDuration(String file)
        {
            var inputFile = new MediaToolkit.Model.MediaFile { Filename = file };
            using (var engine = new MediaToolkit.Engine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_data/ffmpeg/ffmpeg.exe")))
            {
                engine.GetMetadata(inputFile);
            }

            TimeSpan ts = new TimeSpan(inputFile.Metadata.Duration.Ticks);
            return ts.ToString(@"hh\:mm\:ss");
        }

        public static double Convert100NanosecondsToMilliseconds(double nanoseconds)
        {
            // One million nanoseconds in 1 millisecond, but we are passing in 100ns units...
            return nanoseconds * 0.0001;
        }
    }

    public class JsonContent : HttpContent
    {

        private readonly MemoryStream _Stream = new MemoryStream();
        public JsonContent(object value)
        {

            Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var jw = new JsonTextWriter(new StreamWriter(_Stream));
            jw.Formatting = Formatting.Indented;
            var serializer = new JsonSerializer();
            serializer.Serialize(jw, value);
            jw.Flush();
            _Stream.Position = 0;

        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _Stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _Stream.Length;
            return true;
        }
    }

    public class Dashboard
    {
        public Dashboard()
        {
            Videos = new Dictionary<string, List<Publication>>();
            Audios = new Dictionary<string, List<Publication>>();
        }

        public Dictionary<string, List<Publication>> Videos { get; set; }
        public Dictionary<string, List<Publication>> Audios { get; set; }
    }

    public class Publication
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Duraion { get; set; }
    }
}
