namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var result = new StringBuilder();

            var genresWithGames = context.Genres
                .Include(x => x.Games)
                .ThenInclude(x => x.Purchases)
                .Include(x => x.Games)
                .ThenInclude(x => x.Developer)
                .Include(x => x.Games)
                .ThenInclude(x => x.GameTags)
                .ThenInclude(x => x.Tag)
                .Where(x => genreNames.Contains(x.Name))
                .ToList()
                .Select(x => new ExportGenreDto()
                {
                    Id = x.Id,
                    Genre = x.Name,
                    Games = x.Games
                    .Where(g => g.Purchases.Count > 0)
                    .Select(g => new ExportGameDto()
                    {
                        Id = g.Id,
                        Title = g.Name,
                        Developer = g.Developer.Name,
                        Tags = string.Join(", ", g.GameTags.Select(t => t.Tag.Name)),
                        Players = g.Purchases.Count()
                    })
                    .OrderByDescending(g => g.Players)
                    .ThenBy(g => g.Id)
                    .ToList(),
                    TotalPlayers = x.Games.Sum(g => g.Purchases.Count)

                })
                .OrderByDescending(x => x.TotalPlayers)
                .ThenBy(x => x.Id)
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            result.AppendLine(JsonConvert.SerializeObject(genresWithGames, settings));

            return result.ToString().Trim();
        }

        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            var result = new StringBuilder();

            PurchaseType purchaseTypeEnum = Enum.Parse<PurchaseType>(storeType);

            var usersDto = context.Users
                .ToList()
                .Where(u => u.Cards.Any(c => c.Purchases.Any()))
                .Select(u => new ExportUserDto()
                {
                    Username = u.Username,
                    Purchases = context.Purchases
                        .ToArray()
                        .Where(p => p.Card.User.Username == u.Username && p.Type == purchaseTypeEnum)
                        .OrderBy(p => p.Date)
                        .Select(p => new ExportPurchaseDto()
                        {
                            Card = p.Card.Number,
                            Cvc = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new ExportPurchaseGameDto()
                            {
                                Name = p.Game.Name,
                                Genre = p.Game.Genre.Name,
                                Price = p.Game.Price
                            }
                        })
                        .ToList(),
                    TotalSpent = context
                        .Purchases
                        .ToList()
                        .Where(p => p.Card.User.Username == u.Username && p.Type == purchaseTypeEnum)
                        .Sum(p => p.Game.Price)

                })
                .Where(u => u.Purchases.Count > 0)
                .OrderByDescending(u => u.TotalSpent)
                .ThenBy(u => u.Username)
                .ToList();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var xmlSerializer = new XmlSerializer(typeof(List<ExportUserDto>), new XmlRootAttribute("Users"));

            using (StringWriter writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, usersDto, namespaces);
            }

            return result.ToString().Trim();
        }
    }
}

