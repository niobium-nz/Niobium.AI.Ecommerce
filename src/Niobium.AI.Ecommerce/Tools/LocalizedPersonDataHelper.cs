using System.Collections.ObjectModel;

namespace Niobium.AI.Ecommerce.Tools
{
    public sealed record FirstNameCityPair(string FirstName, string City);

    /// <summary>
    /// Generates weighted first-name and city pairs for a supported country using hardcoded datasets
    /// derived from recent country population distributions and commonly reported baby-name rankings.
    /// City weights approximate each location's share of national population with coverage targeted near
    /// the largest urban areas, while name weights approximate country-level popularity and are adjusted
    /// per city to reflect higher immigrant diversity in larger global metros.
    /// </summary>
    public static class LocalizedPersonDataHelper
    {
        private static readonly StringComparer CountryComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

        private static readonly IReadOnlyDictionary<string, CountryProfile> CountryProfiles =
            new ReadOnlyDictionary<string, CountryProfile>(new Dictionary<string, CountryProfile>(CountryComparer)
            {
                ["US"] = CreateProfile(
                    "US",
                    new[]
                    {
                    City("New York", 8.5), City("Los Angeles", 4.0), City("Chicago", 2.7), City("Houston", 2.3), City("Phoenix", 1.6),
                    City("Philadelphia", 1.6), City("San Antonio", 1.5), City("San Diego", 1.4), City("Dallas", 1.3), City("San Jose", 1.0),
                    City("Austin", 1.0), City("Jacksonville", 1.0), City("Fort Worth", 0.95), City("Columbus", 0.9), City("Charlotte", 0.9),
                    City("San Francisco", 0.8), City("Seattle", 0.75), City("Denver", 0.72), City("Boston", 0.67), City("Nashville", 0.69),
                    City("Detroit", 0.63), City("Atlanta", 0.51), City("Miami", 0.46), City("Portland", 0.65), City("Las Vegas", 0.66)
                    },
                    new[]
                    {
                    Name("Liam", 2.3), Name("Noah", 2.1), Name("Oliver", 1.85), Name("James", 1.8), Name("Elijah", 1.55),
                    Name("Mateo", 1.2), Name("Lucas", 1.35), Name("Benjamin", 1.3), Name("Theodore", 1.0), Name("Henry", 0.95),
                    Name("William", 0.9), Name("Jack", 0.85), Name("Leo", 0.75), Name("Ethan", 0.82), Name("Mason", 0.8),
                    Name("Logan", 0.78), Name("Jacob", 0.76), Name("Michael", 0.73), Name("Daniel", 0.7), Name("Jackson", 0.68),
                    Name("Aiden", 0.66), Name("Santiago", 0.6), Name("Sebastian", 0.64), Name("Alexander", 0.7), Name("David", 0.62),
                    Name("Muhammad", 0.35), Name("Jose", 0.32), Name("Jayden", 0.42), Name("Carter", 0.5), Name("Julian", 0.48),
                    Name("John", 0.4), Name("Luke", 0.46), Name("Owen", 0.52), Name("Wyatt", 0.41), Name("Samuel", 0.47),
                    Name("Levi", 0.44), Name("Isaac", 0.39), Name("Asher", 0.45), Name("Ezra", 0.33), Name("Gabriel", 0.43),
                    Name("Anthony", 0.38), Name("Christopher", 0.31), Name("Dylan", 0.34), Name("Nathan", 0.37), Name("Thomas", 0.36),
                    Name("Mila", 1.7), Name("Olivia", 2.2), Name("Emma", 2.0), Name("Charlotte", 1.7), Name("Amelia", 1.5),
                    Name("Sophia", 1.4), Name("Isabella", 1.25), Name("Evelyn", 1.05), Name("Ava", 1.1), Name("Sofia", 0.85),
                    Name("Camila", 0.82), Name("Harper", 0.8), Name("Luna", 0.79), Name("Gianna", 0.58), Name("Elizabeth", 0.5),
                    Name("Eleanor", 0.55), Name("Ella", 0.6), Name("Abigail", 0.52), Name("Emily", 0.54), Name("Scarlett", 0.64),
                    Name("Victoria", 0.48), Name("Aria", 0.62), Name("Samantha", 0.33), Name("Penelope", 0.57), Name("Layla", 0.59),
                    Name("Nora", 0.56), Name("Zoey", 0.43), Name("Hannah", 0.37), Name("Maya", 0.39), Name("Natalia", 0.34)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)
                    {
                        ["New York"] = CreateAdjustments(("Mateo", 1.4), ("Santiago", 1.3), ("Muhammad", 1.5), ("Jose", 1.3), ("Sofia", 1.15), ("Camila", 1.2), ("Natalia", 1.15), ("Maya", 1.1)),
                        ["Los Angeles"] = CreateAdjustments(("Mateo", 1.5), ("Santiago", 1.35), ("Sebastian", 1.25), ("Muhammad", 1.3), ("Jose", 1.25), ("Sofia", 1.2), ("Camila", 1.3), ("Natalia", 1.2)),
                        ["Miami"] = CreateAdjustments(("Mateo", 1.5), ("Santiago", 1.4), ("Sebastian", 1.25), ("Muhammad", 1.15), ("Jose", 1.2), ("Sofia", 1.3), ("Camila", 1.25), ("Natalia", 1.2)),
                        ["San Francisco"] = CreateAdjustments(("Mateo", 1.0), ("Santiago", 1.0), ("Muhammad", 1.25), ("Sofia", 1.05), ("Camila", 1.05), ("Aria", 1.1), ("Maya", 1.1))
                    }),
                ["UK"] = CreateProfile(
                    "UK",
                    new[]
                    {
                    City("London", 13.5), City("Birmingham", 1.15), City("Manchester", 0.57), City("Leeds", 0.54), City("Glasgow", 0.63),
                    City("Liverpool", 0.5), City("Bristol", 0.47), City("Sheffield", 0.56), City("Edinburgh", 0.52), City("Leicester", 0.37),
                    City("Coventry", 0.35), City("Bradford", 0.55), City("Cardiff", 0.36), City("Belfast", 0.35), City("Nottingham", 0.33)
                    },
                    new[]
                    {
                    Name("Muhammad", 1.7), Name("Noah", 1.4), Name("Oliver", 1.35), Name("George", 1.1), Name("Leo", 1.0),
                    Name("Arthur", 0.95), Name("Oscar", 0.9), Name("Harry", 0.88), Name("Charlie", 0.86), Name("Theo", 0.78),
                    Name("Jack", 0.8), Name("Freddie", 0.62), Name("Luca", 0.58), Name("Henry", 0.76), Name("Archie", 0.7),
                    Name("Ethan", 0.55), Name("Thomas", 0.52), Name("William", 0.54), Name("James", 0.65), Name("Lucas", 0.57),
                    Name("Olivia", 1.6), Name("Amelia", 1.35), Name("Isla", 1.2), Name("Lily", 1.0), Name("Ava", 0.95),
                    Name("Freya", 0.82), Name("Ivy", 0.74), Name("Florence", 0.62), Name("Mia", 0.73), Name("Sophia", 0.76),
                    Name("Grace", 0.58), Name("Ella", 0.55), Name("Evie", 0.57), Name("Rosie", 0.46), Name("Poppy", 0.5),
                    Name("Sienna", 0.44), Name("Fatima", 0.28), Name("Aisha", 0.25), Name("Elsie", 0.42), Name("Layla", 0.48),
                    Name("Mohammed", 0.24), Name("Adam", 0.41), Name("Alexander", 0.4), Name("Alfie", 0.38), Name("Max", 0.36),
                    Name("Arlo", 0.34), Name("Roman", 0.32), Name("Reuben", 0.22), Name("Finn", 0.33), Name("Harvey", 0.27),
                    Name("Mason", 0.26), Name("Harrison", 0.24), Name("Bonnie", 0.22), Name("Daisy", 0.38), Name("Phoebe", 0.28),
                    Name("Hallie", 0.18), Name("Willow", 0.31), Name("Erin", 0.19), Name("Molly", 0.25), Name("Ruby", 0.34)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)
                    {
                        ["London"] = CreateAdjustments(("Muhammad", 1.25), ("Fatima", 1.35), ("Aisha", 1.3), ("Olivia", 1.0), ("Amelia", 0.95)),
                        ["Birmingham"] = CreateAdjustments(("Muhammad", 1.15), ("Fatima", 1.2), ("Aisha", 1.15)),
                        ["Manchester"] = CreateAdjustments(("Muhammad", 1.0), ("Fatima", 1.05), ("Aisha", 1.0), ("Olivia", 0.95))
                    }),
                ["CA"] = CreateProfile(
                    "CA",
                    new[]
                    {
                    City("Toronto", 2.9), City("Montreal", 1.8), City("Calgary", 1.3), City("Ottawa", 1.0), City("Edmonton", 1.0),
                    City("Winnipeg", 0.75), City("Vancouver", 0.71), City("Mississauga", 0.72), City("Brampton", 0.66), City("Hamilton", 0.58),
                    City("Quebec City", 0.55), City("Surrey", 0.57), City("Halifax", 0.48), City("Laval", 0.44), City("London", 0.42)
                    },
                    new[]
                    {
                    Name("Noah", 1.6), Name("Liam", 1.55), Name("Oliver", 1.3), Name("William", 1.05), Name("Leo", 0.95),
                    Name("Theodore", 0.88), Name("Jack", 0.84), Name("Thomas", 0.8), Name("Lucas", 0.78), Name("Benjamin", 0.76),
                    Name("Ethan", 0.72), Name("Nathan", 0.66), Name("Henri", 0.28), Name("Olivier", 0.26), Name("Muhammad", 0.3),
                    Name("Emma", 1.45), Name("Olivia", 1.4), Name("Charlotte", 1.2), Name("Amelia", 1.0), Name("Sofia", 0.88),
                    Name("Ava", 0.84), Name("Chloe", 0.72), Name("Mia", 0.75), Name("Alice", 0.55), Name("Sophie", 0.62),
                    Name("Evelyn", 0.58), Name("Lea", 0.24), Name("Florence", 0.34), Name("Mila", 0.64), Name("Fatima", 0.18),
                    Name("Jacob", 0.58), Name("Alexandre", 0.2), Name("Logan", 0.54), Name("Felix", 0.32), Name("Mason", 0.46),
                    Name("Samuel", 0.43), Name("Jackson", 0.39), Name("Aiden", 0.37), Name("Mateo", 0.29), Name("Aria", 0.44),
                    Name("Lily", 0.42), Name("Ella", 0.45), Name("Abigail", 0.34), Name("Zoe", 0.28), Name("Claire", 0.24),
                    Name("Aurora", 0.22), Name("Jeanne", 0.12), Name("Nora", 0.38), Name("Ellie", 0.36), Name("Hannah", 0.3)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)
                    {
                        ["Montreal"] = CreateAdjustments(("Henri", 1.5), ("Olivier", 1.45), ("Lea", 1.2), ("Alice", 1.35), ("Sophie", 1.4)),
                        ["Toronto"] = CreateAdjustments(("Muhammad", 1.25), ("Fatima", 1.15)),
                        ["Brampton"] = CreateAdjustments(("Muhammad", 1.45), ("Fatima", 1.2))
                    }),
                ["AU"] = CreateProfile(
                    "AU",
                    new[]
                    {
                    City("Sydney", 5.3), City("Melbourne", 5.2), City("Brisbane", 2.6), City("Perth", 2.2), City("Adelaide", 1.4),
                    City("Gold Coast", 0.72), City("Canberra", 0.47), City("Newcastle", 0.32), City("Wollongong", 0.31), City("Geelong", 0.27),
                    City("Hobart", 0.25), City("Townsville", 0.19), City("Cairns", 0.16)
                    },
                    new[]
                    {
                    Name("Oliver", 1.65), Name("Noah", 1.5), Name("Jack", 1.2), Name("Henry", 1.1), Name("Leo", 1.0),
                    Name("Charlie", 0.92), Name("Theodore", 0.88), Name("Thomas", 0.82), Name("William", 0.8), Name("Luca", 0.72),
                    Name("Muhammad", 0.32), Name("Archie", 0.68), Name("Charlotte", 1.35), Name("Olivia", 1.3), Name("Amelia", 1.1),
                    Name("Isla", 1.0), Name("Mia", 0.92), Name("Ava", 0.84), Name("Matilda", 0.72), Name("Grace", 0.64),
                    Name("Sienna", 0.58), Name("Harper", 0.56), Name("Zara", 0.3), Name("Aisha", 0.2),
                    Name("Hudson", 0.42), Name("Hugo", 0.4), Name("Archer", 0.34), Name("Oscar", 0.62), Name("Lucas", 0.58),
                    Name("Levi", 0.33), Name("Lachlan", 0.31), Name("Xavier", 0.22), Name("Finn", 0.36), Name("Hamish", 0.18),
                    Name("Evie", 0.42), Name("Ella", 0.46), Name("Chloe", 0.38), Name("Willow", 0.32), Name("Ruby", 0.41),
                    Name("Sophie", 0.37), Name("Georgia", 0.26), Name("Lucy", 0.35), Name("Poppy", 0.24), Name("Ellie", 0.33),
                    Name("Hazel", 0.19), Name("Scarlett", 0.23), Name("Mila", 0.29), Name("Layla", 0.28), Name("Mackenzie", 0.12),
                    Name("Cooper", 0.21)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)
                    {
                        ["Sydney"] = CreateAdjustments(("Muhammad", 1.35), ("Zara", 1.15), ("Aisha", 1.2)),
                        ["Melbourne"] = CreateAdjustments(("Muhammad", 1.2), ("Zara", 1.05), ("Aisha", 1.1))
                    }),
                ["NZ"] = CreateProfile(
                    "NZ",
                    new[]
                    {
                    City("Auckland", 1.7), City("Christchurch", 0.4), City("Wellington", 0.22), City("Hamilton", 0.18), City("Tauranga", 0.16),
                    City("Dunedin", 0.13), City("Palmerston North", 0.09), City("Napier", 0.07), City("Nelson", 0.05), City("Rotorua", 0.06)
                    },
                    new[]
                    {
                    Name("Oliver", 1.55), Name("Noah", 1.4), Name("Jack", 1.2), Name("Leo", 1.0), Name("George", 0.82),
                    Name("Theodore", 0.74), Name("Wiremu", 0.25), Name("Arlo", 0.58), Name("Charlotte", 1.3), Name("Isla", 1.15),
                    Name("Amelia", 1.0), Name("Olivia", 0.95), Name("Ava", 0.84), Name("Harper", 0.66), Name("Mia", 0.7),
                    Name("Maia", 0.24), Name("Aroha", 0.16), Name("Sofia", 0.55), Name("Isabella", 0.48), Name("Ella", 0.44),
                    Name("Emily", 0.4), Name("Ruby", 0.36), Name("Willow", 0.28), Name("Evie", 0.26), Name("Luca", 0.34),
                    Name("Liam", 0.92), Name("Lucas", 0.52), Name("William", 0.46), Name("James", 0.5), Name("Hunter", 0.22),
                    Name("Niko", 0.16), Name("Rawiri", 0.14), Name("Manaia", 0.18), Name("Tama", 0.12), Name("Hemi", 0.1),
                    Name("Sienna", 0.24), Name("Aria", 0.22), Name("Millie", 0.18), Name("Lily", 0.3), Name("Grace", 0.28),
                    Name("Poppy", 0.2), Name("Ayla", 0.16), Name("Bonnie", 0.14), Name("Emma", 0.42), Name("Aaliyah", 0.12),
                    Name("Ethan", 0.48), Name("Mason", 0.36), Name("Logan", 0.34), Name("Finn", 0.32), Name("Samuel", 0.22),
                    Name("Tui", 0.08), Name("Kiri", 0.08)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)
                    {
                        ["Auckland"] = CreateAdjustments(("Wiremu", 1.1), ("Maia", 1.05)),
                        ["Rotorua"] = CreateAdjustments(("Wiremu", 1.35), ("Maia", 1.3), ("Aroha", 1.4))
                    }),
                ["SG"] = CreateProfile(
                    "SG",
                    new[]
                    {
                    City("Singapore", 100.0)
                    },
                    new[]
                    {
                    Name("Wei Jie", 1.0), Name("Jun Jie", 0.95), Name("Muhammad", 0.82), Name("Ryan", 0.78), Name("Jia Hao", 0.74),
                    Name("Ethan", 0.68), Name("Lucas", 0.62), Name("Arjun", 0.34), Name("Zi Xuan", 0.72), Name("Nur", 0.4),
                    Name("Charlotte", 0.55), Name("Olivia", 0.58), Name("Siti", 0.36), Name("Aisyah", 0.3), Name("Hui Min", 0.32),
                    Name("Jing Wen", 0.28), Name("Harini", 0.18), Name("Kayla", 0.25), Name("Aarav", 0.22), Name("Daniel", 0.46),
                    Name("Jayden", 0.38), Name("Xavier", 0.3), Name("Darren", 0.26), Name("Bryan", 0.22), Name("Jovan", 0.12),
                    Name("Anya", 0.18), Name("Chloe", 0.28), Name("Grace", 0.3), Name("Hannah", 0.24), Name("Mei Lin", 0.2),
                    Name("Yu Xuan", 0.18), Name("Jia Yi", 0.24), Name("Nurul", 0.16), Name("Nur Aisyah", 0.14), Name("Ishaan", 0.12),
                    Name("Pranav", 0.1), Name("Alicia", 0.22), Name("Shao Xuan", 0.12), Name("Syafiq", 0.11), Name("Adam", 0.2),
                    Name("Aiden", 0.24), Name("Noah", 0.32), Name("Shawn", 0.18), Name("Marcus", 0.17), Name("Zhi Hao", 0.16),
                    Name("Xin Yi", 0.15), Name("Pei Qi", 0.12), Name("Claire", 0.14), Name("Theo", 0.13), Name("Nurin", 0.1)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)),
                ["IE"] = CreateProfile(
                    "IE",
                    new[]
                    {
                    City("Dublin", 1.45), City("Cork", 0.24), City("Limerick", 0.1), City("Galway", 0.09), City("Waterford", 0.06),
                    City("Drogheda", 0.04), City("Swords", 0.04), City("Dundalk", 0.04), City("Bray", 0.03), City("Kilkenny", 0.03)
                    },
                    new[]
                    {
                    Name("Jack", 1.4), Name("Noah", 1.2), Name("James", 1.0), Name("Conor", 0.74), Name("Rian", 0.52),
                    Name("Tadhg", 0.42), Name("Cillian", 0.46), Name("Fionn", 0.34), Name("Emily", 1.1), Name("Grace", 1.0),
                    Name("Fiadh", 0.86), Name("Sophie", 0.8), Name("Aoife", 0.72), Name("Saoirse", 0.5), Name("Clodagh", 0.3),
                    Name("Aisha", 0.14), Name("Muhammad", 0.18), Name("Luca", 0.28), Name("Charlie", 0.36), Name("Oisin", 0.34),
                    Name("Darragh", 0.24), Name("Finn", 0.26), Name("Seán", 0.22), Name("Páidí", 0.14), Name("Donnacha", 0.12),
                    Name("Ava", 0.46), Name("Olivia", 0.44), Name("Éabha", 0.38), Name("Sadie", 0.3), Name("Anna", 0.28),
                    Name("Molly", 0.24), Name("Cara", 0.18), Name("Niamh", 0.34), Name("Caoimhe", 0.36), Name("Róisín", 0.18),
                    Name("Ella", 0.32), Name("Lucy", 0.26), Name("Amelia", 0.29), Name("Hannah", 0.18), Name("Tomás", 0.16),
                    Name("Lorcan", 0.12), Name("Eoin", 0.14), Name("Odhran", 0.11), Name("Croía", 0.14), Name("Laoise", 0.12),
                    Name("Mia", 0.22), Name("Ellie", 0.2), Name("Rory", 0.14), Name("Senan", 0.1), Name("Éabha-Rose", 0.09)
                    },
                    new Dictionary<string, IReadOnlyDictionary<string, double>>(NameComparer)
                    {
                        ["Dublin"] = CreateAdjustments(("Aisha", 1.25), ("Muhammad", 1.2))
                    })
            });

