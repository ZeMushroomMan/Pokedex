using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;


namespace Pokedex_Blazor.Pages
{
    public partial class Pokedex : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; }
        private static readonly HttpClient client = new HttpClient();

        private string searchText = "";
        private bool showFilterPanel = false;
        private bool showComparePanel = false;

        private List<PokemonResult> names = new List<PokemonResult>(); // List of pokemon that gets displayed in the UI. Update this if you want to display any pokemon names to the UI.
        private List<PokemonResult> allTypes = new List<PokemonResult>();
        private List<PokemonResult> filteredList; //List of filtered pokemon by category to be saved for searching function.

        private List<FilterCategory> filterCategories = new List<FilterCategory> // Categeries to filter by
    {
        new FilterCategory { key = "type", name = "Type" },
        new FilterCategory { key = "ability", name = "Ability" },
        new FilterCategory { key = "color", name = "Color" },
        new FilterCategory { key = "shape", name = "Shape" },
        new FilterCategory { key = "habitat", name = "Habitat" },
        new FilterCategory { key = "generation", name = "Generation" }
    };

        private FilterCategory selectedCategory;
        private string selectedValue;
        private List<PokemonResult> filterValues;
        private PokemonDetails? selectedPokemon;

        private PokemonDetails? comparePokemon1;
        private PokemonDetails? comparePokemon2;
        private PokemonDetails winner;

        //constructor that runs at start of the page. Passes the url to make the API call to, to fetch names and urls of all pokemon currently on the API and saves it into a List
        protected override async Task OnInitializedAsync()
        {
            await LoadAllPokemon("https://pokeapi.co/api/v2/pokemon?offset=0&limit=200000");
            Console.WriteLine("Begin");
        }

        // Load entire list of Pokémon from the api call and caches it for future calls. Saves all the data retrieved into a list.
        private async Task<PokemonListResponse> LoadAllPokemon(string url)
        {
            try
            {
                var cached = await LoadCacheAsync<PokemonListResponse>(url);
                if (cached != null && cached.results?.Count > 0)
                {
                    names = cached.results;
                    return cached;
                }

                var response = await client.GetFromJsonAsync<PokemonListResponse>(url);
                if (response != null)
                {
                    names = response.results;
                    await SaveCacheAsync(url, response);
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        //Loads the selected pokemons data into a panel on the left like an old pokedex
        private async Task<PokemonDetails> LoadPokemonDetails(string url)
        {
            try
            {
                
                var cache = await LoadCacheAsync<PokemonDetails>(url);
                selectedPokemon = cache;
                if(selectedPokemon != null)
                {
                    if (showComparePanel && (comparePokemon2 is null))
                    {
                        comparePokemon2 = selectedPokemon;
                    }
                    else if (showComparePanel && comparePokemon1 is null)
                    {
                        comparePokemon1 = selectedPokemon;
                    }
                    if (comparePokemon1 != null && comparePokemon2 != null)
                    {
                        ComparePokemon(comparePokemon1, comparePokemon2);
                    }
                    return cache;
                }
                if (selectedPokemon == null)
                {
                    var data = await Http.GetFromJsonAsync<PokemonDetails>(url);
                    selectedPokemon = data;
                    await SaveCacheAsync(url, selectedPokemon);
                    if (showComparePanel && (comparePokemon1 is null))
                    {
                        comparePokemon1 = selectedPokemon;
                    }
                    else if (showComparePanel && comparePokemon2 is null)
                    {
                        comparePokemon2 = selectedPokemon;
                        
                    }
                    if (comparePokemon1 != null && comparePokemon2 != null)
                    {
                        ComparePokemon(comparePokemon1, comparePokemon2);
                    }
                    return data;
                }
                    return null;
            }
            catch (Exception ex) { Console.WriteLine(ex);
                return null;
            }
        }
        private void Clear()
        {
            selectedPokemon = null;
            comparePokemon1 = null;
            comparePokemon2 = null;
        }

        // Caching functions
        #region Caching
        //Saves current Api call into a local browser cache to prevent excessive api calls for every single page load and action. This function is called in every other function that uses any API calls. 
        //Stores data and uses the url for the api call as the key so if the same url is passed it can fetch the cached data first
        private async Task SaveCacheAsync<T>(string key, T data)
        {
            var json = JsonSerializer.Serialize(data);
            await JS.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        //Loads the cached data from the url key stored
        private async Task<T> LoadCacheAsync<T>(string key)
        {
            var json = await JS.InvokeAsync<string>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }
        #endregion

        // Filtering
        #region Filtering
        //Fetches all values inside each category to filter by and populates the drop down
        private async Task FilterCategoryChanged(ChangeEventArgs e)
        {
            try
            {
                var selectedKey = e.Value?.ToString();
                selectedCategory = filterCategories.FirstOrDefault(c => c.key == selectedKey);
                selectedValue = null;
                filterValues = null;

                if (string.IsNullOrEmpty(selectedKey))
                {
                    await LoadAllPokemon("https://pokeapi.co/api/v2/pokemon?offset=0&limit=200000");
                    return;
                }



                string url = selectedCategory.key switch
                {
                    "type" => "https://pokeapi.co/api/v2/type",
                    "ability" => "https://pokeapi.co/api/v2/ability",
                    "color" => "https://pokeapi.co/api/v2/pokemon-color",
                    "shape" => "https://pokeapi.co/api/v2/pokemon-shape",
                    "habitat" => "https://pokeapi.co/api/v2/pokemon-habitat",
                    "generation" => "https://pokeapi.co/api/v2/generation",
                    _ => ""
                };

                if (string.IsNullOrEmpty(url)) return;

                var cached = await LoadCacheAsync<PokemonListResponse>(url);
                if (cached != null)
                    filterValues = cached.results;
                else
                {
                    var response = await client.GetFromJsonAsync<PokemonListResponse>(url);
                    if (response != null)
                    {
                        filterValues = response.results;
                        await SaveCacheAsync(url, response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        //Event for when the value to filter by in a specific category is changed and is then filtered by.
        private async Task FilterValueChanged(ChangeEventArgs e)
        {
            try
            {
                selectedValue = e.Value?.ToString();

                if (string.IsNullOrEmpty(selectedValue))
                {
                    await LoadAllPokemon("https://pokeapi.co/api/v2/pokemon?offset=0&limit=200000");
                    return;
                }

                if (selectedCategory != null)
                    await FilterByCategoryValue(selectedCategory.key, selectedValue);
            }
            catch (Exception ex) { Console.WriteLine(ex); return; }
        }


        //The logic of actually making the api request to filter the category by specific type and values callied in the event changed method above.
        private async Task FilterByCategoryValue(string categoryKey, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    await LoadAllPokemon("https://pokeapi.co/api/v2/pokemon?offset=0&limit=200000");
                    filteredList = new List<PokemonResult>(names);
                    return;
                }

                string url = categoryKey switch
                {
                    "type" => $"https://pokeapi.co/api/v2/type/{value}",
                    "ability" => $"https://pokeapi.co/api/v2/ability/{value}",
                    "color" => $"https://pokeapi.co/api/v2/pokemon-color/{value}",
                    "shape" => $"https://pokeapi.co/api/v2/pokemon-shape/{value}",
                    "habitat" => $"https://pokeapi.co/api/v2/pokemon-habitat/{value}",
                    "generation" => $"https://pokeapi.co/api/v2/generation/{value}",
                    _ => ""
                };

                if (string.IsNullOrEmpty(url)) return;

                List<PokemonResult> filtered;

                if (categoryKey == "type" || categoryKey == "ability")
                {
                    var cached = await LoadCacheAsync<CategoryResult>(url);
                    if (cached != null)
                        filtered = cached.pokemon.Select(p => p.pokemon).ToList();
                    else
                    {
                        var response = await client.GetFromJsonAsync<CategoryResult>(url);
                        filtered = response.pokemon.Select(p => p.pokemon).ToList();
                        await SaveCacheAsync(url, response);
                    }
                }
                else
                {
                    var cached = await LoadCacheAsync<PokemonSpecies>(url);
                    if (cached != null)
                        filtered = cached.pokemon_species
                            .Select(p => new PokemonResult { name = p.name, url = $"https://pokeapi.co/api/v2/pokemon/{p.name}" })
                            .ToList();
                    else
                    {
                        var response = await client.GetFromJsonAsync<PokemonSpecies>(url);
                        filtered = response.pokemon_species
                            .Select(p => new PokemonResult { name = p.name, url = $"https://pokeapi.co/api/v2/pokemon/{p.name}" })
                            .ToList();
                        await SaveCacheAsync(url, response);
                    }
                }

                names = filtered.Select(p =>
                {
                    if (string.IsNullOrEmpty(p.url))
                        p.url = $"https://pokeapi.co/api/v2/pokemon/{p.name}";
                    return p;
                }).ToList();

                filteredList = new List<PokemonResult>(names);
                StateHasChanged();
            }
            catch (Exception ex) { Console.WriteLine(ex); return; }
        }

        //Simple switch to toggle the filter panel visibility
        private void ToggleFilterPanel()
        {
            showFilterPanel = !showFilterPanel;
        }

        //The logic of searching the current data displayed. Stores old values in a list and then applies the search criteria to a new copy of that list and searchs through it. When search bar is cleared returns old list.
        private void SearchPokemon(ChangeEventArgs e)
        {
            try
            {
                var searchValue = e.Value?.ToString() ?? "";

                if (filteredList == null)
                    filteredList = new List<PokemonResult>(names);

                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    names = new List<PokemonResult>(filteredList);
                    return;
                }

                names = filteredList
                    .Where(p => p.name.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (Exception ex) { Console.WriteLine(ex); return; }
        }
        #endregion

        //Comparison Logic

        //Function to compare two types and return an affectiveness multiplier
        public double GetFullTypeEffectiveness(List<TypeList> attackerTypes, List<TypeList> defenderTypes)
        {
            double multiplier = 1.0;

            int i, j;
            for (i = 0; i < attackerTypes.Count; i++)
            {
                string atkType = attackerTypes[i].type.name.ToLower();

                for (j = 0; j < defenderTypes.Count; j++)
                {
                    string defType = defenderTypes[j].type.name.ToLower();

                    double typeMultiplier = 1.0;
                    if (TypeChart.Effectiveness.ContainsKey(atkType))
                    {
                        Dictionary<string, double> chart = TypeChart.Effectiveness[atkType];
                        if (chart.ContainsKey(defType))
                        {
                            typeMultiplier = chart[defType];
                        }
                    }

                    multiplier *= typeMultiplier; 
                }
            }

            return multiplier;
        }

        //Function that takes two pokemon and compares them on an average of their stats and their attack multiplier agaisnt their type attributes. 
        public void ComparePokemon(PokemonDetails p1, PokemonDetails p2)
        {
            try
            {
                //Attack with type multiplier
                int attack1 = GetStat(p1, "attack");
                int attack2 = GetStat(p2, "attack");

                double attack1Modified = attack1 * GetFullTypeEffectiveness(p1.types, p2.types);
                double attack2Modified = attack2 * GetFullTypeEffectiveness(p2.types, p1.types);

                //Sum all other stats normally
                int total1 = CalculateBaseStatTotal(p1) - attack1 + ((int)attack1Modified);

                int total2 = CalculateBaseStatTotal(p2) - attack2 + ((int)attack2Modified);

                if (total1 > total2)
                {
                    winner = p1;
                    Console.WriteLine("HERE");
                }
                else if (total2 > total1)
                {
                    winner = p2;
                    Console.WriteLine(" ");
                }
                else
                {
                    winner = null;
                    Console.WriteLine("HERE 2");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                winner = null;
            }

        }

        //Fetches a single stat from the pokemons stat list
        public int GetStat(PokemonDetails p, string statName)
        {
            for (int i = 0; i < p.stats.Count; i++)
            {
                if (p.stats[i].stat.name.Equals(statName, StringComparison.OrdinalIgnoreCase))
                    return p.stats[i].base_stat;
            }
            return 0;
        }

        //Averages all stats of a pokemon into one base stat average
        public int CalculateBaseStatTotal(PokemonDetails p)
        {
            int total = 0;
            for (int i = 0; i < p.stats.Count; i++)
            {
                total += p.stats[i].base_stat;
            }
            return total;
        }


        private void ToggleComparePanel()
        {
            showComparePanel = !showComparePanel;
            if (!showComparePanel)
            {
                comparePokemon1 = null;
                comparePokemon2 = null;
                selectedPokemon = null;
            }
        }



        // Data Models
        #region Data Models
        public class PokemonListResponse
        {
            public List<PokemonResult> results { get; set; }
            public string next { get; set; }
            public string previous { get; set; }
        }

        public class PokemonResult
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        public class CategoryResult
        {
            public List<CategoryPokemonEntry> pokemon { get; set; }
        }

        public class CategoryPokemonEntry
        {
            public PokemonResult pokemon { get; set; }
            public int slot { get; set; }
        }

        public class FilterCategory
        {
            public string key { get; set; }
            public string name { get; set; }
        }

        public class PokemonSpecies
        {
            public List<PokemonResult> pokemon_species { get; set; }
        }
        public class PokemonDetails
        {
            public int id { get; set; }
            public string name { get; set; }
            public int height { get; set; }
            public int weight { get; set; }
            public Sprites sprites { get; set; }
            public List<TypeList> types { get; set; }
            public List<StatEntry> stats { get; set; }
            public List<AbilityEntry> abilities { get; set; }
        }

        public class TypeList
        {
            public TypeInfo type { get; set; }
        }

        public class TypeInfo
        {
            public string name { get; set; }
        }

        public class Sprites
        {
            public string front_default { get; set; }
        }

        public class StatEntry
        {
            public int base_stat { get; set; }
            public int effort { get; set; }
            public EntryInfo stat { get; set; }
        }

        public class AbilityEntry
        {
            public EntryInfo ability { get; set; }
            public bool is_hidden { get; set; }
            public int slot { get; set; }
        }

        public class EntryInfo
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        //Dictionary to store all the different affectiveness multipliers of the different pokemon types
        public static class TypeChart
        {
            public static Dictionary<string, Dictionary<string, double>> Effectiveness = new Dictionary<string, Dictionary<string, double>>()
    {
        { "normal", new Dictionary<string, double> { {"rock",0.5}, {"ghost",0}, {"steel",0.5} } },
        { "fire", new Dictionary<string, double> { {"fire",0.5}, {"water",0.5}, {"grass",2}, {"ice",2}, {"bug",2}, {"rock",0.5}, {"dragon",0.5}, {"steel",2} } },
        { "water", new Dictionary<string, double> { {"fire",2}, {"water",0.5}, {"grass",0.5}, {"ground",2}, {"rock",2}, {"dragon",0.5} } },
        { "electric", new Dictionary<string, double> { {"water",2}, {"electric",0.5}, {"grass",0.5}, {"ground",0}, {"flying",2}, {"dragon",0.5} } },
        { "grass", new Dictionary<string, double> { {"fire",0.5}, {"water",2}, {"grass",0.5}, {"poison",0.5}, {"ground",2}, {"flying",0.5}, {"bug",0.5}, {"rock",2}, {"dragon",0.5}, {"steel",0.5} } },
        { "ice", new Dictionary<string, double> { {"fire",0.5}, {"water",0.5}, {"grass",2}, {"ice",0.5}, {"ground",2}, {"flying",2}, {"dragon",2}, {"steel",0.5} } },
        { "fighting", new Dictionary<string, double> { {"normal",2}, {"ice",2}, {"rock",2}, {"dark",2}, {"steel",2}, {"poison",0.5}, {"flying",0.5}, {"psychic",0.5}, {"bug",0.5}, {"ghost",0} } },
        { "poison", new Dictionary<string, double> { {"grass",2}, {"poison",0.5}, {"ground",0.5}, {"rock",0.5}, {"ghost",0.5}, {"steel",0} } },
        { "ground", new Dictionary<string, double> { {"fire",2}, {"electric",2}, {"grass",0.5}, {"poison",2}, {"flying",0}, {"bug",0.5}, {"rock",2}, {"steel",2} } },
        { "flying", new Dictionary<string, double> { {"electric",0.5}, {"grass",2}, {"fighting",2}, {"bug",2}, {"rock",0.5}, {"steel",0.5} } },
        { "psychic", new Dictionary<string, double> { {"fighting",2}, {"poison",2}, {"psychic",0.5}, {"dark",0}, {"steel",0.5} } },
        { "bug", new Dictionary<string, double> { {"fire",0.5}, {"grass",2}, {"fighting",0.5}, {"poison",0.5}, {"flying",0.5}, {"psychic",2}, {"ghost",0.5}, {"dark",2}, {"steel",0.5}, {"fairy",0.5} } },
        { "rock", new Dictionary<string, double> { {"fire",2}, {"ice",2}, {"fighting",0.5}, {"ground",0.5}, {"flying",2}, {"bug",2}, {"steel",0.5} } },
        { "ghost", new Dictionary<string, double> { {"normal",0}, {"psychic",2}, {"ghost",2}, {"dark",0.5} } },
        { "dragon", new Dictionary<string, double> { {"dragon",2}, {"steel",0.5}, {"fairy",0} } },
        { "dark", new Dictionary<string, double> { {"fighting",0.5}, {"psychic",2}, {"ghost",2}, {"dark",0.5}, {"fairy",0.5} } },
        { "steel", new Dictionary<string, double> { {"fire",0.5}, {"water",0.5}, {"electric",0.5}, {"ice",2}, {"rock",2}, {"steel",0.5}, {"fairy",2} } },
        { "fairy", new Dictionary<string, double> { {"fire",0.5}, {"fighting",2}, {"dragon",2}, {"dark",2}, {"poison",0.5}, {"steel",0.5} } }
    };
        }
        #endregion
    }
}
