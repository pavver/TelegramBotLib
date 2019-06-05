using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.Linq;
using TelegramBotLib;

namespace Example
{
    public class Commands : BotCommands
    {
        private readonly string[] _carMarks =
        {
            "Alfa Romeo", "Aston Martin", "Audi", "Bentley", "Benz",
            "BMW", "Bugatti", "Cadillac", "Chevrolet", "Chrysler",
            "Citroen", "Corvette", "DAF", "Dacia", "Daewoo", "Daihatsu",
            "Datsun", "De Lorean", "Dino", "Dodge", "Farboud",
            "Ferrari", "Fiat", "Ford", "Honda", "Hummer", "Hyundai",
            "Jaguar", "Jeep", "KIA", "Koenigsegg", "Lada", "Lamborghini",
            "Lancia", "Land Rover", "Lexus", "Ligier", "Lincoln", "Lotus",
            "Martini", "Maserati", "Maybach", "Mazda", "McLaren",
            "Mercedes", "Mercedes-Benz", "Mini", "Mitsubishi",
            "Nissan", "Noble", "Opel", "Peugeot", "Pontiac", "Porsche",
            "Renault", "Rolls-Royce", "Rover", "Saab", "Seat",
            "Skoda", "Smart", "Spyker", "Subaru", "Suzuki", "Toyota",
            "Vauxhall", "Volkswagen", "Volvo"
        };

        public Commands(Chat chat) : base(chat)
        {
        }

        public override async Task OnUnknownCommand(string command)
        {
           await Start();
        }

        public override async Task Start()
        {
            await SendMessage("Типа главное меню",
                new[]
                {
                    new[] {new ButtonClass("Марки машин", nameof(CarList), "0")}
                });
        }

        public async Task CarList(string page)
        {
            int p = int.Parse(page);

            int countInPage = 9;

            var marks = _carMarks
                .Skip(p * countInPage)
                .Take(countInPage)
                .Select(key => new List<ButtonClass>() {new ButtonClass(key, nameof(SelectCar), $"{page}|{key}")})
                .ToList();

            var pagination = new List<ButtonClass>();

            if (p > 0)
                pagination.Add(new ButtonClass(page + " ⏪", nameof(CarList), (p - 1).ToString()));

            if (_carMarks.Length > p * (countInPage + 1))
                pagination.Add(new ButtonClass((p + 1) + " ⏩", nameof(CarList), (p + 1).ToString()));

            marks.Add(pagination);

            await SendMessage($"Марки машин, страница {p + 1}", marks);
        }

        public async Task SelectCar(string carName)
        {
            int i = carName.IndexOf('|');
            string page = carName.Substring(0, i);
            string name = carName.Substring(i + 1);

            await SendMessage($"Выбранная марка машины {name} и бла бла бла",
                new IEnumerable<ButtonClass>[]
                {
                    new[]
                    {
                        new ButtonClass("Назад", nameof(CarList), page)
                    }
                });
        }

    }
}