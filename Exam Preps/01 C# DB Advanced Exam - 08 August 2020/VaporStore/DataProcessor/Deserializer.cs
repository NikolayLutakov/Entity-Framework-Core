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
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var result = new StringBuilder();

			var listOfGamesToAdd = new List<Game>();

			var gamesDto = JsonConvert.DeserializeObject<List<ImportGameDto>>(jsonString).ToList();

			var developers = new List<Developer>();

			var genres = new List<Genre>();

			var tags = new List<Tag>();

            foreach (var game in gamesDto)
            {
				if (!IsValid(game))
				{
					result.AppendLine($"Invalid Data");
					continue;
				}

				DateTime releaseDate;
				bool isDateValid = DateTime.TryParseExact(game.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
					DateTimeStyles.None, out releaseDate);

                if (!isDateValid)
                {
					result.AppendLine($"Invalid Data");
					continue;
				}


                if (game.Tags.Count == 0)
                {
					result.AppendLine($"Invalid Data");
					continue;
				}

				var gameToAdd = new Game()
				{
					Name = game.Name,
					Price = game.Price,
					ReleaseDate = releaseDate
				};

				var developer = developers.Where(x => x.Name == game.Developer).FirstOrDefault();

                if (developer == null)
                {
					developer = new Developer()
					{
						Name = game.Developer
					};

					developers.Add(developer);
				}

				gameToAdd.Developer = developer;

				var genre = genres.Where(x => x.Name == game.Genre).FirstOrDefault();

                if (genre == null)
                {
					genre = new Genre()
					{
						Name = game.Genre
					};

					genres.Add(genre);
                }

				gameToAdd.Genre = genre;

                foreach (var tagToken in game.Tags)
                {
                    if (String.IsNullOrEmpty(tagToken))
                    {
						continue;
                    }

					var tag = tags.Where(x => x.Name == tagToken).FirstOrDefault();
                    
					if (tag == null)
                    {
						var tagToInsert = new Tag()
						{
							Name = tagToken
						};

						tags.Add(tagToInsert);

						gameToAdd.GameTags.Add(new GameTag()
						{
							Game = gameToAdd,
							Tag = tagToInsert
						});
                    }
                    else
                    {
						gameToAdd.GameTags.Add(new GameTag()
						{
							Game = gameToAdd,
							Tag = tag
						});
                    }
                }

				if (!gameToAdd.GameTags.Any())
				{
					result.AppendLine($"Invalid Data");
					continue;
				}

				listOfGamesToAdd.Add(gameToAdd);
				result.AppendLine($"Added {gameToAdd.Name} ({gameToAdd.Genre.Name}) with {gameToAdd.GameTags.Count} tags");
			}
			;
			context.Games.AddRange(listOfGamesToAdd);
			context.SaveChanges();


			return result.ToString().Trim();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var result = new StringBuilder();

			var usersDto = JsonConvert.DeserializeObject<List<ImportUserDto>>(jsonString).ToList();

			var usersToImport = new List<User>();
			var cards = new List<Card>();

            foreach (var user in usersDto)
            {
                if (!IsValid(user))
                {
					result.AppendLine($"Invalid Data");
					continue;
                }

                if (user.Cards.Count == 0)
                {
					result.AppendLine($"Invalid Data");
					continue;
				}

				var userToAdd = new User()
				{
					FullName = user.FullName,
					Username = user.Username,
					Email = user.Email,
					Age = user.Age
					
				};

                foreach (var card in user.Cards)
                {
					var curCard = cards.Where(x => x.Number == card.Number).FirstOrDefault();

                    if (curCard == null)
                    {
						curCard = new Card()
						{
							Number = card.Number,
							Cvc = card.CVC,
							Type = Enum.Parse<CardType>(card.Type)
							
						};
						cards.Add(curCard);
                    }

					userToAdd.Cards.Add(curCard);
                }

                if (!userToAdd.Cards.Any())
                {
					result.AppendLine($"Invalid Data");
					continue;
				}

				usersToImport.Add(userToAdd);
				result.AppendLine($"Imported {userToAdd.Username} with {userToAdd.Cards.Count} cards");
			}

			

			context.Users.AddRange(usersToImport);
			context.SaveChanges();



			return result.ToString().Trim();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var result = new StringBuilder();

			var xmlSerializer = new XmlSerializer(typeof(List<ImportPurchaseDto>), new XmlRootAttribute("Purchases"));

			var purchasesToAdd = new List<Purchase>();

			using (StringReader reader = new StringReader(xmlString))
            {

				var purchasesDto = xmlSerializer.Deserialize(reader) as List<ImportPurchaseDto>;

				

                foreach (var purchase in purchasesDto)
                {
					if (!IsValid(purchase))
					{
						result.AppendLine($"Invalid Data");
						continue;
					}

					DateTime date;
					bool isDateValid = DateTime.TryParseExact(purchase.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture,
						DateTimeStyles.None, out date);

                    if (!isDateValid)
					{
						result.AppendLine($"Invalid Data");
						continue;

					}

					var game = context.Games.FirstOrDefault(x => x.Name == purchase.Game);

                    if (game == null)
                    {
						result.AppendLine($"Invalid Data");
						continue;
					}

					var card = context.Cards.FirstOrDefault(x => x.Number == purchase.Card);

                    if (card == null)
                    {
						result.AppendLine($"Invalid Data");
						continue;
					}

					var curPurchase = new Purchase()
					{
						Type = Enum.Parse<PurchaseType>(purchase.Type),
						ProductKey = purchase.ProductKey,
						Date = date,
						Card = card,
						Game = game
						
						
					};

					purchasesToAdd.Add(curPurchase);
					result.AppendLine($"Imported {curPurchase.Game.Name} for {curPurchase.Card.User.Username}");
				}


				
			}
			context.AddRange(purchasesToAdd);
			context.SaveChanges();
			//Console.WriteLine(purchasesToAdd.Count);

			return result.ToString().Trim();
		}


		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}


		//   	  private static bool IsGameValid(ImportGameDto game)
		//      {
		//          if (game.Name == null
		//                  || game.ReleaseDate == null
		//                  || game.Developer == null
		//                  || game.Genre == null
		//                  || game.Tags.Count == 0 || game.Price < 0)
		//          {
		//              return false;
		//          }

		//          return true;
		//      }


	}
}