        /// <summary>
        /// Generates distinct first-name and city pairs for a supported country.
        /// </summary>
        public static IReadOnlyList<FirstNameCityPair> GenerateFirstNameCityPairs(string countryCode, int count, Random? random = null)
        {
            if (String.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException("Country code must be provided.", nameof(countryCode));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be a positive number.");
            }

            if (!CountryProfiles.TryGetValue(countryCode, out CountryProfile? profile))
            {
                throw new ArgumentException("Supported countries are US, UK, CA, AU, NZ, SG, and IE.", nameof(countryCode));
            }

            if (count > profile.Names.Count())
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, $"Count cannot exceed {profile.Names.Count()} unique first names for {profile.CountryCode}.");
            }

            random ??= Random.Shared;
            HashSet<string> usedNames = new(NameComparer);
            List<FirstNameCityPair> pairs = new(count);

            while (pairs.Count < count)
            {
                WeightedValue<string> city = SelectWeighted(profile.Cities, random);
                IReadOnlyList<WeightedValue<string>> availableNames = GetNamesForCity(profile, city.Value)
                    .Where(name => !usedNames.Contains(name.Value))
                    .ToArray();

                if (availableNames.Count == 0)
                {
                    break;
                }

                WeightedValue<string> name = SelectWeighted(availableNames, random);

                usedNames.Add(name.Value);
                pairs.Add(new FirstNameCityPair(name.Value, city.Value));
            }

