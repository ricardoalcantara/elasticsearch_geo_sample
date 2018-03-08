using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace elasticsearch_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }
        static async Task Run()
        {
            var client = new ElasticClient();

            var dtpPosition = new double[] { -22.95346821, -43.18583823 };
            var dtpCompany = new Company
            {
                Id = 1,
                Name = "Dtp",
                Cnpj = "1234567890",
                Point = dtpPosition,
            };
            var hortifrutiPosition = new double[] { -22.95248789, -43.18417835 };
            var hortifrutiCompany = new Company
            {
                Id = 2,
                Name = "Hortifruti",
                Cnpj = "0987654321",
                Point = hortifrutiPosition
            };

            await client.DeleteIndexAsync("ef");
            var createIndexResponse = await client.CreateIndexAsync("ef", c => c
                .Mappings(ms => ms
                    .Map<Company>(m => m
                        .Properties(ps => ps.GeoPoint(s => s.Name(n => n.Point)))
                        .AutoMap()
                    )
                )
            );

            var asyncIndexResponse = await client.IndexAsync(dtpCompany, s => s.Index("ef"));
            asyncIndexResponse = await client.IndexAsync<Company>(hortifrutiCompany, s => s.Index("ef"));

            var metroPosition = new double[] { -22.95153513, -43.1840917 };
            await Task.Delay(1000);
            var searchResponse = await client.SearchAsync<Company>(s => s
            .AllIndices()
            .AllTypes()
            .From(0)
            .Size(10)
            .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MatchAll()
                        )
                        .Filter(f => f
                            .GeoDistance(g => g
                                .Distance("150")
                                .Field(f2 => f2.Point)
                                .Location(metroPosition)
                            )
                        )
                    )
                )
            );

            foreach (var c in searchResponse.Documents)
            {
                Console.WriteLine(c.Name);
            }
        }

        public class Company
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Cnpj { get; set; }
            public double[] Point { get; set; }
        }
    }
}
