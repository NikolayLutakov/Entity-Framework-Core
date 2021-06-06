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
    using Newtonsoft.Json.Converters;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;
	

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var result = new StringBuilder();

			var format = "yyyy-MM-dd"; // your datetime format
			
			var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format, Culture = CultureInfo.InvariantCulture };

			var gamesDto = JsonConvert.DeserializeObject<List<ImportGameDto>>(jsonString, dateTimeConverter).ToList();

			var gamesToAdd = new List<Game>();
			var genres = new List<Genre>();
			var developers = new List<Developer>();
			var tags = new List<Tag>();
            foreach (var gameDto in gamesDto)
            {
                if (!IsValid(gameDto))
                {
					result.AppendLine("Invalid Data");
					continue;
                }

                if (gameDto.Tags.Count == 0)
                {
					result.AppendLine("Invalid Data");
					continue;
				}

				var developer = developers.FirstOrDefault(x => x.Name == gameDto.Developer);
                if (developer == null)
                {
					developer = new Developer()
					{
						Name = gameDto.Developer		
					};
					developers.Add(developer);
                }

				var genre = genres.FirstOrDefault(x => x.Name == gameDto.Genre);
                if (genre == null)
                {
					genre = new Genre()
					{
						Name = gameDto.Genre
					};
					genres.Add(genre);
                }

				var gameToAdd = new Game()
				{
					Name = gameDto.Name,
					Price = gameDto.Price,
					ReleaseDate = gameDto.ReleaseDate,
					Developer = developer,
					Genre = genre
				};


				var gameTags = new List<GameTag>();
				foreach (var tag in gameDto.Tags)
                {
					var tagToAdd = tags.FirstOrDefault(x => x.Name == tag);
					if (tagToAdd == null)
					{
						tagToAdd = new Tag()
						{
							Name = tag
						};
						tags.Add(tagToAdd);
					}

					var gameTag = new GameTag()
					{
						Game = gameToAdd,
						Tag = tagToAdd
					};

					gameTags.Add(gameTag);
				}

				gameToAdd.GameTags = gameTags;

				gamesToAdd.Add(gameToAdd);
				result.AppendLine($"Added {gameToAdd.Name} ({gameToAdd.Genre.Name}) with {gameToAdd.GameTags.Count} tags");

            }

			context.AddRange(gamesToAdd);
			context.SaveChanges();

			return result.ToString().Trim();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var result = new StringBuilder();

			var usersDto = JsonConvert.DeserializeObject<List<ImportUserDto>>(jsonString).ToList();

			var usersToAdd = new List<User>();
            foreach (var user in usersDto)
            {
                if (!IsValid(user))
                {
					result.AppendLine("Invalid Data");
					continue;
				}

				var userToAdd = new User()
				{
					FullName = user.FullName,
					Username = user.Username,
					Email = user.Email,
					Age = user.Age
				};

				var cardsToAdd = new List<Card>();
                foreach (var card in user.Cards)
                {
                    if (!IsValid(card))
                    {
						
						break;
					}

					CardType cardType;
					var parseCard = Enum.TryParse<CardType>(card.Type, out cardType);

                    if (!parseCard)
                    {
						
						break;
					}

					var cardToAdd = new Card()
					{
						Number = card.Number,
						Cvc = card.Cvc,
						Type = cardType
					
					};

					cardsToAdd.Add(cardToAdd);
                }

				if (cardsToAdd.Count == 0)
				{
					result.AppendLine("Invalid Data");
					continue;
				}

				userToAdd.Cards = cardsToAdd;
				usersToAdd.Add(userToAdd);
				result.AppendLine($"Imported {userToAdd.Username} with {cardsToAdd.Count} cards");
			}

			context.AddRange(usersToAdd);
			context.SaveChanges();
			return result.ToString().Trim();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var result = new StringBuilder();

			var xmlSerializer = new XmlSerializer(typeof(List<ImportPurchaseDto>), new XmlRootAttribute("Purchases"));

			var purchasesToAdd = new List<Purchase>();

            using (var reader = new StringReader(xmlString))
            {
				var purchasesDto = xmlSerializer.Deserialize(reader) as List<ImportPurchaseDto>;

                foreach (var purchaseDto in purchasesDto)
                {
                    if (!IsValid(purchasesDto))
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					PurchaseType purchaseType;
					var parsedType = Enum.TryParse<PurchaseType>(purchaseDto.Type, out purchaseType);

                    if (!parsedType)
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					DateTime date;
					var parseDate = DateTime
						.TryParseExact(purchaseDto.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                    if (!parseDate)
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					var card = context.Cards.FirstOrDefault(x => x.Number == purchaseDto.Card);
                    if (card == null)
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					var game = context.Games.FirstOrDefault(x => x.Name == purchaseDto.Game);
                    if (game == null)
                    {
						result.AppendLine("Invalid Data");
						continue;
					}

					var purchaseToAdd = new Purchase()
					{
						Game = game,
						Type = purchaseType,
						ProductKey = purchaseDto.ProductKey,
						Card = card,
						Date = date
					};
					purchasesToAdd.Add(purchaseToAdd);
					result.AppendLine($"Imported {purchaseToAdd.Game.Name} for {purchaseToAdd.Card.User.Username}");
                }
            }
			context.AddRange(purchasesToAdd);
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