            return pairs.Count != count
                ? throw new InvalidOperationException($"Unable to generate {count} distinct first names for {profile.CountryCode} with the configured weights.")
                : (IReadOnlyList<FirstNameCityPair>)pairs;
        }

        private static CountryProfile CreateProfile(
            string countryCode,
            IReadOnlyList<WeightedValue<string>> cities,
            IReadOnlyList<WeightedValue<string>> names,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> cityNameAdjustments)
        {
            if (String.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException("Country code must be provided.", nameof(countryCode));
            }

            ArgumentNullException.ThrowIfNull(cities);
            ArgumentNullException.ThrowIfNull(names);
            ArgumentNullException.ThrowIfNull(cityNameAdjustments);

            foreach (KeyValuePair<string, IReadOnlyDictionary<string, double>> adjustment in cityNameAdjustments)
            {
                foreach (string adjustedName in adjustment.Value.Keys)
                {
                    if (!names.Any(name => NameComparer.Equals(name.Value, adjustedName)))
                    {
                        throw new InvalidOperationException($"City adjustment weight for {countryCode}/{adjustment.Key}/{adjustedName} does not match a configured name.");
                    }
                }
            }

            return new CountryProfile(countryCode, cities, names, cityNameAdjustments);
        }

        private static IReadOnlyList<WeightedValue<string>> GetNamesForCity(CountryProfile profile, string city)
        {
            if (!profile.CityNameAdjustments.TryGetValue(city, out IReadOnlyDictionary<string, double>? multipliers))
            {
                return profile.Names;
            }

            IReadOnlyList<WeightedValue<string>> names = profile.Names;
            int nameCount = names.Count();
            WeightedValue<string>[] adjusted = new WeightedValue<string>[nameCount];
            for (int index = 0; index < nameCount; index++)
            {
                double multiplier = multipliers.TryGetValue(names[index].Value, out double adjustedWeight) ? adjustedWeight : 1.0;
                adjusted[index] = new WeightedValue<string>(names[index].Value, names[index].Weight * multiplier);
            }

            return adjusted;
        }

        private static WeightedValue<string> SelectWeighted(IReadOnlyList<WeightedValue<string>> values, Random random)
        {
            ArgumentNullException.ThrowIfNull(values);
            ArgumentNullException.ThrowIfNull(random);

            double totalWeight = values.Sum(item => item.Weight);
            double selectedWeight = random.NextDouble() * totalWeight;

            foreach (WeightedValue<string> value in values)
            {
                selectedWeight -= value.Weight;
                if (selectedWeight <= 0)
                {
                    return value;
                }
            }

            return values[^1];
        }

        private static WeightedValue<string> City(string name, double percentage) => new(name, percentage);

        private static WeightedValue<string> Name(string name, double popularityWeight) => new(name, popularityWeight);

        private static IReadOnlyDictionary<string, double> CreateAdjustments(params (string Name, double Multiplier)[] adjustments)
        {
            ArgumentNullException.ThrowIfNull(adjustments);

            Dictionary<string, double> values = new(NameComparer);
            foreach ((string name, double multiplier) in adjustments)
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Adjusted name must be provided.", nameof(adjustments));
                }

                values[name] = multiplier;
            }

            return new ReadOnlyDictionary<string, double>(values);
        }

        private sealed record WeightedValue<T>(T Value, double Weight);

        private sealed class CountryProfile
        {
            public CountryProfile(
                string countryCode,
                IReadOnlyList<WeightedValue<string>> cities,
                IReadOnlyList<WeightedValue<string>> names,
                IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> cityNameAdjustments)
            {
                this.CountryCode = countryCode;
                this.Cities = cities.ToArray();
                this.Names = names.ToArray();
                this.CityNameAdjustments = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, double>>(new Dictionary<string, IReadOnlyDictionary<string, double>>(cityNameAdjustments, NameComparer));
            }

            public string CountryCode { get; }

            public IReadOnlyList<WeightedValue<string>> Cities { get; }

            public IReadOnlyList<WeightedValue<string>> Names { get; }

            public IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> CityNameAdjustments { get; }
        }
    }
}
