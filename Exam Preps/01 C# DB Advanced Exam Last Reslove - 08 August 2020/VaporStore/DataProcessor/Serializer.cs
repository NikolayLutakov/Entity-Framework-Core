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
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{

			var genres = context.Genres
				.Where(x => genreNames.Contains(x.Name))
				.ToList()
				.Select(x => new
				{
					Id = x.Id,
					Genre = x.Name,
					Games = x.Games
					.Where(g => g.Purchases.Count > 0)
					.Select(g => new
					{
						Id = g.Id,
						Title = g.Name,
						Developer = g.Developer.Name,
						Tags = string.Join(", ", g.GameTags.Select(t => t.Tag.Name)),
						Players = g.Purchases.Count()
					})
					.ToList()
					.OrderByDescending(g => g.Players)
					.ThenBy(g => g.Id)
					,
					TotalPlayers = x.Games.Sum(g => g.Purchases.Count)
				})
				.OrderByDescending(x => x.TotalPlayers)
				.ThenBy(x => x.Id);

			return JsonConvert.SerializeObject(genres, Formatting.Indented);
		}

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{

			var a = Enum.TryParse<PurchaseType>(storeType, out var parsed);

			var users = context.Users
				.ToArray()
				.Where(x => x.Cards.Any(c => c.Purchases.Any(p => p.Type == parsed)))
				.Select(x => new ExportUserDto
				{
					Username = x.Username,
					Purchases = context.Purchases.Where(p => p.Card.User.Username == x.Username && p.Type == parsed)
					.OrderBy(p => p.Date)
					.Select(p => new ExportPurchaseDto 
					{ 
						Card = p.Card.Number,
						Cvc = p.Card.Cvc,
						Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
						Game = new ExportGameDto
						{ 
							GameName = p.Game.Name,
							Genre = p.Game.Genre.Name,
							Price = p.Game.Price
						}
					})
					
					.ToArray(),
					TotalSpent = x.Cards.Sum(p => p.Purchases.Where(p => p.Card.User.Username == x.Username && p.Type == parsed).Sum(d => d.Game.Price))
				})
				.OrderByDescending(x => x.TotalSpent)
				.ThenBy(x => x.Username)
				.ToArray();

			var xmlSerializer = new XmlSerializer(typeof(ExportUserDto[]), new XmlRootAttribute("Users"));

			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add(string.Empty, string.Empty);

			var result = new StringBuilder();

            using (var writer = new StringWriter(result))
            {
				xmlSerializer.Serialize(writer, users, namespaces);
            }

			return result.ToString().Trim();
		}
	}
}