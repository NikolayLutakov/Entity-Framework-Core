namespace MusicHub
{
    using System;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context =
                new MusicHubDbContext();

            //DbInitializer.ResetDatabase(context);

            //Test your solutions here

            //Console.WriteLine(ExportAlbumsInfo(context, InputReader()));

            //Console.WriteLine(ExportSongsAboveDuration(context, InputReader()));
        }

        private static int InputReader() => int.Parse(Console.ReadLine());

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            StringBuilder result = new StringBuilder();

            //var albums = context.Producers
            //    .FirstOrDefault(x => x.Id == producerId)
            //    .Albums
            //    .Select(album => new 
            //    { 
            //        Name = album.Name, 
            //        ReleaseDate = album.ReleaseDate, 
            //        ProducerName = album.Producer.Name, 
            //        AlbumSongs = album.Songs.Select(song => new 
            //        { 
            //            SongName = song.Name, 
            //            Price = song.Price, 
            //            SongWriter = song.Writer.Name 
            //        })
            //        .OrderByDescending(x => x.SongName)
            //        .ThenBy(x => x.SongWriter)
            //        .ToList(), 
            //        AlbumTotalPrice = album.Price 
            //    })
            //    .OrderByDescending(x => x.AlbumTotalPrice)
            //    .ToList();

            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    Name = a.Name,
                    ReleaseDate = a.ReleaseDate,
                    ProducerName = a.Producer.Name,
                    AlbumTotalPrice = a.Price,
                    AlbumSongs = a.Songs
                        .OrderByDescending(s => s.Name)
                        .ThenBy(s => s.Writer.Name)
                        .Select(s => new
                        {
                            SongName = s.Name,
                            Price = s.Price,
                            SongWriter = s.Writer.Name
                        }).ToList()


                }).ToList().OrderByDescending(a => a.AlbumTotalPrice);

            foreach (var album in albums)
            {
                result.AppendLine($"-AlbumName: {album.Name}");
                result.AppendLine($"-ReleaseDate: {album.ReleaseDate:MM/dd/yyyy}");
                result.AppendLine($"-ProducerName: {album.ProducerName}");
                result.AppendLine($"-Songs:");

                int counter = 0;
                foreach (var song in album.AlbumSongs)
                {
                    counter++;
                    result.AppendLine($"---#{counter}");
                    result.AppendLine($"---SongName: {song.SongName}");
                    result.AppendLine($"---Price: {song.Price:F2}");
                    result.AppendLine($"---Writer: {song.SongWriter}");
                    
                }

                result.AppendLine($"-AlbumPrice: {album.AlbumTotalPrice:F2}");
            }

            return result.ToString().Trim();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            StringBuilder result = new StringBuilder();

            var songs = context.Songs
                .Select(s => new
                {
                    SongName = s.Name,
                    SongPerformerFullName = s.SongPerformers
                        .Select(sp => sp.Performer.FirstName + " " + sp.Performer.LastName)
                        .FirstOrDefault(),
                    SongWriterName = s.Writer.Name,
                    SongAlbumProducer = s.Album.Producer.Name,
                    SongDuration = s.Duration,
                }).ToList()
                .Where(s => s.SongDuration.TotalSeconds > duration)
                .OrderBy(s => s.SongName)
                .ThenBy(s => s.SongWriterName)
                .ThenBy(s => s.SongPerformerFullName)
                .ToList();
            
            int cnt = 0;
            foreach (var song in songs)
            {
                cnt++;
                result.AppendLine($"-Song #{cnt}");
                result.AppendLine($"---SongName: {song.SongName}");
                result.AppendLine($"---Writer: {song.SongWriterName}");
                result.AppendLine($"---Performer: {song.SongPerformerFullName}");
                result.AppendLine($"---AlbumProducer: {song.SongAlbumProducer}");
                result.AppendLine($"---Duration: {song.SongDuration:c}");
            }
            return result.ToString().Trim();
        }
    }
}
