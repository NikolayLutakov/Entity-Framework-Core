namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var result = new StringBuilder();

			var games = JsonConvert.DeserializeObject<List<ImportGameDto>>(jsonString);

            foreach (var game in games)
            {
                if (!IsValid(game))
                {
					result.AppendLine("Invalid Data");
					continue;
                }

				var developer = context.Developers.FirstOrDefault(x => x.Name == game.Developer);

                if (developer == null)
                {
					developer = new Developer
					{
						Name = game.Developer
					};
                } 

				var genre = context.Genres.FirstOrDefault(x => x.Name == game.Genre);

				if (genre == null)
				{
					genre = new Genre
					{
						Name = game.Genre
					};
				}

				var gameToImport = new Game
				{
					Name = game.Name,
					Price = game.Price,
					ReleaseDate = game.ReleaseDate.Value,
					Developer = developer,
					Genre = genre,
					
				};

                foreach (var tag in game.Tags)
                {
					var curTag = context.Tags.FirstOrDefault(x => x.Name == tag);

                    if (curTag == null)
                    {
						curTag = new Tag
						{
							Name = tag
						};
                    }

					var gameTag = new GameTag
					{
						Tag = curTag
					};

					gameToImport.GameTags.Add(gameTag);
                }

                if (gameToImport.GameTags.Count == 0)
                {
					result.AppendLine("Invalid Data");
					continue;
				}

				context.Add(gameToImport);
				result.AppendLine($"Added {gameToImport.Name} ({gameToImport.Genre.Name}) with {gameToImport.GameTags.Count()} tags");
				context.SaveChanges();

			}


			return result.ToString().Trim();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var result = new StringBuilder();

			var users = JsonConvert.DeserializeObject<List<ImportUserDto>>(jsonString);

            foreach (var user in users)
            {
                if (!IsValid(user))
                {
					result.AppendLine("Invalid Data");
					continue;
                }

				var userToImport = new User
				{
					FullName = user.FullName,
					Username = user.Username,
					Email = user.Email,
					Age = user.Age
				};

				var flag = false;
                foreach (var card in user.Cards)
                {
					
                    if (!IsValid(card))
                    {
						flag = true;
						result.AppendLine("Invalid Data");
						break;
					}

					var cardToAdd = new Card
					{
						Number = card.Number,
						Cvc = card.CVC,
						Type = card.Type.Value
					};

					userToImport.Cards.Add(cardToAdd);
                }

                if (flag)
                {
					continue;
                }

				result.AppendLine($"Imported {userToImport.Username} with {userToImport.Cards.Count} cards");
				context.Add(userToImport);
				

            }
			context.SaveChanges();
			return result.ToString().Trim();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var result = new StringBuilder();

			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add(string.Empty, string.Empty);

			var xmlSerializer = new XmlSerializer(typeof(List<ImportPurchaseDto>), new XmlRootAttribute("Purchases"));

            using (var reader = new StringReader(xmlString))
            {
				var purchases = xmlSerializer.Deserialize(reader) as List<ImportPurchaseDto>;

                foreach (var purchase in purchases)
                {
                    if (!IsValid(purchase))
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					DateTime date;
					var parseDate = DateTime
						.TryParseExact(purchase.Date, "dd/MM/yyyy HH:mm",
									CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                    if (!parseDate)
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					var card = context.Cards.FirstOrDefault(x => x.Number == purchase.CardNumber);
					var game = context.Games.FirstOrDefault(x => x.Name == purchase.GameName);
                    
					if (card == null || game == null)
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					var purchaseToImport = new Purchase
					{
						Game = game,
						Type = purchase.Type.Value,
						ProductKey = purchase.Key,
						Card = card,
						Date = date
					};

					var username = context.Users
						.FirstOrDefault(x => x.Cards.Select(c => c.Number).Contains(purchase.CardNumber))
						.Username;


					result.AppendLine($"Imported {purchaseToImport.Game.Name} for {username}");
					context.Add(purchaseToImport);
                }
            }

			context.SaveChanges();

			return result.ToString().Trim();
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}